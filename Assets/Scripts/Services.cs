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
public class PopupMessage
{
    public string uuid;
    public string message;
}


[Serializable]
public class PositionRotation
{
    public float positionX;
    public float positionY;
    public float positionZ;

    public float rotationX;
    public float rotationY;
    public float rotationZ;
}

public class Services : MonoBehaviour
{
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


    private float reconnectInterval = 5f; 
    private Coroutine reconnectCoroutine;
    readonly SaveLoadManager saveLoadManager = new SaveLoadManager();

    private Popups popups;
    private BodyOrganManager bodyOrganManager;

    public TMP_Dropdown UserRole;
    private string userRoleStr = "";

    private Vector3 previousPosition = Vector3.zero;
    private Quaternion previousRotation = Quaternion.identity;

    void Start()
    {
        TMP_Dropdown role = UserRole;
        userRoleStr = role.options[role.value].text;

        PlayerData playerData = saveLoadManager.LoadPlayerData();
        BodyOrganName bodyOrganName = saveLoadManager.LoadBodyOrganName();
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

        TMP_Dropdown role = UserRole;
        userRoleStr = role.options[role.value].text;
    
        ConnectToServer();
       

        if (isConnected) {
            // ping server
            StartCoroutine(PingServer());

            if (stream != null && stream.DataAvailable) 
            {
                string jsonStream = ReadJsonStream();

                PopupMessage popupMessage = JsonUtility.FromJson<PopupMessage>(jsonStream);
                if (userRoleStr == "Host")
                {
                    if (!string.IsNullOrEmpty(jsonStream))
                    {
                        // Debug.Log(jsonStream);
                        if (popupMessage != null) 
                        {

                            PlayerData playerData = saveLoadManager.LoadPlayerData();
                            TMP_Text tmp_queue_text = popups.QueueingMessageText;

                            if (playerData != null && playerData.playerUUID == popupMessage.uuid) {
                                tmp_queue_text.text = popupMessage.message;
                            } else {
                                tmp_queue_text.text = "";
                            }
                        }

                        Position position = JsonUtility.FromJson<Position>(jsonStream);
                        if (position != null) 
                        {
                            if (!bodyOrganManager.enableZooming)
                            {
                                // Vector3 vectorPosition = JsonUtility.FromJson<Vector3>(json);
                                Vector3 vectorPosition = new Vector3(position.x, position.y, position.z);
                                UpdateVectorPosition(vectorPosition);
                            }
                            // Vector3 vectorPosition = JsonUtility.FromJson<Vector3>(json);
                        }

                        // popups =  FindAnyObjectByType<Popups>();
                        // bodyOrganManager = FindAnyObjectByType<BodyOrganManager>();
                        // TMP_Text server_status_text = popups.ServerStatusText;

                    }
                }
                else if ( userRoleStr == "Guest")
                {
                    // Debug.Log(jsonStream);
                    if (!string.IsNullOrEmpty(jsonStream))
                    {
                        if (popupMessage != null) 
                        {

                            PlayerData playerData = saveLoadManager.LoadPlayerData();
                            TMP_Text tmp_queue_text = popups.QueueingMessageText;

                            if (playerData != null && playerData.playerUUID == popupMessage.uuid) {
                                tmp_queue_text.text = popupMessage.message;
                            } else {
                                tmp_queue_text.text = "";
                            }
                        }


                        PositionRotation positionRotation = JsonUtility.FromJson<PositionRotation>(jsonStream);
                        if (positionRotation != null)
                        {
                            UpdateGameObject(positionRotation);
                        }

                    }
                }

            }
        }
    }

    string ReadJsonStream() {
        try 
        {
            byte[] lengthPrefix = new byte[4];
            stream.Read(lengthPrefix, 0, 4);
            int dataLength = BitConverter.ToInt32(lengthPrefix, 0);

            byte[] data = new byte[dataLength];
            stream.Read(data, 0, dataLength);

            string json = Encoding.UTF8.GetString(data);
            return json;
        } 
        catch (Exception e) 
        {
            Debug.LogError("Error reading from stream: " + e.Message);
            return "";
        } 
    }

    void ConnectToServer() {
       
        if (!isConnected) {
            popups =  FindAnyObjectByType<Popups>();
            TMP_Text server_status_text = popups.ServerStatusText;

            Debug.Log("Attempting to connect to server...");
            try
            {
                client = new TcpClient(serverAddress, port);
                stream = client.GetStream();
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
                isConnected = false;

                if (reconnectCoroutine == null)
                {
                    reconnectCoroutine = StartCoroutine(ReconnectToServer(server_status_text));
                }
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
                isConnected = false;
                Debug.LogError("Reconnection failed: " + e.Message);
                server_status_text.text = "Reconnection Failed...";
            }
        }

        reconnectCoroutine = null;
    }

