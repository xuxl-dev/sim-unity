using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static Clicker;

public class TrafficController : MonoBehaviour
{

    [System.Serializable]
    public class CarInfo
    {
        public CarAgent agent;
        public Vector3 target = Vector3.zero;

        [Range(0f, 10f)]
        public float priority = 1f;
        internal bool isActivated;

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

    public List<CarInfo> CarsList = new();

    private SimpleMultiAgentGroup agentGroup;
    private TrafficSettings trafficSettings;

    public bool UseRandomSpawnPos = true;

    void Start()
    {
        trafficSettings = GetComponent<TrafficSettings>();
        if (trafficSettings == null)
        {
            Debug.LogError("TrafficSettings not found");
            return;
        }
        agentGroup = new SimpleMultiAgentGroup();
        foreach (var car in CarsList)
        {
            car.T = car.agent.transform;
            car.gameObject = car.agent.gameObject;
            car.StartingPos = car.T.position;
            car.StartingRot = car.T.rotation;
            car.Rb = car.agent.GetComponent<Rigidbody>();
            car.isActivated = true;
            car.agent.trafficController = this;
            car.agent.carInfo = car;
            agentGroup.RegisterAgent(car.agent);
        }

        ResetScene();
    }

    [Header("Max Environment Steps")] public int MaxEnvironmentSteps = 30000;

    private int resetTimer;
    private int activeAgents = 0;
    private int successAgents = 0;

    private void FixedUpdate()
    {
        // if (resetTimer % 10 == 0)
        // {
        //     Debug.Log($"resetTimer: {resetTimer}, activeAgents: {activeAgents}, successAgents: {successAgents}");
        // }

        if (activeAgents == 0)
        {
            agentGroup.EndGroupEpisode();
            ResetScene();
        }

        resetTimer += 1;
        if (resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            // Debug.Log("Max Environment Steps Reached");
            agentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }

        CheckTargetReached();
        CheckFalloff();

        //Hurry Up Penalty
        agentGroup.AddGroupReward(-0.5f / MaxEnvironmentSteps);
        CarsList.ForEach(car =>
        {
            if (car.isActivated)
            {
                // hurry up penalty
                // car.agent.AddReward(-0.5f / MaxEnvironmentSteps);
                // forward speed reward
                car.agent.AddReward(
                    Vector3.Dot(car.T.forward.normalized, car.Rb.velocity.normalized) * 0.3f / MaxEnvironmentSteps
                );
            }
        });
    }

    void CheckTargetReached()
    {
        foreach (var car in CarsList)
        {
            if (car.isActivated && Vector3.Distance(car.T.position, car.target + this.transform.position) < 3f)
            {
                // Debug.Log("Target Reached");
                car.agent.SetReward(5f - (float)resetTimer / MaxEnvironmentSteps);
                agentGroup.AddGroupReward(1f / CarsList.Count);
                successAgents++;
                Deactivate(car);
            }
        }
    }

    void CheckFalloff()
    {
        foreach (var car in CarsList)
        {
            if (car.isActivated && car.T.localPosition.y < 0f)
            {
                car.agent.SetReward(-1f);
                Deactivate(car);
            }
        }
    }

    public void CarCollision(CarAgent car1, CarAgent _)
    {
        // Debug.Log("Crush");
        car1.SetReward(-1f);
        Deactivate(car1.carInfo);
    }

    private void Deactivate(CarInfo car)
    {
        // in case has been deactivated already
        if (car.isActivated == false)
        {
            return;
        }
        car.isActivated = false;
        car.gameObject.SetActive(false);
        activeAgents -= 1;
    }

    [ContextMenu("Reset Scene")]
    public void ResetScene()
    {
        // Debug.Log("Reset Scene");
        resetTimer = 0;

        //Reset Agents
        ResetAllAgents();

        activeAgents = CarsList.Count;
        successAgents = 0;
    }

