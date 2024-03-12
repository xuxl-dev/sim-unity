using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MiniMapGen : MonoBehaviour
{
    [Serializable]
    public class Hull
    {
        public List<Vector2> points;
    }

    [Serializable]
    public class Minimap
    {
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
        PhyEnvReporter.Instance.Push("minimap", new
        {
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

    public bool useAlphaShape = false;
    public bool DoSimplify = false;
    public float Epsilon = 0.1f;
    public List<List<Vector2>> GenerateMiniMap()
    {
        List<MeshFilter> meshes = CollectMeshes();
        List<List<Vector2>> hulls = new();
        foreach (MeshFilter mf in meshes)
        {
            List<Vector2> points = ProjectMesh(mf);

            var hull = ConvexHull.ComputeConvexHull(points).ToList();
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


public static class ConvexHull
{
    public static IList<Vector2> ComputeConvexHull(List<Vector2> points, bool sortInPlace = false)
    {
        if (!sortInPlace)
            points = new List<Vector2>(points);
        points.Sort((a, b) =>
            a.x == b.x ? a.y.CompareTo(b.y) : (a.x > b.x ? 1 : -1));

        // Importantly, DList provides O(1) insertion at beginning and end
        CircularList<Vector2> hull = new CircularList<Vector2>();
        int L = 0, U = 0; // size of lower and upper hulls

        // Builds a hull such that the output polygon starts at the leftmost Vector2.
        for (int i = points.Count - 1; i >= 0; i--)
        {
            Vector2 p = points[i], p1;

            // build lower hull (at end of output list)
            while (L >= 2 && (p1 = hull.Last).Sub(hull[hull.Count - 2]).Cross(p.Sub(p1)) >= 0)
            {
                hull.PopLast();
                L--;
            }
            hull.PushLast(p);
            L++;

            // build upper hull (at beginning of output list)
            while (U >= 2 && (p1 = hull.First).Sub(hull[1]).Cross(p.Sub(p1)) <= 0)
            {
                hull.PopFirst();
                U--;
            }
            if (U != 0) // when U=0, share the Vector2 added above
                hull.PushFirst(p);
            U++;
            Debug.Assert(U + L == hull.Count + 1);
        }
        hull.PopLast();
        return hull;
    }

    private static Vector2 Sub(this Vector2 a, Vector2 b)
    {
        return a - b;
    }

    private static float Cross(this Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    private class CircularList<T> : List<T>
    {
        public T Last
        {
            get
            {
                return this[this.Count - 1];
            }
            set
            {
                this[this.Count - 1] = value;
            }
        }

        public T First
        {
            get
            {
                return this[0];
            }
            set
            {
                this[0] = value;
            }
        }

        public void PushLast(T obj)
        {
            this.Add(obj);
        }

        public T PopLast()
        {
            T retVal = this[this.Count - 1];
            this.RemoveAt(this.Count - 1);
            return retVal;
        }

        public void PushFirst(T obj)
        {
            this.Insert(0, obj);
        }

        public T PopFirst()
        {
            T retVal = this[0];
            this.RemoveAt(0);
            return retVal;
        }
    }
}


public class Edge
{
    public Vector2 A { get; set; }
    public Vector2 B { get; set; }
}

public class AlphaShape
{
    public List<Edge> BorderEdges { get; private set; }
    public List<Vector2> BorderPoints { get; private set; }
    public AlphaShape(List<Vector2> points, float alpha)
    {
        // 0. error checking, init
        if (points == null || points.Count < 2) { throw new ArgumentException("AlphaShape needs at least 2 points"); }
        BorderEdges = new List<Edge>();
        var alpha_2 = alpha * alpha;

        // 1. run through all pairs of points
        for (int i = 0; i < points.Count - 1; i++)
        {
            for (int j = i + 1; j < points.Count; j++)
            {
                if (points[i] == points[j]) { continue; } // alternatively, continue
                var dist = Dist(points[i], points[j]);
                if (dist > 2 * alpha) { continue; } // circle fits between points ==> p_i, p_j can't be alpha-exposed                    

                float x1 = points[i].x, x2 = points[j].x, y1 = points[i].y, y2 = points[j].y; // for clarity & brevity

                var mid = new Vector2((x1 + x2) / 2, (y1 + y2) / 2);

                // find two circles that contain p_i and p_j; note that center1 == center2 if dist == 2*alpha
                var center1 = new Vector2(
                    mid.x + (float)Math.Sqrt(alpha_2 - (dist / 2) * (dist / 2)) * (y1 - y2) / dist,
                    mid.y + (float)Math.Sqrt(alpha_2 - (dist / 2) * (dist / 2)) * (x2 - x1) / dist
                    );

                var center2 = new Vector2(
                    mid.x - (float)Math.Sqrt(alpha_2 - (dist / 2) * (dist / 2)) * (y1 - y2) / dist,
                    mid.y - (float)Math.Sqrt(alpha_2 - (dist / 2) * (dist / 2)) * (x2 - x1) / dist
                    );

                // check if one of the circles is alpha-exposed, i.e. no other point lies in it
                bool c1_empty = true, c2_empty = true;
                for (int k = 0; k < points.Count && (c1_empty || c2_empty); k++)
                {
                    if (points[k] == points[i] || points[k] == points[j]) { continue; }

                    if ((center1.x - points[k].x) * (center1.x - points[k].x) + (center1.y - points[k].y) * (center1.y - points[k].y) < alpha_2)
                    {
                        c1_empty = false;
                    }

                    if ((center2.x - points[k].x) * (center2.x - points[k].x) + (center2.y - points[k].y) * (center2.y - points[k].y) < alpha_2)
                    {
                        c2_empty = false;
                    }
                }

                if (c1_empty || c2_empty)
                {
                    // yup!
                    BorderEdges.Add(new Edge() { A = points[i], B = points[j] });
                }
            }
        }

        // 2. extract border points from border edges
        BorderPoints = new List<Vector2>();
        var set = new HashSet<Vector2>();
        foreach (var edge in BorderEdges)
        {
            if (!set.Contains(edge.A))
            {
                BorderPoints.Add(edge.A);
                set.Add(edge.A);
            }
            if (!set.Contains(edge.B))
            {
                BorderPoints.Add(edge.B);
                set.Add(edge.B);
            }
        }
    }

    public static float Dist(Vector2 A, Vector2 B)
    {
        return (float)Math.Sqrt((A.x - B.x) * (A.x - B.x) + (A.y - B.y) * (A.y - B.y));
    }

}