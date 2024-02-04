using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Zoomer : MonoBehaviour
{
    private static Zoomer instance;
    public static Zoomer Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("Zoomer").AddComponent<Zoomer>();
            }
            return instance;
        }
    }
    private Zoomer() { }

    // List<CinemachineVirtualCamera> Cameras = new();
    Dictionary<string, CinemachineVirtualCamera> Cameras = new Dictionary<string, CinemachineVirtualCamera>();
    int default_priority = 10;
    public string Register(CinemachineVirtualCamera camera)
    {
        string id = System.Guid.NewGuid().ToString();
        Cameras.Add(id, camera);
        camera.Priority = default_priority;
        return id;
    }

    public void Zoom(string id)
    {
        if (Cameras.ContainsKey(id))
        {
            CinemachineVirtualCamera camera = Cameras[id];
            ResetAllPriorityExcept(id);
            camera.Priority = 100;
        }
    }

    public void ResetAllPriorityExcept(string except)
    {
        foreach (var camera in Cameras)
        {
            if (camera.Key != except)
            {
                camera.Value.Priority = default_priority;
            }
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
