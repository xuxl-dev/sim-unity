using System.Collections.Generic;
using UnityEngine;

public class DetectorBehavior : MonoBehaviour
{
  // internal IntersectionLane intersectionLane;
  public string lane = null;
  public List<string> next = new();
  public float test = 0f;
  

  private void OnTriggerEnter(Collider other)
  {
    if (other.gameObject.CompareTag(Tokens.CAR))
    {
      if (other.gameObject.TryGetComponent<CarAgent>(out var car))
      {
        car.OnDetectorEnter(this);
      }
    }
  }

  private void OnTriggerExit(Collider other)
  {
    if (other.gameObject.CompareTag(Tokens.CAR))
    {
      if (other.gameObject.TryGetComponent<CarAgent>(out var car))
      {
        car.OnDetectorExit(this);
      }
    }
  }
}