using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Intersection
{
    public Vector2 Position;
    public (Road u, Road v) Roads;
    public Vector2 Size;

    public Dictionary<Cardinal, List<IntersectionLane>> intersectionLanes = new(){
        { Cardinal.North, new List<IntersectionLane>() },
        { Cardinal.East, new List<IntersectionLane>() },
        { Cardinal.South, new List<IntersectionLane>() },
        { Cardinal.West, new List<IntersectionLane>() },
    };

    public Intersection(Road u, Road v)
    {
        var left = Mathf.Max(u.Left, v.Left);
        var right = Mathf.Min(u.Right, v.Right);
        var top = Mathf.Max(u.Top, v.Top);
        var bottom = Mathf.Min(u.Bottom, v.Bottom);

        Position = new Vector2(left, top);
        Size = new Vector2(right - left, bottom - top).Abs();
        Roads = (u, v);

        SolveIntersectionLane();
        SolveNext();
    }

    public void SolveIntersectionLane()
    {
        // Generate IntersectionLanes
        var lanes = Roads.u.lanes.Concat(Roads.v.lanes);
        var intersectionLanes = new List<IntersectionLane>();
        foreach (var lane in lanes)
        {
            intersectionLanes.AddRange(IntersectionLane.Get(lane, this));
        }

        // add them to lanes according to their attachTo
        foreach (var intersectionLane in intersectionLanes)
        {
            this.intersectionLanes[intersectionLane.attachTo].Add(intersectionLane);
        }

        Debug.Log($"Intersection {Position} has {intersectionLanes.Count} lanes");
    }

    public void SolveNext()
    {
        List<IntersectionLane> GetNextLanes(IntersectionLane from, Turn turn)
        {
            if (!from.entering) // only entering lanes have next lanes
            {
                return new List<IntersectionLane>();
            }

            if (turn == Turn.Straight)
            {
                return new List<IntersectionLane>() { from.pair };
            }

            var nextAttachingTo = from.attachTo.TurnTo(turn);

            // find next lanes if we are turning
            return intersectionLanes[nextAttachingTo].Where(x => !x.entering).ToList();
        }

        foreach (var intersectionLane in intersectionLanes.Values.SelectMany(x => x))
        {
            var lane = intersectionLane.lane;
            foreach (var turn in lane.turns)
            {
                var nextLanes = GetNextLanes(intersectionLane, turn);
                intersectionLane.next.AddRange(nextLanes);
            }
        }

        Debug.Log($"Intersection {Position} has {intersectionLanes.Values.SelectMany(x => x).Count()} next lanes");
    }

    public float Left => Position.x;
    public float Right => Position.x + Size.x;
    public float Top => Position.y;
    public float Bottom => Position.y + Size.y;

    public (Vector2 from, Vector2 to) GetEdge(Cardinal cardinal)
    {
        return cardinal switch
        {
            Cardinal.North => (new Vector2(Left, Top), new Vector2(Right, Top)),
            Cardinal.East => (new Vector2(Right, Top), new Vector2(Right, Bottom)),
            Cardinal.South => (new Vector2(Right, Bottom), new Vector2(Left, Bottom)),
            Cardinal.West => (new Vector2(Left, Bottom), new Vector2(Left, Top)),
            _ => throw new System.Exception("Invalid cardinal"),
        };
    }
}

static class IntersectionUtils
{
    public static void Show(this Intersection intersection)
    {
        // DebugUtils.DrawCircle(new Vector3(intersection.Left, 0, intersection.Top), 0.1f, Color.green, 1000);
        Debug.DrawLine(new Vector3(intersection.Left, 0, intersection.Top), new Vector3(intersection.Right, 0, intersection.Top), Color.green, 1000);
        Debug.DrawLine(new Vector3(intersection.Right, 0, intersection.Top), new Vector3(intersection.Right, 0, intersection.Bottom), Color.green, 1000);
        Debug.DrawLine(new Vector3(intersection.Right, 0, intersection.Bottom), new Vector3(intersection.Left, 0, intersection.Bottom), Color.green, 1000);
        Debug.DrawLine(new Vector3(intersection.Left, 0, intersection.Bottom), new Vector3(intersection.Left, 0, intersection.Top), Color.green, 1000);
        intersection.ShowIntersectionLanes();
    }

