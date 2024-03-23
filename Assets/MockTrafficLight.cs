using System;
using UnityEngine;

public class MockTrafficLight : MonoBehaviour
{
  public string id = Guid.NewGuid().ToString();
  public string Name => $"TrafficLight-{id}";

  string Current_color {
    get {
      var color = GetComponent<Renderer>().material;
      if (color == red) return "red";
      if (color == green) return "green";
      if (color == yellow) return "yellow";
      return "unknown";
    }
  }
  public Material red;
  public Material green;
  public Material yellow;

  private void Start()
  {
    var GetLightStatus = new Func<object>(() => new
    {
      status = Current_color,
    });

    PhyEnvReporter.Instance.Subscribe(new PhyEnvReporter.SubscriberConfig
    {
      @event = "force-set-traffic-light",
      data = GetLightStatus,
      id = Name,
    });
  }
}