using System;
using Unity.VisualScripting;
using UnityEngine;

public class MockCar : MonoBehaviour
{
  public string id = Guid.NewGuid().ToString();
  public string Name => $"CarAgent-{id}";
  Rigidbody rb;
  private void Start()
  {
    rb = GetComponent<Rigidbody>();
    Func<object> GetBriefFunc()
    {
      // rotate Y (this is quaternion rotation, not euler angle)
      return () => new
      {
        speed = rb.velocity.magnitude,
        speed3 = rb.velocity.ToObject(),
        angular_speed = rb.angularVelocity.ToObject(),
        position = transform.position.ToObject(),
        rotation = transform.rotation.ToObject(),
        reverse_y = false,
        visibility = true,
        traffic_light = "<none>",
      };
    }
    PhyEnvReporter.Instance.Subscribe(new PhyEnvReporter.SubscriberConfig
    {
      @event = "status",
      data = GetBriefFunc(),
      id = Name,
    });
  }
}