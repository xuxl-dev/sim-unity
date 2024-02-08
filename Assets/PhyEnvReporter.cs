using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhyEnvReporter : MonoBehaviour
{
  public static PhyEnvReporter Instance;
  private void Awake()
  {
    Instance = this;
  }

  internal float report_interval = 0.1f;
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
        var dict = Sio.MakeDict(new {
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
  public void Push(string @event, object data, string? id = null)
  {
    var dict = Sio.MakeDict(new {
      payload = data,
      @event = @event,
    });
    if (id != null) //todo refactor
    {
      dict["id"] = id;
    }
    Sio.EmitDict(@event, dict);
  }
#nullable disable

  public void Stop()
  {
    _stop = true;
  }

  public void Start()
  {
    _stop = false;
    StartCoroutine(ReportCoroutine());
  }
}