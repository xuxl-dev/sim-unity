using System;
using System.Collections.Generic;
using SocketIOClient;
using UnityEngine;
using Newtonsoft.Json.Linq;
using SocketIOClient.Newtonsoft.Json;
using System.Text.Json;

public class Sio : MonoBehaviour
{
    private void Awake() {
        Init();
    }
    private static SocketIOUnity socket;
    public static SocketIOUnity Instance
    {
        get
        {
            if (socket == null)
            {
                Init();
            }
            return socket;
        }
    }
    // Start is called before the first frame update
    static void Init()
    {
        var uri = new Uri("http://127.0.0.1:3666");
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
                {
                    {"token", "UNITY" }
                }
            ,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
            ReconnectionDelayMax = 5000,
            ReconnectionDelay = 1000,
        })
        {
            JsonSerializer = new NewtonsoftJsonSerializer()
        };

        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("socket.OnConnected");
        };
        socket.OnDisconnected += (sender, e) =>
         {
             Debug.Log("disconnect: " + e);
         };
        socket.OnReconnectAttempt += (sender, e) =>
        {
            Debug.Log($"{DateTime.Now} Reconnecting: attempt = {e}");
        };


        Debug.Log("Connecting...");
        socket.Connect();
        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("socket.OnConnected");
        };

        socket.OnAnyInUnityThread((name, response) =>
        {
            Debug.Log($"OnAnyInUnityThread: {name} ");
        });

    }

    // when close the game, disconnect the socket
    private void OnApplicationQuit()
    {
        socket.Disconnect();
    }
}