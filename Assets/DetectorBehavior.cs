using System.Collections.Generic;
using UnityEngine;

public class DetectorBehavior : MonoBehaviour
{
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

  // called when anything enters the detector
  // public List<DetectorEnterDelegate> enters = new();
  // public List<DetectorEnterDelegate> exits = new();

  // DetectorEnterDelegate enter;
}

// [System.Serializable]
// public delegate void DetectorEnterDelegate(DetectorBehavior detector, Collider other);