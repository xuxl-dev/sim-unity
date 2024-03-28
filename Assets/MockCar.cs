using System;
using Unity.VisualScripting;
using UnityEngine;

public class MockCar : MonoBehaviour
{
  public bool reverse_y = false;
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
        speed = this.speed.magnitude,
        speed3 = this.speed.ToObject(),
        angular_speed = rb.angularVelocity.ToObject(),
        position = transform.position.ToObject(),
        rotation = transform.rotation.ToObject(),
        reverse_y,
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
    previous_position = transform.position;
  }

  Vector3 previous_position;
  Vector3 speed;
  int interval = 0;
  private void FixedUpdate()
  {
    if (interval++ % 5 != 0) return;
    // calc speed
    speed = (transform.position - previous_position) / Time.fixedDeltaTime / 1.8f;
    if (speed.magnitude > 180f)
    {
      speed /= speed.magnitude;
      speed *= 3f;
    }

    previous_position = transform.position;
  }
}