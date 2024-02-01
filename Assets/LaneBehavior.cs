using UnityEngine;

public class LaneBehavior : MonoBehaviour
{
    public float debug_drawing_y = 5f;
    public float road_y = 0f;
    internal Vector3 begin;
    internal Vector3 end;

    public Vector3 BeginAbs => transform.TransformPoint(begin);
    public Vector3 EndAbs => transform.TransformPoint(end);

    void Start()
    {
        var bounds = CUtils.GetBounds(gameObject);
        // determine direction of lane
        if (bounds.size.x > bounds.size.z)
        {
            // horizontal lane
            begin = new Vector3(bounds.min.x, road_y, bounds.center.z);
            end = new Vector3(bounds.max.x, road_y, bounds.center.z);
        }
        else
        {
            // vertical lane
            begin = new Vector3(bounds.center.x, road_y, bounds.min.z);
            end = new Vector3(bounds.center.x, road_y, bounds.max.z);
        }
    }

    void Update()
    {
        var y_shift = new Vector3(0, debug_drawing_y, 0);
        // draw lane
        Debug.DrawLine(begin + y_shift, end + y_shift, Color.red);
    }

    /// <summary>
    /// calculate the perpendicular distance to the center line (y-plane)
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public float GetOffsetToCenterLine(Vector3 position)
    {
        var pos_y_0 = new Vector3(position.x, 0, position.z);
        var beg_y_0 = new Vector3(begin.x, 0, begin.z);
        var end_y_0 = new Vector3(end.x, 0, end.z);
        var perpendicular = Vector3.Project(pos_y_0 - beg_y_0, end_y_0 - beg_y_0) + beg_y_0;
        var perpendicular_y_0 = new Vector3(perpendicular.x, 0, perpendicular.z);
        var y_shift = new Vector3(0, debug_drawing_y, 0);

        CUtils.DrawDebugLine(pos_y_0 + y_shift, perpendicular_y_0 + y_shift, Color.green);

        var offset = Vector3.Distance(pos_y_0, perpendicular_y_0);
        return offset;
    }


}
