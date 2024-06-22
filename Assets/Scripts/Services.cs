using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using Vuforia;
using Image = Vuforia.Image;
using System.Text;
using System.Threading;
using TMPro;

[System.Serializable]
public class Position
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class QueueMessage
{
    public string uuid;
    public string queue;
}

public class Services : MonoBehaviour
{
    public GameObject[] organs; 
    const PixelFormat PIXEL_FORMAT = PixelFormat.RGB888;
    const TextureFormat TEXTURE_FORMAT = TextureFormat.RGB24;

    private  Texture2D mTexture;
    private bool mFormatRegistered;
    private TcpClient client;
    private NetworkStream stream;

    private int port;
    private string serverAddress;
    private bool isConnecting = false;
    private bool isConnected = false;


    private float reconnectInterval = 5f; // Seconds to wait before trying to reconnect
    private Coroutine reconnectCoroutine;
    SaveLoadManager saveLoadManager = new SaveLoadManager();

    private Popups popups;
    private BodyOrganManager bodyOrganManager;

    void Start()
    {

        ServerData serverData = saveLoadManager.LoadServerData();

        serverAddress = serverData.serverAddress;
        port = int.Parse(serverData.serverPort);    


        ConnectToServer();
        
       // Register Vuforia Engine life-cycle callbacks:
        VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;
        VuforiaApplication.Instance.OnVuforiaStopped += OnVuforiaStopped;

        if (VuforiaBehaviour.Instance != null)
            VuforiaBehaviour.Instance.World.OnStateUpdated += OnVuforiaUpdated;
    }


    void Update()
    {
        if (!isConnected && !isConnecting)
        {
            ConnectToServer();
        }

        if (stream != null && stream.DataAvailable)
        {
            popups =  FindAnyObjectByType<Popups>();
            bodyOrganManager = FindAnyObjectByType<BodyOrganManager>();
            TMP_Text server_status_text = popups.ServerStatusText;
            try 
            {
                byte[] lengthPrefix = new byte[4];
                stream.Read(lengthPrefix, 0, 4);
                int dataLength = BitConverter.ToInt32(lengthPrefix, 0);

                byte[] data = new byte[dataLength];
                stream.Read(data, 0, dataLength);

                string json = Encoding.UTF8.GetString(data);
                Position position = JsonUtility.FromJson<Position>(json);
                
                if(position != null){
                    if(!bodyOrganManager.enableZooming){
                       // Vector3 vectorPosition = JsonUtility.FromJson<Vector3>(json);
                        Vector3 vectorPosition = new Vector3(position.x, position.y, position.z);
                        UpdateVectorPosition(vectorPosition);
                    }


                        
                    // Vector3 vectorPosition = JsonUtility.FromJson<Vector3>(json);
                   
                }
                
                QueueMessage queueMessage = JsonUtility.FromJson<QueueMessage>(json);

                if(queueMessage != null){
                    PlayerData playerData = saveLoadManager.LoadPlayerData();
                    TMP_Text tmp_queue_text = popups.QueueingMessageText;

                    if(playerData != null && playerData.playerUUID == queueMessage.uuid){
                        tmp_queue_text.text = queueMessage.queue;
                    }else{
                        tmp_queue_text.text = "";
                    }
                }
                
            } 
            catch (Exception e) 
            {
                Debug.LogError("Error reading from stream: " + e.Message);
                isConnected = false;
                StopCoroutine(reconnectCoroutine);
                reconnectCoroutine = StartCoroutine(ReconnectToServer(server_status_text));
            } 

        }
    }

    void SendJsonMessage()
    {
        try
        {
            PlayerData playerData = saveLoadManager.LoadPlayerData();
            BodyOrganName bodyOrganName = saveLoadManager.LoadBodyOrganName();

     
            string uuids = playerData.playerUUID;
            string typeSelected = bodyOrganName.name.ToLower();

            string json = $"{{\"uuid\":\"{uuids}\",\"message\":\"{typeSelected}\"}}"; // $"{{\"uuid\":\"{uuids}\",\"message\":\"{typeSelected}\"}}";
            byte[] data = Encoding.UTF8.GetBytes(json);
            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

            stream.Write(lengthPrefix, 0, lengthPrefix.Length);
            stream.Write(data, 0, data.Length);

            // Debug.Log("JSON message sent to server.");  

        }
        catch (Exception e)
        {
            Debug.LogError("Failed to send JSON message: " + e.Message);
            isConnected = false;
            if (reconnectCoroutine == null)
            {
                popups =  FindAnyObjectByType<Popups>();
                TMP_Text server_status_text = popups.ServerStatusText;
                reconnectCoroutine = StartCoroutine(ReconnectToServer(server_status_text));
            }
        }
    }