    Quaternion GetRandomRot()
    {
        return Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0);
    }

    public Vector3 GetRandomSpawnPos(Vector3 from, Vector3 to)
    {
        var foundNewSpawnLocation = false;
        var randomSpawnPos = Vector3.zero;
        while (foundNewSpawnLocation == false)
        {
            randomSpawnPos = Vector3Lerp(from, to, Random.Range(0f, 1f));

            if (Physics.CheckBox(randomSpawnPos, new Vector3(1.5f, 0.01f, 1.5f)) == false)
            {
                foundNewSpawnLocation = true;
            }
        }
        return randomSpawnPos;
    }

    private Vector3 Vector3Lerp(Vector3 from, Vector3 to, float v)
    {
        return new Vector3(
            Mathf.Lerp(from.x, to.x, v),
            Mathf.Lerp(from.y, to.y, v),
            Mathf.Lerp(from.z, to.z, v)
        );
    }

    static float car_spawn_y_offset = 0.5f;
    public List<Vec3Pair> pairs = new();

    private (Vector3 start, Vector3 target, Vector3 lookingAt) GenerateRandomStartAndTarget()
    {
        var retry = 0;
        // pick one road
        // pick 2 random point on the road
        var road = new Vec3Pair();
        var start = Vector3.zero;
        bool foundStartPos = false;
        float partition = 0f;
        while (foundStartPos == false && retry < 150)
        {
            partition = Random.Range(0f, 0.4f);
            road = pairs[Random.Range(0, pairs.Count)];

            start = Vector3Lerp(road.start, road.end, partition);
            if (Physics.CheckBox(this.transform.position + start + new Vector3(0, 0.5f, 0), new Vector3(4f, 0.1f, 4f)) == false)
            {
                foundStartPos = true;
            }
            ++retry;
        }
        if (foundStartPos == false)
        {
            Debug.LogError("Can't find start pos");
            return (Vector3.zero, Vector3.zero, Vector3.zero);
        }

        var target = Vector3Lerp(road.start, road.end, 1f - partition);

        return (start, this.transform.position + target, (road.end - road.start).normalized);
    }

    private void ResetAgentRnd(CarInfo car)
    {
        var (start, target, lookingAt) = GenerateRandomStartAndTarget();
        // Debug.Log($"start: {start}, target: {target}, lookingAt: {lookingAt}");
        car.agent.transform.localPosition = start + new Vector3(0, car_spawn_y_offset, 0);
        car.agent.transform.localRotation = Quaternion.LookRotation(lookingAt);
        car.target = target;
        car.Rb.velocity = Vector3.zero;
        car.Rb.angularVelocity = Vector3.zero;
        car.agent.gameObject.SetActive(true);
        car.isActivated = true;
        agentGroup.RegisterAgent(car.agent);
    }

    private void ResetAgent(CarInfo car)
    {
        car.agent.transform.position = car.StartingPos;
        car.agent.transform.rotation = car.StartingRot;
        car.Rb.velocity = Vector3.zero;
        car.Rb.angularVelocity = Vector3.zero;
        car.agent.gameObject.SetActive(true);
        car.isActivated = true;

        agentGroup.RegisterAgent(car.agent);
    }

    IEnumerator ResetAgentCoroutine(CarInfo car, int wait_ms)
    {
        yield return new WaitForSeconds(wait_ms / 1000f);
        ResetAgentRnd(car);
    }

    [ContextMenu("!!!Reset All Agents Pos")]
    private void ResetAllAgents()
    {
        if (!UseRandomSpawnPos)
        {
            foreach (var agent in CarsList)
            {
                ResetAgent(agent);
            }
        }
        else
        {
            // disable all agents
            foreach (var agent in CarsList)
            {
                agent.isActivated = false;
                agent.gameObject.SetActive(false);
            }

            foreach (var agent in CarsList)
            {
                ResetAgentRnd(agent);
            }
        }
    }
}
