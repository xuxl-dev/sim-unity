using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PhyCar : Agent
{
    public PhyCarEnvController.PhyCarInfo info;
    [NonSerialized]
    static int _id = 0;
    [NonSerialized]
    public int id = -1;
    public string Name => $"CarAgent-{id}";
    PhyCarController controller;
    TextMeshPro text;
    Vector3 CarCenterShift;
    TrafficLightBehaviorPhy TrafficLight;
    Rigidbody rb;
    internal string cam_id;
    static Dictionary<string, PhyCar> cars = new();
    static string focused_car = "none";
    static float focus_distance = 10f;

    void Start()
    {
        id = _id++;
        Debug.Log("PhyCar start of id: " + id);
        cars[Name] = this;
        controller = GetComponent<PhyCarController>();
        text = transform.Find("text").GetChild(0).GetComponent<TextMeshPro>();
        CarCenterShift = CUtils.GetBounds(transform.Find("body").gameObject).center - this.transform.position;
        rb = GetComponent<Rigidbody>();
        cam_id = Zoomer.Instance.Register(transform.Find("vc").GetComponent<CinemachineVirtualCamera>());

        PhyEnvReporter.Instance.Subscribe(new PhyEnvReporter.SubscriberConfig
        {
            @event = "status",
            data = GetBriefFunc(),
            id = Name,
        });

        PhyEnvReporter.Instance.Subscribe(new PhyEnvReporter.SubscriberConfig
        {
            @event = "focused-nearby-cars",
            data = GetBriefFunc(),
            id = Name,
            filter = (_) =>
            {
                return
                    focused_car != "none" &&
                    Vector3.Distance(transform.position, cars[focused_car].transform.position) < focus_distance;
            }
        });

        Func<object> GetBriefFunc()
        {
            return () => new
            {
                motor = controller.motor,
                steering = controller.steering,
                brake = controller.brake,
                speed = rb.velocity.magnitude,
                speed3 = rb.velocity.ToObject(),
                angular_speed = rb.angularVelocity.ToObject(),
                position = transform.position.ToObject(),
                rotation = transform.rotation.ToObject(),
                offset = GetOffsetToCenterLine(),
            };
        }
    }

    void Update()
    {
        var offset = GetOffsetToCenterLine();

        // when nan, this indicates the car is not on the road
        if (float.IsNaN(offset))
        {
            OnDrop?.Invoke(this);
        }

        var formatFloat = new Func<float, string>((f) =>
        {
            if (float.IsNaN(f))
            {
                return "NaN";
            }
            else
            {
                return f.ToString("0.00");
            }
        });
        text.text = $"motor: {controller.motor:0.00} (v: {this.rb.velocity})\n" +
            $"steering: {controller.steering:0.00}\n" +
            $"brake: {controller.brake:0.00}\n" +
            $"offset: {formatFloat(offset)}\n" +
            $"traffic light: {(TrafficLight != null ? TrafficLight.current_color : null) ?? "[UNK]"}";
    }

    void FixedUpdate()
    {
        OnMove?.Invoke(this);

        if (this.transform.position.y < -1f)
        {
            OnDrop?.Invoke(this);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // total __ observations
        // * traffic light observation (1+1+1+1+3)
        //   0: red, 1: yellow, 2: green, 3: none
        //   4-6: relitive position to traffic light ((0,0,0) if no traffic light)

        // * internal status (1+1+1)
        //   7: motor
        //   8: steering
        //   9: brake

        // * physics status (1+1+3)
        //   10: speed
        //   11: angular speed
        //   12-15: position

        // * lane status (1)
        //   16: offset to center line

        // * target position (3)
        //   17-19: target position


        // variable length observations
        // broadcast car internal status from other cars
        //TODO ...

        // broadcast event from other cars
        //TODO ...

    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float motor = actionBuffers.ContinuousActions[0];
        float steering = actionBuffers.ContinuousActions[1];
        int brake = actionBuffers.DiscreteActions[0];

        motor = Mathf.Clamp(motor, -1f, 1f);
        // map motor to [0.2, 1.0], because low speed is not wanted
        motor = CUtils.LinearMapping(motor, -1f, 1f, 0.2f, 1.0f);
        steering = Mathf.Clamp(steering, -1f, 1f);
        brake = Mathf.Clamp(brake, 0, 1);

        controller.SetMotorAndSteering(motor, steering);
        controller.SetBrake(brake);
    }

    public override void Heuristic(in ActionBuffers actionBuffers)
    {
        actionBuffers.ContinuousActions.Array[0] = Input.GetAxis("Vertical");
        actionBuffers.ContinuousActions.Array[1] = Input.GetAxis("Horizontal");
        actionBuffers.DiscreteActions.Array[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;

        float motor = actionBuffers.ContinuousActions[0];
        float steering = actionBuffers.ContinuousActions[1];
        int brake = actionBuffers.DiscreteActions[0];

        motor = Mathf.Clamp(motor, -1f, 1f);
        steering = Mathf.Clamp(steering, -1f, 1f);
        brake = Mathf.Clamp(brake, 0, 1);

        controller.SetMotorAndSteering(motor, steering);
        controller.SetBrake(brake);
    }

    // get bias to center of current lane
    GameObject old_lane;
    private float GetOffsetToCenterLine()
    {
        RaycastHit[] hits = new RaycastHit[4]; // max 4 lanes
        // draw detection line
        // cast a ray top down to get the current lane
        if (Physics.RaycastNonAlloc(
            transform.position + CarCenterShift + new Vector3(0, 4, 0),
            Vector3.down,
            hits,
            20f,
            LayerMask.GetMask("lane")
        ) > 0)
        {
            if (Array.Exists(hits, hit => hit.collider == old_lane))
            {
                if (old_lane == null)
                {
                    old_lane = hits.First(hit => hit.collider != null).collider.gameObject;
                }
                // still in the same lane
                return old_lane.GetComponent<LaneBehavior>().GetOffsetToCenterLine(transform.position);
            }
            else
            {
                // enter a new lane
                var lane = hits.First(hit => hit.collider != null).collider.gameObject;
                old_lane = lane;
                return lane.GetComponent<LaneBehavior>().GetOffsetToCenterLine(transform.position);
            }
        }
        return float.NaN;
    }

    private void OnCollisionEnter(Collision other)
    {
        OnCollision?.Invoke(this, other);
    }

    private void OnCollisionExit(Collision other)
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(Tokens.TRAFFIC_LIGHT))
        {
            if (other.gameObject.TryGetComponent<TrafficLightBehaviorPhy>(out var trafficLight))
            {
                TrafficLight = trafficLight;
                trafficLight.CarEnter(this);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(Tokens.TRAFFIC_LIGHT))
        {
            if (other.gameObject.TryGetComponent<TrafficLightBehaviorPhy>(out var trafficLight))
            {
                TrafficLight = null;
                trafficLight.CarExit(this);
            }
        }
    }

    public void OnMouseDown()
    {
        Zoomer.Instance.Zoom(cam_id);
        PhyEnvReporter.Instance.Push("focused-car", new { obj = Name });
    }

    public event Action<PhyCar, Collision> OnCollision;
    public event Action<PhyCar> OnDrop;
    public event Action<PhyCar> OnTrafficRuleViolation;
    public event Action<PhyCar> OnMove;
}