    void ConnectToServer(){
       
        if (isConnecting) return;

        isConnecting = true;
        popups =  FindAnyObjectByType<Popups>();
        TMP_Text server_status_text = popups.ServerStatusText;

        Debug.Log("Attempting to connect to server...");
        try
        {
            client = new TcpClient(serverAddress, port);
            stream = client.GetStream();
            isConnecting = false;
            isConnected = true;

            Debug.Log("Connected to Python server.");
            server_status_text.text = Store.ServerOnline;
            if (reconnectCoroutine != null)
            {
                StopCoroutine(reconnectCoroutine);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Connection failed: " + e.Message);
            isConnecting = false;
            if (reconnectCoroutine == null)
            {
                reconnectCoroutine = StartCoroutine(ReconnectToServer(server_status_text));
            }
        }
    }

    IEnumerator ReconnectToServer(TMP_Text server_status_text)
    {
        while (!isConnected)
        {
            server_status_text.text = "Reconnecting back to the server....";
            Debug.Log("Reconnecting to server...");
            yield return new WaitForSeconds(reconnectInterval);

            try
            {
                client = new TcpClient(serverAddress, port);
                stream = client.GetStream();
                isConnected = true;
                server_status_text.text = Store.ServerOnline;
                Debug.Log("Reconnected to Python server.");
            }
            catch (Exception e)
            {
                Debug.LogError("Reconnection failed: " + e.Message);
                server_status_text.text = "Reconnection Failed...";
            }
        }

        reconnectCoroutine = null;
    }
    void UpdateVectorPosition(Vector3 vectorPosition)
    {
        // Example: Update a UI element or game object with the new position
        foreach(GameObject organ in organs){
            if(organ.activeSelf){
                organ.transform.localPosition = vectorPosition;
            }
        }
        // cube.transform.localPosition = vectorPosition;
        // cube.SetActive(true);
        // Implement logic to update your Unity scene with the heart position
    }

    void OnDestroy()
    {
        // Unregister Vuforia Engine life-cycle callbacks:
        if (VuforiaBehaviour.Instance != null)
            VuforiaBehaviour.Instance.World.OnStateUpdated -= OnVuforiaUpdated;

        VuforiaApplication.Instance.OnVuforiaStarted -= OnVuforiaStarted;
        VuforiaApplication.Instance.OnVuforiaStopped -= OnVuforiaStopped;

        if (VuforiaApplication.Instance.IsRunning)
        {
            UnregisterFormat();
        }

        if (mTexture != null)
            Destroy(mTexture);

        // Close the TCP connection
        if (stream != null)
            stream.Close();
        if (client != null)
            client.Close();
            
        if (reconnectCoroutine != null)
        {
            StopCoroutine(reconnectCoroutine);
        }
    }

    void OnVuforiaStarted()
    {
        mTexture = new Texture2D(0, 0, TEXTURE_FORMAT, false);
        RegisterFormat();
    }

    void OnVuforiaStopped()
    {
        UnregisterFormat();
        if (mTexture != null)
            Destroy(mTexture);
    }
    void OnVuforiaUpdated()
    {
        var image = VuforiaBehaviour.Instance.CameraDevice.GetCameraImage(PIXEL_FORMAT);

        if (Image.IsNullOrEmpty(image))
            return;

        // Debug.Log("\nImage Format: " + image.PixelFormat +
        //           "\nImage Size: " + image.Width + " x " + image.Height +
        //           "\nBuffer Size: " + image.BufferWidth + " x " + image.BufferHeight +
        //           "\nImage Stride: " + image.Stride + "\n");

        // Update the texture with the new frame
        image.CopyToTexture(mTexture, true);
        FlipTextureVertically(mTexture);
        // Send the frame to the Python server

        PlayerData playerData = saveLoadManager.LoadPlayerData();
        BodyOrganName bodyOrganName = saveLoadManager.LoadBodyOrganName();

        if(playerData != null && bodyOrganName != null){
            SendJsonMessage();
            SendFrameToServer(mTexture);
            
        }
        Thread.Sleep(30);
    }

    void RegisterFormat()
    {
        var success = VuforiaBehaviour.Instance.CameraDevice.SetFrameFormat(PIXEL_FORMAT, true);
        if (success)
        {
            // Debug.Log("Successfully registered pixel format " + PIXEL_FORMAT);
            mFormatRegistered = true;
        }
        else
        {
            // Debug.LogError("Failed to register pixel format " + PIXEL_FORMAT +
            //                "\n the format may be unsupported by your device;" +
            //                "\n consider using a different pixel format.");
            mFormatRegistered = false;
        }
    }

    void UnregisterFormat()
    {
        Debug.Log("Unregistering camera pixel format " + PIXEL_FORMAT);
        VuforiaBehaviour.Instance.CameraDevice.SetFrameFormat(PIXEL_FORMAT, false);
        mFormatRegistered = false;
    }

    void SendFrameToServer(Texture2D texture)
    {
        try
        {
            byte[] frameBytes = texture.EncodeToJPG();
            byte[] lengthPrefix = BitConverter.GetBytes(frameBytes.Length);

            stream.Write(lengthPrefix, 0, lengthPrefix.Length);
            stream.Write(frameBytes, 0, frameBytes.Length);

            // Debug.Log("Frame sent to server.");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to send frame: " + e.Message);
            isConnected = false;
            if (reconnectCoroutine == null)
            {
                popups =  FindAnyObjectByType<Popups>();
                TMP_Text server_status_text = popups.ServerStatusText;
                reconnectCoroutine = StartCoroutine(ReconnectToServer(server_status_text));
            }
        }
    }
    void FlipTextureVertically(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();
        Color[] flippedPixels = new Color[pixels.Length];
        int width = texture.width;
        int height = texture.height;

        for (int y = 0; y < height; y++)
        {
            int flippedY = height - y - 1;
            Array.Copy(pixels, y * width, flippedPixels, flippedY * width, width);
        }

        texture.SetPixels(flippedPixels);
        texture.Apply();
    }
}
