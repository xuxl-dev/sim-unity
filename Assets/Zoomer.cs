using System.Collections;
using System.Collections.Generic;
using Cinemachine;

public class Zoomer
{
    public static readonly Zoomer _instance = new();
    public static Zoomer Instance
    {
        get
        {
            return _instance;
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
