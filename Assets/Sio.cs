using System;
using System.Collections.Generic;
using SocketIOClient;
using UnityEngine;
using Newtonsoft.Json.Linq;
using SocketIOClient.Newtonsoft.Json;
using System.Text.Json;

public class Sio
{
    private Sio()
    {
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
        var uri = new Uri("http://localhost:3000");
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
                {
                    {"token", "UNITY" }
                }
            ,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
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
        Debug.Log("Connected" + socket.Connected);

        socket.OnAnyInUnityThread((name, response) =>
        {
            Debug.Log($"OnAnyInUnityThread: {name} ");
        });

    }
}