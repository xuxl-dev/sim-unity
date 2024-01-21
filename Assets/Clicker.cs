using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clicker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        m_Camera = Camera.main;
    }

    Camera m_Camera;
    Vector3 left_down;
    Vector3 right_down;

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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            GetPoint(out left_down);
        }
        if (Input.GetMouseButton(1))
        {
            GetPoint(out right_down);
        }

        if (left_down != Vector3.zero && right_down != Vector3.zero && !isDone)
        {
            Debug.Log("left_down: " + left_down);
            Debug.Log("right_down: " + right_down);
            isDone = true;

            pairs.Add(new(left_down, right_down));
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isDone = false;
            left_down = Vector3.zero;
            right_down = Vector3.zero;
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
            if (gameobj.CompareTag("road"))
            {
                point = hit.point;
            }
        }
    }
}
