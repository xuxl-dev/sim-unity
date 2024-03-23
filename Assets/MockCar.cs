using System;
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
      // add 180 deg to Y rotation
      var new_rotation = transform.rotation.eulerAngles;
      new_rotation.y += 180;
      

      return () => new 
      {
        speed = rb.velocity.magnitude,
        speed3 = rb.velocity.ToObject(),
        angular_speed = rb.angularVelocity.ToObject(),
        position = transform.position.ToObject(),
        rotation = new_rotation.ToObject(),
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