    public static void ShowIntersectionLanes(this Intersection intersection)
    {
        foreach (var curr in intersection.intersectionLanes.Values.SelectMany(x => x))
        {
            DebugUtils.DrawCircle(new Vector3(curr.attachingPoint.x, 0, curr.attachingPoint.y), 0.1f,
                curr.entering ? Color.blue : Color.yellow, 1000);
            foreach (var next in curr.next)
            {
                var from = new Vector3(curr.attachingPoint.x, 0, curr.attachingPoint.y);
                var to = new Vector3(next.attachingPoint.x, 0, next.attachingPoint.y);
                DrawArrow.ForDebug(from, to - from, Color.cyan, 100, 0.08f, 20f);
            }
        }
        Debug.Log($"Intersection {intersection.Position} has {intersection.intersectionLanes.Values.SelectMany(x => x).Count()} lanes");
    }

}

public enum Cardinal
{
    North,
    East,
    South,
    West
}

public static class CardinalUtils
{
    public static Cardinal Opposite(this Cardinal cardinal)
    {
        return cardinal switch
        {
            Cardinal.North => Cardinal.South,
            Cardinal.East => Cardinal.West,
            Cardinal.South => Cardinal.North,
            Cardinal.West => Cardinal.East,
            _ => throw new System.Exception("Invalid cardinal"),
        };
    }

    public static Cardinal TurnTo(this Cardinal cardinal, Turn turn)
    {
        return (cardinal, turn) switch
        {
            (Cardinal.North, Turn.Left) => Cardinal.West,
            (Cardinal.North, Turn.Right) => Cardinal.East,
            (Cardinal.North, Turn.Straight) => Cardinal.North,
            (Cardinal.North, Turn.UTurn) => Cardinal.South,

            (Cardinal.East, Turn.Left) => Cardinal.North,
            (Cardinal.East, Turn.Right) => Cardinal.South,
            (Cardinal.East, Turn.Straight) => Cardinal.East,
            (Cardinal.East, Turn.UTurn) => Cardinal.West,

            (Cardinal.South, Turn.Left) => Cardinal.East,
            (Cardinal.South, Turn.Right) => Cardinal.West,
            (Cardinal.South, Turn.Straight) => Cardinal.South,
            (Cardinal.South, Turn.UTurn) => Cardinal.North,

            (Cardinal.West, Turn.Left) => Cardinal.South,
            (Cardinal.West, Turn.Right) => Cardinal.North,
            (Cardinal.West, Turn.Straight) => Cardinal.West,
            (Cardinal.West, Turn.UTurn) => Cardinal.East,

            _ => throw new System.Exception("Invalid cardinal"),
        };
    }
}

public enum Turn
{
    Left,
    Right,
    Straight,
    UTurn,
}

public class Direction
{
    public Cardinal From;
    public Cardinal To;

    public Direction(Cardinal from, Cardinal to)
    {
        From = from;
        To = to;
    }
}

[Serializable]
public class Lane
{
    public int gid;
    public int Id = UID.Next();
    public Vector2 Position
    {
        get
        {
            var shift = (gid - 1) * road.Width / road.lanes.Count;
            Debug.Log($"Lane {gid} shift {shift}, gid {gid}, nLanes {road.lanes.Count}");
            return direction switch
            {
                { From: Cardinal.North, To: Cardinal.South } => new Vector2(road.Left + shift, road.Top),
                { From: Cardinal.South, To: Cardinal.North } => new Vector2(road.Left + shift, road.Bottom),
                { From: Cardinal.East, To: Cardinal.West } => new Vector2(road.Right, road.Top + shift),
                { From: Cardinal.West, To: Cardinal.East } => new Vector2(road.Left, road.Top + shift),
                _ => throw new System.Exception("Invalid direction"),
            };
        }
    }
    public Road road;
    public Direction direction;

    public Lane(Road road, Direction direction)
    {
        this.road = road;
        this.direction = direction;
    }

    public List<Turn> turns = new();
}

public class IntersectionLane
{
    public Lane lane;
    public Intersection intersection;
    public bool entering;
    public Cardinal attachTo;
    public int Gid => lane.gid;
    public Vector2 attachingPoint;

    public IntersectionLane pair;

    public List<IntersectionLane> next = new();
    public List<CarAgent> cars = new();

    private IntersectionLane() { }

