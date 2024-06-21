using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using Vuforia;
using Image = Vuforia.Image;
using System.Text;
using System.Threading;

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
    public string queue;
}
public class Services : MonoBehaviour
{
    public GameObject cube; 
    const PixelFormat PIXEL_FORMAT = PixelFormat.RGB888;
    const TextureFormat TEXTURE_FORMAT = TextureFormat.RGB24;

    private  Texture2D mTexture;
    private bool mFormatRegistered;

    private TcpClient client;
    private NetworkStream stream;
    private const int port = 5000;
    private const string serverAddress = "192.168.8.147"; // Change this to your server's IP address
    private bool isConnecting = false;
    private bool isConnected = false;


    private float reconnectInterval = 5f; // Seconds to wait before trying to reconnect
    private Coroutine reconnectCoroutine;

    void Start()
    {

        
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

        if(isConnected){
            SendJsonMessage();
        }

        if (stream != null && stream.DataAvailable)
        {
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
                    Vector3 vectorPosition = new Vector3(position.x, position.y, position.z);
                    // Vector3 vectorPosition = JsonUtility.FromJson<Vector3>(json);
                    UpdateVectorPosition(vectorPosition);
                }
                
                QueueMessage queueMessage = JsonUtility.FromJson<QueueMessage>(json);

                if(queueMessage != null){
                    Debug.Log(queueMessage.queue);
                }
                

            } 
            catch (Exception e) 
            {
                Debug.LogError("Error reading from stream: " + e.Message);
                isConnected = false;
                StopCoroutine(reconnectCoroutine);
                reconnectCoroutine = StartCoroutine(ReconnectToServer());
            } 

        }
    }



    void SendJsonMessage()
    {
        try
        {
            string uuids = "sfsdfsdfsdfsdff";
            string typeSelected = "heart";
            string json = $"{{\"uuid\":\"{uuids}\",\"message\":\"{typeSelected}\"}}";
            byte[] data = Encoding.UTF8.GetBytes(json);
            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

            stream.Write(lengthPrefix, 0, lengthPrefix.Length);
            stream.Write(data, 0, data.Length);

            Debug.Log("JSON message sent to server.");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to send JSON message: " + e.Message);
            isConnected = false;
            if (reconnectCoroutine == null)
            {
                reconnectCoroutine = StartCoroutine(ReconnectToServer());
            }
        }
    }



    void ConnectToServer(){
        if (isConnecting) return;
        isConnecting = true;

        Debug.Log("Attempting to connect to server...");

        try
        {
            client = new TcpClient(serverAddress, port);
            stream = client.GetStream();
            isConnecting = false;
            isConnected = true;

            Debug.Log("Connected to Python server.");
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
                reconnectCoroutine = StartCoroutine(ReconnectToServer());
            }
        }
    }

    IEnumerator ReconnectToServer()
    {
        while (!isConnected)
        {
            Debug.Log("Reconnecting to server...");
            yield return new WaitForSeconds(reconnectInterval);

            try
            {
                client = new TcpClient(serverAddress, port);
                stream = client.GetStream();
                isConnected = true;
                Debug.Log("Reconnected to Python server.");
            }
            catch (Exception e)
            {
                Debug.LogError("Reconnection failed: " + e.Message);
            }
        }

        reconnectCoroutine = null;
    }
    void UpdateVectorPosition(Vector3 vectorPosition)
    {
        // Example: Update a UI element or game object with the new position
        Debug.Log($"Heart Position: {vectorPosition}");
        cube.transform.localPosition = vectorPosition;
        cube.SetActive(true);
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

        Debug.Log("\nImage Format: " + image.PixelFormat +
                  "\nImage Size: " + image.Width + " x " + image.Height +
                  "\nBuffer Size: " + image.BufferWidth + " x " + image.BufferHeight +
                  "\nImage Stride: " + image.Stride + "\n");

        // Update the texture with the new frame
        image.CopyToTexture(mTexture, true);
        FlipTextureVertically(mTexture);
        // Send the frame to the Python server
        SendFrameToServer(mTexture);
        Thread.Sleep(30);
    }

    void RegisterFormat()
    {
        var success = VuforiaBehaviour.Instance.CameraDevice.SetFrameFormat(PIXEL_FORMAT, true);
        if (success)
        {
            Debug.Log("Successfully registered pixel format " + PIXEL_FORMAT);
            mFormatRegistered = true;
        }
        else
        {
            Debug.LogError("Failed to register pixel format " + PIXEL_FORMAT +
                           "\n the format may be unsupported by your device;" +
                           "\n consider using a different pixel format.");
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

            Debug.Log("Frame sent to server.");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to send frame: " + e.Message);
            isConnected = false;
            if (reconnectCoroutine == null)
            {
                reconnectCoroutine = StartCoroutine(ReconnectToServer());
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