    IEnumerator PingServer()
    {
        yield return new WaitForSeconds(3f);

        try
        {
            PlayerData playerData = saveLoadManager.LoadPlayerData();
            string uuids = playerData.playerUUID;
            string json = $"{{\"uuid\":\"{uuids}\", \"role\":\"{userRoleStr}\", \"message\":\"PING\"}}"; // $"{{\"uuid\":\"{uuids}\",\"message\":\"{typeSelected}\"}}";
            byte[] data = Encoding.UTF8.GetBytes(json);
            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

            stream.Write(lengthPrefix, 0, lengthPrefix.Length);
            stream.Write(data, 0, data.Length);
            
        }
        catch (Exception e)
        {
            isConnected = false;
            Debug.LogError("Ping failed: " + e.Message);
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
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to send JSON message: " + e.Message);
            // isConnected = false;
            // if (reconnectCoroutine == null)
            // {
            //     popups =  FindAnyObjectByType<Popups>();
            //     TMP_Text server_status_text = popups.ServerStatusText;
            //     reconnectCoroutine = StartCoroutine(ReconnectToServer(server_status_text));
            // }
        }
    }

    void SendPositionRotation() {
        PlayerData playerData = saveLoadManager.LoadPlayerData();
        bodyOrganManager = FindAnyObjectByType<BodyOrganManager>();

        GameObject organ = bodyOrganManager.ActiveObject;

        if (organ != null) {
            Transform organTransform = organ.transform;
            Vector3 position = organTransform.position;
            Vector3 rotation = organTransform.eulerAngles;

            // Format position and rotation as JSON
            string uuids = playerData.playerUUID;
            string typeSelected = organ.name.ToLower().Trim(); // Assuming bodyOrganName is defined elsewhere

            string json = $"{{\"uuid\":\"{uuids}\", \"role\":\"{userRoleStr}\", \"message\":\"{typeSelected}\", \"position\":{{\"x\":{position.x},\"y\":{position.y},\"z\":{position.z}}},\"rotation\":{{\"x\":{rotation.x},\"y\":{rotation.y},\"z\":{rotation.z}}}}}";
            // Debug.Log(json);
            byte[] data = Encoding.UTF8.GetBytes(json);
            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

            stream.Write(lengthPrefix, 0, lengthPrefix.Length);
            stream.Write(data, 0, data.Length);
        }

            
        
    }
    void UpdateVectorPosition(Vector3 vectorPosition)
    {
        // Example: Update a UI element or game object with the new position
        // foreach(GameObject organ in bodyOrganManager.Organs){
        //     if(organ.activeSelf){
        //         organ.transform.localPosition = vectorPosition;
        //     }
        // }

        bodyOrganManager = FindAnyObjectByType<BodyOrganManager>();

        if (bodyOrganManager != null) {
            GameObject organ = bodyOrganManager.ActiveObject;
            if (organ != null) {
                organ.transform.localPosition = vectorPosition;
            }
        }

        // cube.transform.localPosition = vectorPosition;
        // cube.SetActive(true);
        // Implement logic to update your Unity scene with the heart position
    }

    void UpdateGameObject(PositionRotation posRot) 
    {
        if (posRot != null)
        {
            previousPosition = new Vector3(posRot.positionX, posRot.positionY, posRot.positionZ);
            previousRotation = Quaternion.Euler(posRot.rotationX, posRot.rotationY, posRot.rotationZ);
        }
        
        bodyOrganManager = FindAnyObjectByType<BodyOrganManager>();
        if (bodyOrganManager != null) {
            GameObject organ = bodyOrganManager.ActiveObject;

            if (organ != null) {
                organ.transform.SetPositionAndRotation(previousPosition, previousRotation);
            }
        }


        // foreach(GameObject organ in bodyOrganManager.Organs){
        //     if (organ.activeSelf) {
        //         organ.transform.SetPositionAndRotation(previousPosition, previousRotation);
        //     }
        // }
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

        image.CopyToTexture(mTexture, true);
        FlipTextureVertically(mTexture);

        if (userRoleStr == "Host") {
            SendPositionRotation();
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
            // isConnected = false;
            // if (reconnectCoroutine == null)
            // {
            //     popups =  FindAnyObjectByType<Popups>();
            //     TMP_Text server_status_text = popups.ServerStatusText;
            //     reconnectCoroutine = StartCoroutine(ReconnectToServer(server_status_text));
            // }
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