    // this creates several lanes for each direction (in/out)
    public static List<IntersectionLane> Get(Lane lane, Intersection intersection)
    {
        var driveDirection = lane.direction.To;
        var nLanes = lane.road.lanes.Count;
        var partion = (float)((lane.gid - 1) * 2 + 1) / (float)(nLanes * 2);
        Debug.Log($"Lane {lane.gid} has partion {partion} (total {nLanes})");
        // when driveDirection is North or West, we need to flip the partion,
        // because the attaching point is from the opposite direction
        if (driveDirection == Cardinal.North || driveDirection == Cardinal.West)
        {
            partion = 1 - partion;
        }
        var laneWidth = lane.road.Width / nLanes;
        var inEdge = intersection.GetEdge(driveDirection.Opposite());
        var outEdge = intersection.GetEdge(driveDirection);

        static Vector2 EdgeLerp(Vector2 from, Vector2 to, float t)
        {
            return new Vector2(
                Mathf.Lerp(from.x, to.x, t),
                Mathf.Lerp(from.y, to.y, t)
            );
        }

        var inLane = new IntersectionLane()
        {
            lane = lane,
            intersection = intersection,
            entering = true,
            attachTo = driveDirection,
            attachingPoint = EdgeLerp(inEdge.from, inEdge.to, partion),
        };

        var outLane = new IntersectionLane()
        {
            lane = lane,
            intersection = intersection,
            entering = false,
            attachTo = driveDirection.Opposite(),
            attachingPoint = EdgeLerp(outEdge.to, outEdge.from, partion),
        };

        // DrawArrow.ForDebug(new Vector3(inLane.attachingPoint.x, 0, inLane.attachingPoint.y),
        // new Vector3(outLane.attachingPoint.x, 0, outLane.attachingPoint.y) - new Vector3(inLane.attachingPoint.x, 0, inLane.attachingPoint.y)
        // , Color.magenta, 1000);

        inLane.pair = outLane;
        outLane.pair = inLane;
        // Debug.Log($"{partion} Rd {lane.road.uid} Lane {lane.gid} has {inLane.attachTo} {inLane.attachingPoint} and {outLane.attachTo} {outLane.attachingPoint}");
        // Debug.Log($"inEdge {inEdge.from} {inEdge.to} outEdge {outEdge.from} {outEdge.to}");
        return new List<IntersectionLane>() { inLane, outLane };
    }

    public override string ToString()
    {
        return $"IntersectionLane({lane.gid}) {lane.direction}, {attachTo} of {lane.road.uid}, {next.Count} nexts";
    }

    public void AddCar(CarAgent car)
    {
        cars.Add(car);
    }

    public void RemoveCar(CarAgent car)
    {
        cars.Remove(car);
    }

}

public class Road
{
    public int uid = UID.Next();
    public Vector2 Position; //left upper cornerW
    public float Width;
    public float Length;
    public List<Intersection> intersections = new();

    public List<Lane> lanes = new();

    public Road(Vector2 position, float width, float length)
    {
        Position = position;
        Width = width;
        Length = length;
    }

    public Road Lane(Direction direction = null)
    {
        if (direction == null)
        {
            if (Width > Length)
            {
                direction = new Direction(Cardinal.East, Cardinal.West);
            }
            else
            {
                direction = new Direction(Cardinal.North, Cardinal.South);
            }
        }
        var lane = new Lane(this, direction)
        {
            gid = lanes.Count + 1
        };
        lanes.Add(lane);
        return this;
    }

    public Road Lane(Cardinal cardinal)
    {
        var direction = new Direction(cardinal, cardinal.Opposite());
        var lane = new Lane(this, direction)
        {
            gid = lanes.Count + 1
        };
        lanes.Add(lane);
        return this;
    }

    public Road Lane(Cardinal from, Cardinal to)
    {
        var direction = new Direction(from, to);
        var lane = new Lane(this, direction)
        {
            gid = lanes.Count + 1
        };
        lanes.Add(lane);
        return this;
    }

    public Road CanTurn(Turn turn)
    {
        lanes.Last().turns.Add(turn);
        return this;
    }

    public Road MakeBidirectional()
    {
        var lane = lanes.Last();
        var canTurns = lane.turns;
        var oppositeDirection = new Direction(lane.direction.To, lane.direction.From);
        lanes.Add(new Lane(this, oppositeDirection)
        {
            gid = lanes.Count + 1,
            turns = canTurns
        });
        return this;
    }

    public float Left => Position.x;
    public float Right => Position.x + Width;
    public float Top => Position.y;
    public float Bottom => Position.y + Length;
    public override string ToString()
    {
        return $"Road {uid} {Position} {Width} {Length} with {lanes.Count} lanes";
    }
}

static class RoadUtils
{
    public static void Show(this Road road)
    {
        Debug.DrawLine(new Vector3(road.Left, 0, road.Top), new Vector3(road.Right, 0, road.Top), Color.red, 1000);
        Debug.DrawLine(new Vector3(road.Right, 0, road.Top), new Vector3(road.Right, 0, road.Bottom), Color.red, 1000);
        Debug.DrawLine(new Vector3(road.Right, 0, road.Bottom), new Vector3(road.Left, 0, road.Bottom), Color.red, 1000);
        Debug.DrawLine(new Vector3(road.Left, 0, road.Bottom), new Vector3(road.Left, 0, road.Top), Color.red, 1000);
    }
}

