using UnityEngine;

public class BroadcastInfoBuilder
{
  public enum BroadcastInfoBuilderType
  {
    VehicleObservation,
    SelfStatus,
    BroadcastEvents,
    ObsticleObservation,
    TrafficLightObservation,
  }

  public BroadcastInfoBuilder(int size)
  {
    broadcast = new float[size];
    p_payload = one_hot_size;
  }
  // [one hot part] [payload part]
  public float[] broadcast;
  int one_hot_size = 8;
  int p_onehot = 0;
  int p_payload = -1;

  public BroadcastInfoBuilder AsType(BroadcastInfoBuilderType type)
  {
    broadcast[p_onehot + (int)type] = 1;
    return this;
  }

  public float[] Build()
  {
    return broadcast;
  }

  public BroadcastInfoBuilder Add(float value)
  {
    broadcast[p_payload++] = value;
    return this;
  }

  public BroadcastInfoBuilder Add(float[] values)
  {
    foreach (var value in values)
    {
      broadcast[p_payload++] = value;
    }
    return this;
  }

  public BroadcastInfoBuilder Add(Vector3 vector)
  {
    broadcast[p_payload++] = vector.x;
    broadcast[p_payload++] = vector.y;
    broadcast[p_payload++] = vector.z;
    return this;
  }

  public BroadcastInfoBuilder Add(Vector3[] vectors)
  {
    foreach (var vector in vectors)
    {
      broadcast[p_payload++] = vector.x;
      broadcast[p_payload++] = vector.y;
      broadcast[p_payload++] = vector.z;
    }
    return this;
  }

  public BroadcastInfoBuilder Add(Quaternion quaternion)
  {
    broadcast[p_payload++] = quaternion.x;
    broadcast[p_payload++] = quaternion.y;
    broadcast[p_payload++] = quaternion.z;
    broadcast[p_payload++] = quaternion.w;
    return this;
  }

  public BroadcastInfoBuilder Add(Quaternion[] quaternions)
  {
    foreach (var quaternion in quaternions)
    {
      broadcast[p_payload++] = quaternion.x;
      broadcast[p_payload++] = quaternion.y;
      broadcast[p_payload++] = quaternion.z;
      broadcast[p_payload++] = quaternion.w;
    }
    return this;
  }

  public BroadcastInfoBuilder Add(int value)
  {
    broadcast[p_payload++] = value;
    return this;
  }

  public BroadcastInfoBuilder Add(int[] values)
  {
    foreach (var value in values)
    {
      broadcast[p_payload++] = value;
    }
    return this;
  }
}
