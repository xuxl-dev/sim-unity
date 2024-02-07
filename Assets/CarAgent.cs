using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.AI;
using UnityEditor;
using Unity.Sentis.Layers;
using System;
using Grpc.Core;


public class CarAgent : Agent
{

    private Rigidbody rBody;
    private TrafficSettings trafficSettings;
    internal Vector3 Target;
    internal TrafficController trafficController;
    BufferSensorComponent bufferSensor;
    internal List<float[]> broadcasts = new();

    internal TrafficController.CarInfo carInfo;

    public override void Initialize()
    {
        bufferSensor = GetComponent<BufferSensorComponent>();
        rBody = GetComponent<Rigidbody>();
        trafficSettings = transform.parent.parent.GetComponent<TrafficSettings>();
    }



    void Start()
    {
    }

    void FixedUpdate()
    {
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(
            Target - transform.position
        );

        // add all cars in the scene
        var cars = trafficController.CarsList;
        foreach (var car in cars)
        {
            if (car.isActivated)
            {
                foreach (var ob in car.agent.broadcasts)
                {
                    if (Vector3.Distance(transform.position, car.agent.transform.position) < trafficSettings.broadcast_clip_radius)
                    {
                        bufferSensor.AppendObservation(ob);
                    }
                }
            }
        }
    }

    float LinearMapping(float x, float x1, float x2, float y1, float y2)
    {
        return (x - x1) / (x2 - x1) * (y2 - y1) + y1;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float throttle = actionBuffers.ContinuousActions[0];
        throttle = Mathf.Clamp(throttle, -1f, 1f);
        // linear mapping to [0.2, 1.0], because low speed is not wanted
        throttle = LinearMapping(throttle, -1f, 1f, 0.2f, 1.0f);

        rBody.AddForce(throttle * trafficSettings.accelerateMultiplier * transform.forward);

        float steering = actionBuffers.ContinuousActions[1];
        steering = Mathf.Clamp(steering, -1f, 1f);
        // steering = 0f;
        // actually, steering here can not go 360 degrees, but only 180 degrees
        // so instead of modifying the angular velocity, we add a force to the rigidbody
        rBody.AddForce(steering * trafficSettings.steeringMultiplier * transform.right, ForceMode.Force);
        rBody.angularVelocity = new Vector3(0f, trafficSettings.steeringMultiplier * steering, 0f);

        // float brake = actionBuffers.ContinuousActions[2];
        // brake = Mathf.Clamp(brake, 0f, 1f);
        // if (brake > 0)
        // {
        //     rBody.velocity *= brake;
        // }
        int brake = actionBuffers.DiscreteActions[0];
        if (brake == 1)
        {
            rBody.velocity = rBody.velocity.normalized * (rBody.velocity.magnitude * 0.9f);
        }


        if (rBody.velocity.magnitude > trafficSettings.maxVelocity)
        {
            rBody.velocity = rBody.velocity.normalized * trafficSettings.maxVelocity;
        }

        if (rBody.angularVelocity.magnitude > trafficSettings.maxAngularVelocity)
        {
            rBody.angularVelocity = rBody.angularVelocity.normalized * trafficSettings.maxAngularVelocity;
        }

        var selfInfo = new BroadcastInfoBuilder(16)
            .AsType(BroadcastInfoBuilder.BroadcastInfoBuilderType.VehicleObservation)
            .Add(throttle)
            .Add(steering)
            .Add(transform.position.x)
            .Add(transform.position.z)
            .Add(transform.rotation.eulerAngles.y)
            .Add(Target.x)
            .Add(Target.z);

        var events = new BroadcastInfoBuilder(16)
            .AsType(BroadcastInfoBuilder.BroadcastInfoBuilderType.BroadcastEvents)
            .Add(brake);

        broadcasts.Add(selfInfo.Build());
        broadcasts.Add(events.Build());

        Sio.Emit("brake", new
        {
            @event = "brake",
            payload = brake == 1
        });

        Sio.Emit("status", new
        {
            @event = "status",
            payload = new
            {
                speed = rBody.velocity.magnitude,
                position = rBody.position.ToObject(),
                rotation = rBody.rotation.ToObject(),
            }
        });
    }
    int step = 0;

    public override void Heuristic(in ActionBuffers actionBuffers)
    {
        actionBuffers.ContinuousActions.Array[0] = Input.GetAxis("Vertical");
        actionBuffers.ContinuousActions.Array[1] = Input.GetAxis("Horizontal");
        // actionBuffers.DiscreteActions.Array[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;

        float acceleration = actionBuffers.ContinuousActions[0];
        float steering = actionBuffers.ContinuousActions[1];

        acceleration = Mathf.Clamp(acceleration, -1f, 1f);
        steering = Mathf.Clamp(steering, -1f, 1f);

        rBody.AddForce(acceleration * trafficSettings.accelerateMultiplier * transform.forward);
        rBody.angularVelocity = new Vector3(0f, trafficSettings.steeringMultiplier * steering, 0f);


        if (rBody.velocity.magnitude > trafficSettings.maxVelocity)
        {
            rBody.velocity = rBody.velocity.normalized * trafficSettings.maxVelocity;
        }

        if (rBody.angularVelocity.magnitude > trafficSettings.maxAngularVelocity)
        {
            rBody.angularVelocity = rBody.angularVelocity.normalized * trafficSettings.maxAngularVelocity;
        }
    }

    private string previousLane = null;
    private string currentLane = null;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("car"))
        {
            trafficController.CarCollision(this, other.gameObject.GetComponent<CarAgent>());
        }
    }

    private void OnCollisionExit(Collision other)
    {

    }

    internal void OnDetectorEnter(DetectorBehavior detector)
    {
        if (detector.CompareTag(Tokens.TRAFFIC_LIGHT))
        {
            if (detector.TryGetComponent<TrafficLightBehavior>(out var trafficLight))
            {
                trafficLight.cars.Add(this);
                Sio.Emit("trafficlight-detector", new
                {
                    type = "event",
                    @event = "trafficlight-detector",
                    payload = new
                    {
                        state = "entering",
                    }
                });
            }
        }
    }
    internal void OnDetectorExit(DetectorBehavior detector)
    {
        if (detector.CompareTag(Tokens.TRAFFIC_LIGHT))
        {
            if (detector.TryGetComponent<TrafficLightBehavior>(out var trafficLight))
            {
                trafficLight.cars.Remove(this);
                Sio.Emit("trafficlight-detector", new
                {
                    type = "event",
                    @event = "trafficlight-detector",
                    payload = new
                    {
                        state = "leaving",
                    }
                });
            }
        }
    }
}
