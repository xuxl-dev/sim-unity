using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.WebRTC;
using System;


public class WebRTCLive : MonoBehaviour
{
    void Start()
    {
        // StartCoroutine(WebRTC.Update());
        // var camera = GetComponent<Camera>();
        // var track = camera.CaptureStreamTrack(1280, 720);
        // var localConnection = new RTCPeerConnection();
        // var sendChannel = localConnection.CreateDataChannel("sendChannel");
        // sendChannel.OnOpen = HandleSendChannelStatusChange;
        // sendChannel.OnClose = HandleSendChannelStatusChange;

        // var configuration = new RTCConfiguration
        // {
        //     iceServers = new[]
        //     {
        //         new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } }
        //     }
        // };

        // var op = localConnection.SetConfiguration(ref configuration);
        // op = localConnection.AddTrack(track);
    }

    // IEnumerator WebRTC.Update()
    // {
    //     var peerConnection = new RTCPeerConnection();
    //     var configuration = new RTCConfiguration
    //     {
    //         iceServers = new[]
    //         {
    //             new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } }
    //         }
    //     };
    //     var op = peerConnection.SetConfiguration(configuration);
    //     yield return op;
    // }


private void HandleSendChannelStatusChange()
{

}

private void Awake()
{

}

void Update()
{

}
}