class DebugUtils
{
    public static void DrawCircle(Vector3 position, float radius, Color color, float duration = 0.1f)
    {
        var segments = 16;
        var angle = 0f;
        var angleStep = Mathf.PI * 2f / segments;

        for (int i = 0; i < segments; i++)
        {
            var x1 = Mathf.Cos(angle) * radius;
            var y1 = Mathf.Sin(angle) * radius;
            angle += angleStep;
            var x2 = Mathf.Cos(angle) * radius;
            var y2 = Mathf.Sin(angle) * radius;

            Debug.DrawLine(new Vector3(x1, 0, y1) + position, new Vector3(x2, 0, y2) + position, color, duration);
        }
    }

}
public static class Tokens
{
    public static string DETECTOR = "detector";
    public static string ROAD = "road";
    public static string CAR = "car";
    public static string TRAFFIC_LIGHT = "traffic_light";
}

public class TrafficRouteBehavior : MonoBehaviour
{
    private void Start()
    {
        var roads = new List<Road>() {
            new Road(new Vector2(0, -5), 1, 11)
                .Lane(Cardinal.South, Cardinal.North)
                    .CanTurn(Turn.Left)
                    .CanTurn(Turn.Right)
                    .CanTurn(Turn.Straight)
                .Lane(Cardinal.North, Cardinal.South)
                    .CanTurn(Turn.Straight)
                .Lane(Cardinal.South, Cardinal.North)
                    .CanTurn(Turn.Straight)
                .Lane(Cardinal.North, Cardinal.South)
                    .CanTurn(Turn.Left)
                    .CanTurn(Turn.Right)
                    .CanTurn(Turn.Straight),

            new Road(new Vector2(-5, 0), 11, 1)
                .Lane(Cardinal.East, Cardinal.West)
                    .CanTurn(Turn.Left)
                    .CanTurn(Turn.Right)
                    .CanTurn(Turn.Straight)
                // .Lane(Cardinal.East, Cardinal.West)
                //     .CanTurn(Turn.Straight)
                // .Lane(Cardinal.West, Cardinal.East)
                //     .CanTurn(Turn.Straight)
                .Lane(Cardinal.West, Cardinal.East)
                    .CanTurn(Turn.Left)
                    .CanTurn(Turn.Right)
                    .CanTurn(Turn.Straight),
        };

        CalculateIntersections(roads);
        roads.ForEach(Debug.Log);
        roads.ForEach(x => { x.Show(); });
        // Debug.Log($"Roads {roads.Count}, {roads.First().intersections.Count} intersections");
        roads.ForEach(x => x.lanes.ForEach(y => Debug.Log(y.gid)));
        roads.First().intersections.ForEach(x => x.Show());

        GenerateRoads(roads);
    }

    public Material roadMaterial;
    private List<GameObject> _roads = new List<GameObject>();
    private List<GameObject> _detectors = new List<GameObject>();

    void GenerateRoads(List<Road> roads)
    {
        if (UnityEditorInternal.InternalEditorUtility.tags.FirstOrDefault(x => x == Tokens.ROAD) == null)
        {
            UnityEditorInternal.InternalEditorUtility.AddTag(Tokens.ROAD);
        }
        foreach (var road in roads)
        {
            // Roads are created as a plane with the width and length of the road
            var roadObject = new GameObject($"Road {road.uid}");
            roadObject.transform.position = new Vector3(road.Position.x, 0, road.Position.y);
            roadObject.AddComponent<MeshFilter>();
            roadObject.AddComponent<MeshRenderer>();
            roadObject.AddComponent<BoxCollider>();
            roadObject.GetComponent<MeshRenderer>().material = roadMaterial;
            roadObject.tag = Tokens.ROAD;

            // Set up the mesh filter with a simple plane mesh
            MeshFilter meshFilter = roadObject.GetComponent<MeshFilter>();
            Mesh mesh = new();
            meshFilter.mesh = mesh;

            Vector3[] vertices = new Vector3[4];
            vertices[0] = new Vector3(0, 0, 0);
            vertices[1] = new Vector3(0, 0, road.Length);
            vertices[2] = new Vector3(road.Width, 0, 0);
            vertices[3] = new Vector3(road.Width, 0, road.Length);

            mesh.vertices = vertices;

            int[] triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
            mesh.triangles = triangles;

            mesh.RecalculateNormals();
            // save this mesh to a prefab
            AssetDatabase.CreateAsset(mesh, $"Assets/temp/Road {road.uid}.asset");

            _roads.Add(roadObject);
        }

        GenerateDetectors(roads);
    }
    [ContextMenu("Dump Roads")]
    void DumpRoadsAndDetectors()
    {
        var root = new GameObject("Roads");
        foreach (var road in _roads)
        {
            road.transform.parent = root.transform;
        }

        foreach (var detector in _detectors)
        {
            detector.transform.parent = root.transform;
        }

        // var deps = EditorUtility.CollectDependencies(new Object[] { root });

        PrefabUtility.SaveAsPrefabAsset(root, "Assets/Roads.prefab");
    }

