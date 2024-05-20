using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BezierCurveVisualizer : MonoBehaviour
{
    [System.Serializable]
    public class CurveEnds
    {
        public Transform pointA;
        public Transform pointB;
    }

    [Header("Bezier Curve Points")]
    public List<CurveEnds> bPoints = new List<CurveEnds>();

    [System.Serializable]
    public class Animation
    {
        public int id;
        public Transform animation;
    }

    class RunningAnimation
    {
        public Transform from;
        public Transform to;
        public int t;
        public int segements;

        public Transform target;
    }

    private List<RunningAnimation> runningAnis = new();

    [Header("Bezier Curve Animations")]
    public List<Animation> animations = new List<Animation>();
    public Transform animationObject;
    public float height = 5.0f;
    public float relHeight = 0.5f;
    public int segmentCount = 50;

    public float lineWidth = 1f;

    void Start()
    {
    }

    void FixedUpdate()
    {
        UpdateManyCurve();
        UpdateAnimations();
    }

    List<GameObject> TempObjs = new();
    public Material TempMaterial;
    LineRenderer GetTempLineRenderer()
    {
        // 创建一个新的GameObject并将其设为当前对象的子对象
        GameObject child = new GameObject();
        child.transform.parent = gameObject.transform;

        // 添加LineRenderer组件
        LineRenderer renderer = child.AddComponent<LineRenderer>();
        renderer.widthMultiplier = lineWidth;
        // 将临时对象添加到TempObjs列表
        TempObjs.Add(child);

        if (renderer.sharedMaterial == null)
        {
            // 分配一个默认材质
            renderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        // 设置材质
        // 使用 sharedMaterial 避免在编辑模式下生成材质实例
        Material newMaterial = new Material(renderer.sharedMaterial);
        var color = Random.ColorHSV();
        color.a = 0.8f;
        newMaterial.color = color;

        renderer.sharedMaterial = newMaterial;

        // 设置位置点数量
        renderer.positionCount = segmentCount + 1;

        return renderer;
    }


    [ContextMenu("CleanObjs")]
    void CleanTempObjs()
    {
        foreach (var item in TempObjs)
        {
            DestroyImmediate(item);
        }
        TempObjs.Clear();
    }

    void UpdateManyCurve()
    {
        for (int i = 0; i < bPoints.Count; i++)
        {
            UpdateCurve(bPoints[i].pointA, bPoints[i].pointB, i < TempObjs.Count ? TempObjs[i].GetComponent<LineRenderer>() : null);
        }
    }

    void UpdateAnimations()
    {
        foreach (var animation in runningAnis)
        {
            float tt = (float)animation.t / animation.segements;
            var pt = CalculateBezierPoint(tt, animation.from.position,
                GetMidCtrlPt(animation.from, animation.to),
                animation.to.position);

            animation.target.position = pt;
            animation.t++;
        }

        foreach (var animation in runningAnis.FindAll(ani => ani.t > ani.segements))
        {
            animation.target.gameObject.SetActive(false);
        }

        runningAnis.RemoveAll(ani => ani.t > ani.segements);
    }

    void UpdateCurve(Transform pointA, Transform pointB, LineRenderer cached = null)
    {
        var lineRenderer = cached ?? GetTempLineRenderer();

        Vector3 controlPoint = (pointA.position + pointB.position) / 2 + Vector3.up * height
            + relHeight * Vector3.Distance(pointA.position, pointB.position) * Vector3.up;


        for (int i = 0; i <= segmentCount; i++)
        {
            float t = i / (float)segmentCount;
            Vector3 position = CalculateBezierPoint(t, pointA.position, controlPoint, pointB.position);
            lineRenderer.SetPosition(i, position);
        }
    }

    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;

        return p;
    }

    [ContextMenu("Preview")]
    public void Preview()
    {
        CleanTempObjs();
        UpdateManyCurve();
    }

    public Vector3 GetMidCtrlPt(Transform pointA, Transform pointB)
    {
        return (pointA.position + pointB.position) / 2 + Vector3.up * height + relHeight * Vector3.Distance(pointA.position, pointB.position) * Vector3.up;
    }

    // public IEnumerator BeginAnimation(int id, float total_time = 1.0f)
    // {
    //     int index = animations.FindIndex(x => x.id == id);
    //     while (true)
    //     {
    //         if (index == -1)
    //         {
    //             break;
    //         }
    //         Transform pointA = bPoints[id].pointA;
    //         Transform pointB = bPoints[id].pointB;
    //         Transform animation = animations[index].animation;
    //         for (float t = 0; t < total_time; t += Time.fixedDeltaTime)
    //         {
    //             float tt = t / total_time;
    //             animation.position = CalculateBezierPoint(tt, pointA.position,
    //             (pointA.position + pointB.position) / 2 + Vector3.up * height + relHeight * Vector3.Distance(pointA.position, pointB.position) * Vector3.up,
    //             pointB.position);
    //             yield return null;
    //         }
    //     }
    // }

    public void AddAni(int id)
    {
        var cfg = bPoints[id];
        RunningAnimation ani = new()
        {
            from = cfg.pointA,
            to = cfg.pointB,
            segements = 100,
            t = 0,
            target = animations[id].animation,
        };
        ani.target.gameObject.SetActive(true);
        runningAnis.Add(ani);
    }

    [ContextMenu("Ani0")]
    public void TestAnimation()
    {
        AddAni(0);
    }
}
