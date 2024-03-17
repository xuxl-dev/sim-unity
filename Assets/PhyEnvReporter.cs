using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.RenderStreaming;
using UnityEngine;

public class PhyEnvReporter : MonoBehaviour
{
  public static PhyEnvReporter Instance;
  private SignalingManager signalingManager;
  public bool trainingMode = false;

  private void Awake()
  {
    Instance = GetComponent<PhyEnvReporter>();
    signalingManager = GetComponent<SignalingManager>();
    if (!trainingMode && signalingManager != null)
    {
      signalingManager.Run();
    }
  }

  public float report_interval = 0.2f;
  private bool _stop = false;
  public class SubscriberConfig
  {
    public string @event;
    public Func<object> data;
#nullable enable
    public Func<object, bool>? filter;
    public string? id;
#nullable disable
  }
  private List<SubscriberConfig> subscribers = new();

  public IEnumerator ReportCoroutine()
  {
    while (!_stop)
    {
      foreach (var subscriber in subscribers)
      {
        var data = subscriber.data();
        if (subscriber.filter != null && !subscriber.filter(data))
        {
          continue;
        }
        var dict = Sio.MakeDict(new
        {
          payload = data,
          @event = subscriber.@event,
        });
        if (subscriber.id != null) //todo refactor
        {
          dict["id"] = subscriber.id;
        }

        Sio.EmitDict(subscriber.@event, dict);
      }
      yield return new WaitForSeconds(report_interval);
    }
  }

  public void Subscribe(SubscriberConfig cfg)
  {
    subscribers.Add(cfg);
  }

#nullable enable
  private Queue<(string @event, object data, string? id)> _eventQueue = new();

  public void Push(string @event, object data, string? id = null)
  {
    if (trainingMode)
    {
      return;
    }

    if (Sio.IsAvaliable == false)
    {
      Debug.LogWarning("Sio is not ready, push to queue");
      _eventQueue.Enqueue((@event, data, id));
      return;
    }

    var dict = Sio.MakeDict(new
    {
      payload = data,
      @event,
    });
    if (id != null) //todo refactor
    {
      dict["id"] = id;
    }
    // Debug.Log($"PhyEnvReporter.dict: {dict} ");
    Sio.EmitDict(@event, dict);
  }
#nullable disable

  public void Stop()
  {
    _stop = true;
  }

  public void Start()
  {
    if (trainingMode)
    {
      Debug.LogWarning("PhyEnvReporter will not working in training mode");

      DisableComponentsWhenTraining();
      Debug.LogWarning("Some components deactivated when training mode is enabled.");

      Sio.prevent_init = true;
      Debug.LogWarning("Sio is disabled when training mode is enabled.");
      return;
    }
    _stop = false;
    StartCoroutine(ReportCoroutine());

    Sio.Ready += () =>
    {
      Debug.Log("Sio.Ready, flushed cached events, total: " + _eventQueue.Count);
      foreach (var (e, d, id) in _eventQueue)
      {
        // Debug.Log($"Resending: {e} ");
        Push(e, d, id);
      }
      // _eventQueue.Clear();
    };
  }

  public void DisableComponentsWhenTraining()
  {
    var types = new List<Type>
    {
      typeof(CinemachineVirtualCamera),
    };
    if (trainingMode)
    {
      foreach (var T in types)
      {
        foreach (var component in FindObjectsOfType(T))
        {
          (component as MonoBehaviour).enabled = false;
        }
      }
    }
  }
}