    // generate detectors for each lane
    // once car enter/exit the detector, it will be added/removed from the lane
    // events will be fired when car enter/exit the lane
    // detectors are generated by IntersectionLanes
    public float detector_length = 1f;

    void GenerateDetectors(List<Road> roads)
    {
        if (UnityEditorInternal.InternalEditorUtility.tags.FirstOrDefault(x => x == Tokens.DETECTOR) == null)
        {
            UnityEditorInternal.InternalEditorUtility.AddTag(Tokens.DETECTOR);
        }

        HashSet<Intersection> intersections = new();
        foreach (var road in roads)
        {
            intersections.UnionWith(road.intersections);
        }

        foreach (var intersection in intersections)
        {
            foreach (var intersectionLane in intersection.intersectionLanes.Values.SelectMany(x => x))
            {
                // generate detector for each lane
                var lane = intersectionLane.lane;
                var direction = lane.direction;
                var from = direction.From;
                var to = direction.To;
                var laneWidth = Mathf.Min(lane.road.Width, lane.road.Length) / lane.road.lanes.Count;
                var attachingPoint = intersectionLane.attachingPoint;

                var detector = new GameObject($"Detector {lane.road.uid} {lane.gid} {from} {to}");
                detector.transform.position = new Vector3(attachingPoint.x, 0, attachingPoint.y);
                detector.transform.rotation = Quaternion.LookRotation(new Vector3(to switch
                {
                    Cardinal.North => 0,
                    Cardinal.East => 1,
                    Cardinal.South => 0,
                    Cardinal.West => -1,
                    _ => throw new System.Exception("Invalid cardinal"),
                }, 0, from switch
                {
                    Cardinal.North => 1,
                    Cardinal.East => 0,
                    Cardinal.South => -1,
                    Cardinal.West => 0,
                    _ => throw new System.Exception("Invalid cardinal"),
                }));

                // collider is a box collider with the length of the detector
                var collider = detector.AddComponent<BoxCollider>();
                collider.size = new Vector3(laneWidth, 0.1f, detector_length);
                collider.isTrigger = true;
                collider.tag = Tokens.DETECTOR;
                // detector has a DetectorBehavior component
                // var detectorBehavior = detector.AddComponent<DetectorBehavior>();
                // detectorBehavior.lane = intersectionLane.ToString();
                // detectorBehavior.next = intersectionLane.next.Select(x => x.ToString()).ToList();
                // detectorBehavior.test = 1f;

                _detectors.Add(detector);
            }
        }
    }

    public void CalculateIntersections(List<Road> roads)
    {
        for (int i = 0; i < roads.Count; i++)
        {
            Road roadA = roads[i];
            for (int j = i + 1; j < roads.Count; j++)
            {
                Road roadB = roads[j];
                if (CheckIntersection(roadA, roadB, out Intersection intersection))
                {
                    AddIntersection(roadA, roadB, intersection);
                }
            }
        }
    }

    private bool CheckIntersection(Road roadA, Road roadB, out Intersection intersection)
    {
        intersection = null;
        if (roadA.Left > roadB.Left) // Make sure roadA is on the left
        {
            (roadB, roadA) = (roadA, roadB);
        }

        // Check if the boundaries intersect
        if (roadA.Left < roadB.Right &&
            roadA.Right > roadB.Left &&
            roadA.Top < roadB.Bottom &&
            roadA.Bottom > roadB.Top)
        {
            // Intersection detected
            intersection = new Intersection(roadA, roadB);
            return true;
        }
        return false; // No intersection
    }

    private void AddIntersection(Road roadA, Road roadB, Intersection intersection)
    {
        roadA.intersections.Add(intersection);
        roadB.intersections.Add(intersection);
    }
}