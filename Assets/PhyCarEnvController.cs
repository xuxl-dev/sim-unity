using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class PhyCarEnvController : MonoBehaviour
{
    private SimpleMultiAgentGroup agentGroup;
    int step = 0;
    int max_steps = 10000;
    [System.Serializable]
    public class PhyCarInfo
    {
        public PhyCar agent;
        public Vector3 target = Vector3.zero;
        [HideInInspector]
        public Transform T;
        [HideInInspector]
        public GameObject gameObject;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }
    public List<GameObject> lanes = new();

    [System.Serializable]
    public class Lane
    {
        public Vector3 beg;
        public Vector3 end;
        public Vector3 direction;
        public bool reversed;

        public Lane(Vector3 beg, Vector3 end, Vector3 direction, bool reversed)
        {
            this.beg = beg;
            this.end = end;
            this.direction = direction;
            this.reversed = reversed;
        }
    }
    public List<Lane> lane_coords = new();

    public List<PhyCarInfo> CarsList = new();
    internal PhyCarEnvSettings settings;

    [ContextMenu("Auto Add Cars")]
    void AutoAddCars()
    {
        CarsList.Clear();
        var cars = FindObjectsOfType<PhyCar>();
        foreach (var car in cars)
        {
            CarsList.Add(new PhyCarInfo
            {
                agent = car,
                target = Vector3.zero
            });
        }
    }

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
                lane_coords.Add(new Lane(beg, end, direction, reversed));
            }
            else
            {
                var beg = boundingBox.center - new Vector3(0, 0, boundingBox.extents.z);
                var end = boundingBox.center + new Vector3(0, 0, boundingBox.extents.z);
                var direction = (end - beg).normalized;
                var reversed = false; // this is assgined by user later
                lane_coords.Add(new Lane(beg, end, direction, reversed));
            }
        }
    }

    [ContextMenu("ShowCoords")]
    void ShowCoords()
    {
        Debug.Log("lane_coords.Count: " + lane_coords.Count);
        foreach (var lane in lane_coords)
        {
            var beg = lane.beg;
            var end = lane.end;
            var direction = lane.direction;
            var reversed = lane.reversed;
            Debug.DrawLine(beg, end, Color.red, 5);
            Debug.Log($"beg: {beg}, end: {end}, direction: {direction}, reversed: {reversed}");
        }
    }

    void Start()
    {
        settings = FindObjectOfType<PhyCarEnvSettings>();
        foreach (var car in CarsList)
        {
            car.agent.info = car;
            car.T = car.agent.transform;
            car.gameObject = car.agent.gameObject;
            car.StartingPos = car.T.position;
            car.StartingRot = car.T.rotation;
            car.Rb = car.agent.GetComponent<Rigidbody>();
            car.agent.OnCollision += HandleCollision;
            car.agent.OnDrop += HandleDrop;
            car.agent.OnMove += (c) =>
            {
                if (Vector3.Distance(c.info.T.position, c.info.target) < 3f)
                {
                    HandleTargetReached(c);
                }
            };
        }
        ResetScene();
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        // time penalty, being fast is good
        agentGroup.AddGroupReward(-0.5f / max_steps);

        step++;
        if (step > max_steps)
        {
            agentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }
    }

    void ResetScene()
    {
        step = 0;
        if (settings.UseRandomSpawnPos)
        {
            foreach (var car in CarsList)
            {
                var (start, target, direction) = GenerateRandomStartAndTarget();
                car.T.position = start;
                car.target = target;
                car.T.rotation = Quaternion.LookRotation(direction);
                car.Rb.velocity = Vector3.zero;
                car.Rb.angularVelocity = Vector3.zero;
            }
        }
        else
        {
            foreach (var car in CarsList)
            {
                car.T.position = car.StartingPos;
                car.T.rotation = car.StartingRot;
                car.Rb.velocity = Vector3.zero;
                car.Rb.angularVelocity = Vector3.zero;
            }
        }

        foreach (var car in CarsList)
        {
            agentGroup.RegisterAgent(car.agent);
        }
    }

    private (Vector3 start, Vector3 target, Vector3 direction) GenerateRandomStartAndTarget()
    {
        var checkOccupied = new System.Func<Vector3, bool>((pos) =>
        {
            return Physics.CheckBox(pos + new Vector3(0, 0.5f, 0), new Vector3(4f, 0.1f, 4f));
        });

        for (int retry = 0; retry < 150; retry++)
        {
            var lane = lane_coords[Random.Range(0, lane_coords.Count)];
            var partition = Random.Range(0f, 0.4f);
            var start = Vector3.Lerp(lane.beg, lane.end, partition);
            var target = Vector3.Lerp(lane.beg, lane.end, 1f - partition);
            if (checkOccupied(start) == false && checkOccupied(target) == false)
            {
                return (start, target, lane.direction);
            }
        }

        return (Vector3.zero, Vector3.zero, Vector3.zero);
    }

    void HandleCollision(PhyCar car, Collision other)
    {
        if (other.gameObject.CompareTag("car"))
        {
            car.SetReward(-1f);
            car.gameObject.SetActive(false);
            // we dont want to disable the other car, because
            // collision is mutual
        }

        if (other.gameObject.CompareTag("obstacle"))
        {
            car.SetReward(-1f);
            car.gameObject.SetActive(false);
        }
    }

    void HandleDrop(PhyCar car)
    {
        car.SetReward(-1f);
        car.gameObject.SetActive(false);
    }

    void HandleTargetReached(PhyCar car)
    {
        car.SetReward(5f);
        car.gameObject.SetActive(false);
    }

}
