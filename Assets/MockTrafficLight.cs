using System;
using UnityEngine;

public class MockTrafficLight : MonoBehaviour
{
  public string id = Guid.NewGuid().ToString();
  public string Name => $"TrafficLight-{id}";

  string Current_color
  {
    get
    {
      var color = GetComponent<Renderer>().material;
      if (CompareTextureColor(color, red)) return "red";
      if (CompareTextureColor(color, green)) return "green";
      if (CompareTextureColor(color, yellow)) return "yellow";
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
      color = Current_color,
      time = 0,
    });

    PhyEnvReporter.Instance.Subscribe(new PhyEnvReporter.SubscriberConfig
    {
      @event = "force-set-traffic-light",
      data = GetLightStatus,
      id = Name,
    });
  }

  private bool CompareTextureColor(Material a, Material b, float tolerance = 0.1f)
  {
    return Math.Abs(a.color.r - b.color.r) < tolerance &&
           Math.Abs(a.color.g - b.color.g) < tolerance &&
           Math.Abs(a.color.b - b.color.b) < tolerance;
  }
}