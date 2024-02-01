using UnityEngine;

public static class CUtils
{
  public static float LinearMapping(float x, float x1, float x2, float y1, float y2)
  {
    return (x - x1) / (x2 - x1) * (y2 - y1) + y1;
  }

  public static Bounds GetBounds(GameObject go)
  {
    Bounds bounds = new(go.transform.position, Vector3.zero);
    foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>())
    {
      bounds.Encapsulate(renderer.bounds);
    }
    return bounds;
  }

  // [System.Diagnostics.Conditional("UNITY_EDITOR")]
  public static void DrawDebugLine(Vector3 begin, Vector3 end, Color color)
  {
    Debug.DrawLine(begin, end, color);
  }
}