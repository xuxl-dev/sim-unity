using System.Collections.Generic;
using UnityEngine;

public class MockLane : MonoBehaviour
{
  public class Lane
  {
    public Vector3 beg;
    public Vector3 end;
    public Vector3 direction;
    public bool reversed;
    public GameObject gameObject;

    public Lane(Vector3 beg, Vector3 end, Vector3 direction, bool reversed, GameObject gameObject)
    {
      this.beg = beg;
      this.end = end;
      this.direction = direction;
      this.reversed = reversed;
      this.gameObject = gameObject;
    }
  }
  public List<Lane> lane_coords = new();
  public List<GameObject> lanes = new();

  [ContextMenu("Auto Add Lanes")]
  void AutoAddLanes()
  {
    this.lanes.Clear();
    this.lane_coords.Clear();
    var lanes = GameObject.FindGameObjectsWithTag("lane");
    foreach (var lane in lanes)
    {
      this.lanes.Add(lane);
    }

    foreach (var lane in this.lanes)
    {
      var boundingBox = CUtils.GetBounds(lane);
      bool isXY = boundingBox.extents.x > boundingBox.extents.z;
      if (isXY)
      {
        var beg = boundingBox.center - new Vector3(boundingBox.extents.x, 0, 0);
        var end = boundingBox.center + new Vector3(boundingBox.extents.x, 0, 0);
        var direction = (end - beg).normalized;
        var reversed = false; // this is assgined by user later
        lane_coords.Add(new Lane(beg, end, direction, reversed, lane));
      }
      else
      {
        var beg = boundingBox.center - new Vector3(0, 0, boundingBox.extents.z);
        var end = boundingBox.center + new Vector3(0, 0, boundingBox.extents.z);
        var direction = (end - beg).normalized;
        var reversed = false; // this is assgined by user later
        lane_coords.Add(new Lane(beg, end, direction, reversed, lane));
      }
    }
  }


  void ReportRoads()

  {
    // Debug.Log("Reporting roads");
    var roads = new List<object>();
    foreach (var lane in lane_coords)
    {
      var boundingBox = CUtils.GetBounds(lane.gameObject);
      roads.Add(new
      {
        beg = lane.beg.ToObject(),
        end = lane.end.ToObject(),
        direction = lane.direction.ToObject(),
        lane.reversed,
        boundingBox = new
        {
          min = boundingBox.min.ToObject(),
          max = boundingBox.max.ToObject()
        }
      });
    }
    PhyEnvReporter.Instance.Push("roads", roads.ToArray());
  }

  private void Start()
  {
    AutoAddLanes();
    ReportRoads();
  }
}