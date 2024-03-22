using System;
using UnityEngine;

public class MockObstacle : MonoBehaviour
{
  [NonSerialized]
  static int _id = 0;
  [NonSerialized]
  public int id = _id++;
  public string Name => $"Human-{id}";
  Rigidbody rb;
  private void Start()
  {
    rb = GetComponent<Rigidbody>();
    Func<object> GetBriefFunc()
    {
      return () => new
      {
        speed = rb.velocity.magnitude,
        speed3 = rb.velocity.ToObject(),
        angular_speed = rb.angularVelocity.ToObject(),
        position = transform.position.ToObject(),
        rotation = transform.rotation.ToObject(),
        visibility = true,
      };
    }
    PhyEnvReporter.Instance.Subscribe(new PhyEnvReporter.SubscriberConfig
    {
      @event = "human",
      data = GetBriefFunc(),
      id = Name,
    });
  }
}