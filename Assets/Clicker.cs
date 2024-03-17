using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Clicker : MonoBehaviour
{
    void Start()
    {
        m_Camera = GetComponent<Camera>();
    }

    Camera m_Camera;
    Vector3 left_down;
    Vector3 right_down;

    enum Phase
    {
        WaitingForLeftDown,
        WaitingForRightDown
    }
    Phase phase = Phase.WaitingForLeftDown;

    [System.Serializable]
    public struct Vec3Pair
    {
        public Vector3 start;
        public Vector3 end;

        public Vec3Pair(Vector3 left_down, Vector3 right_down)
        {
            this.start = left_down;
            this.end = right_down;
        }
    }

    public List<Vec3Pair> pairs = new();
    bool isDone = false;
    readonly object _lock = new();
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) && phase == Phase.WaitingForLeftDown)
        {
            GetPoint(out left_down);
            phase = Phase.WaitingForRightDown;
        }

        if (Input.GetMouseButton(1) && phase == Phase.WaitingForRightDown)
        {
            GetPoint(out right_down);
        }
        lock (_lock)
        {

            if (left_down != Vector3.zero
                && right_down != Vector3.zero
                && !isDone
            )
            {
                Debug.Log("left_down: " + left_down);
                Debug.Log("right_down: " + right_down);
                isDone = true;

                pairs.Add(new(left_down, right_down));
                Sync();
                left_down = Vector3.zero;
                right_down = Vector3.zero;
                isDone = false;
                phase = Phase.WaitingForLeftDown;
            }
        }

        if (left_down != Vector3.zero)
        {
            var current = Vector3.zero;
            GetPoint(out current);
            Debug.DrawLine(left_down, current, Color.red);
        }

        //draw existing lines
        foreach (var pair in pairs)
        {
            Debug.DrawLine(pair.start, pair.end, Color.green);
        }
    }

    private void GetPoint(out Vector3 point)
    {
        point = Vector3.zero;

        //从摄像机发出到点击坐标的射线
        Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            //划出射线，只有在scene视图中才能看到
            Debug.DrawLine(ray.origin, hit.point);
            GameObject gameobj = hit.collider.gameObject;
            //注意要将对象的tag设置成collider才能检测到
            point = hit.point;
        }
    }

    // append latest data to file
    const string path = "Assets/points.txt";
    private void Sync()
    {
        StreamWriter writer = new(path, true);
        var latest = pairs[^1];
        writer.WriteLine($"{{\"start\": {latest.start}, \"end\": {latest.end}}},");
        writer.Close();

    }


}
