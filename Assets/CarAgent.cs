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


public class CarAgent : Agent
{
    private Rigidbody rBody;
    private TrafficSettings trafficSettings;
    internal Vector3 Target;
    internal TrafficController trafficController;
    BufferSensorComponent bufferSensor;
    internal float[] broadcastBuffer = new float[20];

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

    float roadLength = 100f;
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
                bufferSensor.AppendObservation(car.agent.broadcastBuffer);
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

        float[] broadcast = new float[13 + 7];
        // 0~4: info about this car
        broadcast[0] = throttle;
        broadcast[1] = steering;
        broadcast[2] = brake;
        broadcast[3] = carInfo.priority / trafficSettings.max_priority;
        broadcast[4] = carInfo.priority / trafficSettings.max_priority;
        // 5~6: info about the target
        broadcast[5] = (Target - transform.position).x / roadLength;
        broadcast[6] = (Target - transform.position).z / roadLength;
        // 7~20: reserved info copied from actionBuffers[3..15]
        for (int i = 0; i < 13; i++)
        {
            broadcast[7 + i] = actionBuffers.ContinuousActions[i + 3];
        }

        broadcastBuffer = broadcast;
    }

    // float max_speed_reward = 0.3f;
    // float cur_speed_reward = 0.3f;

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
        if (detector.next != null)
        {
            previousLane = currentLane;
            currentLane = detector.lane;

            if (previousLane != null)
            {
                // check if a valid turn
                if (detector.next.Contains(previousLane))
                {
                    // valid turn
                    Debug.Log("Valid turn" + previousLane + " to " + currentLane);
                }
                else
                {
                    // invalid turn
                    Debug.Log("Invalid turn from " + previousLane + " to " + currentLane);
                }
            }
        }
        else
        {
            Debug.Log("Detector has no intersection lane" + detector);
        }
    }

    internal void OnDetectorExit(DetectorBehavior detectorBehavior)
    {

    }


}
