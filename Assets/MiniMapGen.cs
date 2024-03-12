using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MiniMapGen : MonoBehaviour
{
    [Serializable]
    public class Hull {
        public List<Vector2> points;
    }

    [Serializable]
    public class Minimap {
        public List<Hull> hulls;
    }
    // Start is called before the first frame update
    void Start()
    {
        var hulls = GenerateMiniMap();
        Minimap minimap = new()
        {
            hulls = hulls.Select(hull => new Hull { points = hull }).ToList()
        };
        var json = JsonUtility.ToJson(minimap);
        Debug.Log(json);
        PhyEnvReporter.Instance.Push("minimap", new {
            type = "json-text",
            data = json
        });
    }

    // Update is called once per frame
    void Update()
    {

    }

    public List<GameObject> gameObjects = new();
    public List<string> tags = new();

    private List<MeshFilter> CollectMeshes()
    {
        List<MeshFilter> meshes = new();
        foreach (GameObject go in gameObjects)
        {
            if (go.TryGetComponent<MeshFilter>(out var mf))
            {
                meshes.Add(mf);
            }
        }

        foreach (string tag in tags)
        {
            GameObject[] gos = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject go in gos)
            {
                if (go.TryGetComponent<MeshFilter>(out var mf))
                {
                    meshes.Add(mf);
                }
            }
        }

        return meshes;
    }

    private List<Vector2> ProjectMesh(MeshFilter mf)
    {
        List<Vector2> points = new();
        foreach (Vector3 v in mf.mesh.vertices)
        {
            Vector3 p = mf.transform.TransformPoint(v);
            points.Add(new Vector2(p.x, p.z));
        }

        return points;
    }

    private List<Vector2> CalculateConvexHull(List<Vector2> points)
    {
        List<Vector2> hull = new();
        if (points.Count < 3)
        {
            return hull;
        }

        int l = 0;
        for (int i = 1; i < points.Count; i++)
        {
            if (points[i].x < points[l].x)
            {
                l = i;
            }
        }

        int p = l, q;
        do
        {
            hull.Add(points[p]);
            q = (p + 1) % points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                if (Orientation(points[p], points[i], points[q]) == 2)
                {
                    q = i;
                }
            }
            p = q;
        } while (p != l);

        return hull;
    }

    float Orientation(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.y - a.y) * (c.x - b.x) - (b.x - a.x) * (c.y - b.y);
    }

    float PerpendicularDistance(Vector2 p, Vector2 a, Vector2 b)
    {
        return Mathf.Abs((b.x - a.x) * (a.y - p.y) - (a.x - p.x) * (b.y - a.y)) / Mathf.Sqrt(Mathf.Pow(b.x - a.x, 2) + Mathf.Pow(b.y - a.y, 2));
    }

    private List<Vector2> DouglasPeuckerSimplify(List<Vector2> points, float epsilon)
    {
        int dmax = 0;
        int index = 0;
        int end = points.Count - 1;
        for (int i = 1; i < end; i++)
        {
            int d = (int)PerpendicularDistance(points[i], points[0], points[end]);
            if (d > dmax)
            {
                index = i;
                dmax = d;
            }
        }

        List<Vector2> result = new();
        if (dmax > epsilon)
        {
            List<Vector2> recResults1 = DouglasPeuckerSimplify(points.GetRange(0, index), epsilon);
            List<Vector2> recResults2 = DouglasPeuckerSimplify(points.GetRange(index, end - index), epsilon);
            result.AddRange(recResults1.GetRange(0, recResults1.Count - 1));
            result.AddRange(recResults2);
        }
        else
        {
            result.Add(points[0]);
            result.Add(points[end]);
        }

        return result;
    }

    public bool DoSimplify = false;
    public float Epsilon = 0.1f;
    public List<List<Vector2>> GenerateMiniMap()
    {
        List<MeshFilter> meshes = CollectMeshes();
        List<List<Vector2>> hulls = new();
        foreach (MeshFilter mf in meshes)
        {
            List<Vector2> points = ProjectMesh(mf);
            List<Vector2> hull = CalculateConvexHull(points);
            if (DoSimplify)
            {
                hull = DouglasPeuckerSimplify(hull, Epsilon);
            }
            Debug.Log("created hull with " + hull.Count + " points");
            hulls.Add(hull);
        }

        return hulls;
    }
}
