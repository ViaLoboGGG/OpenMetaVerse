using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public string playerName = "UnityClient";
    public string serverIP = "127.0.0.1";
    public int serverPort = 4000;
    public Transform playerTransform;

    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;

    void Start()
    {
        ConnectToServer();
    }

    void ConnectToServer()
    {
        try
        {
            client = new TcpClient();
            client.Connect(serverIP, serverPort);
            stream = client.GetStream();

            Debug.Log("[Client] Connected to server.");

            // You can hardcode this for now or let it come from a UI field
            string spaceID = "2f3b1892-6d5b-4118-a1f1-0f5d9d6a3abc"; // example GUID

            InitMessage init = new InitMessage
            {
                id = playerName,
                space_id = spaceID
            };

            string json = JsonUtility.ToJson(init) + "\n";
            byte[] data = Encoding.UTF8.GetBytes(json);
            stream.Write(data, 0, data.Length);
            Debug.Log($"[Client] Sent Init JSON: {json}");

            receiveThread = new Thread(ReceiveLoop);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("[Client] Connection error: " + e.Message);
        }
    }



    public void SendMove(float x, float y)
    {
        if (!IsConnected()) return;

        MoveMessage move = new MoveMessage { x = x, y = y };
        string json = JsonUtility.ToJson(move) + "\n";
        Send(json);
        Debug.Log($"[Client] Sent Move JSON: {json}");
    }


    public void SendChat(string message)
    {
        if (!IsConnected()) return;

        ChatMessage chat = new ChatMessage { message = message };
        string json = JsonUtility.ToJson(chat) + "\n";
        Send(json);
        Debug.Log($"[Client] Sent Chat JSON: {json}");
    }

    void Send(string message)
    {
        if (client == null || !client.Connected) return;

        byte[] data = Encoding.UTF8.GetBytes(message);
        try
        {
            stream.Write(data, 0, data.Length);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Client] Send error: " + e.Message);
        }
    }

    void ReceiveLoop()
    {
        byte[] buffer = new byte[1024];
        while (client != null && client.Connected)
        {
            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) continue;

                string json = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                Debug.Log("[Client] Raw JSON: " + json);

                BaseMessage baseMsg = JsonUtility.FromJson<BaseMessage>(json);

                switch (baseMsg.type)
                {
                    case "Spawn":
                        SpawnMessage spawn = JsonUtility.FromJson<SpawnMessage>(json);
                        HandleSpawn(spawn);
                        break;
                    case "Move":
                        ServerMoveMessage move = JsonUtility.FromJson<ServerMoveMessage>(json);
                        HandleMove(move);
                        break;
                    case "Chat":
                        ServerChatMessage chat = JsonUtility.FromJson<ServerChatMessage>(json);
                        HandleChat(chat);
                        break;
                    default:
                        Debug.LogWarning($"[Client] Unknown message type: {baseMsg.type}");
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Client] Receive error: " + e.Message);
                break;
            }
        }
    }

    void HandleSpawn(SpawnMessage msg)
    {
        Debug.Log($"[Client] Spawn Player {msg.id} with model: {msg.model_url}");
        // TODO: Download and instantiate model from model_url
    }

    void HandleMove(ServerMoveMessage msg)
    {
        Debug.Log($"[Client] Player {msg.id} moved to: {msg.x}, {msg.y}");
        // TODO: Move remote player representation
    }

    void HandleChat(ServerChatMessage msg)
    {
        Debug.Log($"[Client] {msg.id} says: {msg.message}");
        // TODO: Show message in chat window
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M) && playerTransform != null)
        {
            Vector3 pos = playerTransform.position;
            SendMove(pos.x, pos.z);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            SendChat("Hello from Unity!");
        }
    }

    void OnApplicationQuit()
    {
        receiveThread?.Abort();
        stream?.Close();
        client?.Close();
    }

    bool IsConnected()
    {
        return client != null && client.Connected && stream != null;
    }

    string EscapeJson(string str)
    {
        return str.Replace("\"", "\\\"").Replace("\n", "").Replace("\r", "");
    }
}

[Serializable]
public class BaseMessage
{
    public string type;
}

[Serializable]
public class SpawnMessage
{
    public string type;
    public string id;
    public string model_url;
}

[Serializable]
public class ServerMoveMessage
{
    public string type;
    public string id;
    public float x;
    public float y;
}

[Serializable]
public class ServerChatMessage
{
    public string type;
    public string id;
    public string message;
}
[Serializable]
public class ChatMessage
{
    public string type = "Chat";
    public string message;
}

[Serializable]
public class MoveMessage
{
    public string type = "Move";
    public float x;
    public float y;
}
[Serializable]
public class InitMessage
{
    public string type = "Init";  // <-- ADD THIS
    public string id;
    public string space_id;
}