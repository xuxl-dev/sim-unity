#define SIO_DEBUG

using System;
using System.Collections.Generic;
using SocketIOClient;
using UnityEngine;
using Newtonsoft.Json.Linq;
using SocketIOClient.Newtonsoft.Json;
using System.Text.Json;
using System.Diagnostics;
using Dbg = UnityEngine.Debug;

public class Sio : MonoBehaviour
{
    public static string MyId = "UNITY";
    private void Awake()
    {
        Init();
    }
    private static SocketIOUnity socket;

    // only not in training mode
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
            ReconnectionAttempts = 5
        })
        {
            JsonSerializer = new NewtonsoftJsonSerializer()
        };

        socket.OnConnected += (sender, e) =>
        {
            Dbg.Log("socket.OnConnected");
        };
        socket.OnDisconnected += (sender, e) =>
         {
             Dbg.Log("disconnect: " + e);
         };
        socket.OnReconnectAttempt += (sender, e) =>
        {
            Dbg.Log($"{DateTime.Now} Reconnecting: attempt = {e}");
        };


        Dbg.Log("Connecting...");
        socket.Connect();
        socket.OnConnected += (sender, e) =>
        {
            Dbg.Log("socket.OnConnected");
        };


        socket.OnAnyInUnityThread((name, response) =>
        {
            Dbg.Log($"OnAnyInUnityThread: {name} ");
        });

    }

    // when close the game, disconnect the socket
    private void OnApplicationQuit()
    {
        socket.Disconnect();
    }

    [Conditional("SIO_DEBUG")]
    public static void Emit(string eventName, object data)
    {
        var dict = MakeDict(data);
        // dict["id"] = MyId;
        Instance.Emit(eventName, dict);
    }

    [Conditional("SIO_DEBUG")]
    public static void EmitDict(string eventName, Dictionary<string, object> data)
    {
        Instance.Emit(eventName, data);
    }

    public static Dictionary<string, object> MakeDict(object content)
    {
        var props = content.GetType().GetProperties();
        var pairDictionary = new Dictionary<string, object>();
        foreach (var prop in props)
        {
            var value = prop.GetValue(content);
            if (value == null)
            {
                continue;
            }
            if (value.GetType().IsPrimitive || value.GetType() == typeof(string))
            {
                pairDictionary.Add(prop.Name, value);
            }
            else
            {
                pairDictionary.Add(prop.Name, MakeDict(value));
            }
        }
        return pairDictionary;
    }
}