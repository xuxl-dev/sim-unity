using UnityEngine;

public static class VectorExtension
{
  public static object ToObject(this Vector3 vector)
  {
    return new
    {
      vector.x,
      vector.y,
      vector.z
    };
  }

  public static object ToJSON(this Vector3 vector)
  {
    return $"{{\"x\":{vector.x},\"y\":{vector.y},\"z\":{vector.z}}}";
  }

  public static object ToObject(this Quaternion quaternion)
  {
    return new
    {
      quaternion.x,
      quaternion.y,
      quaternion.z,
      quaternion.w
    };
  }
}

