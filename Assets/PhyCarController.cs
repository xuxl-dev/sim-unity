using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;
    public bool steering;
    public bool brake;
}

public class PhyCarController : MonoBehaviour
{
    public List<AxleInfo> axleInfos;
    public float maxMotorTorque;
    public float maxSteeringAngle;
    public float maxBrakeTorque;

    internal float motor = 0;
    internal float steering = 0;
    internal float brake = 0;

    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0);

        collider.GetWorldPose(out Vector3 position, out Quaternion rotation);

        visualWheel.transform.SetPositionAndRotation(position, rotation);
    }

    public void FixedUpdate()
    {
        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering * maxSteeringAngle;
                axleInfo.rightWheel.steerAngle = steering * maxSteeringAngle;
            }
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor * maxMotorTorque;
                axleInfo.rightWheel.motorTorque = motor * maxMotorTorque;
            }
            if (axleInfo.brake)
            {
                axleInfo.leftWheel.brakeTorque = brake * maxBrakeTorque;
                axleInfo.rightWheel.brakeTorque = brake * maxBrakeTorque;
            }

            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
    }

    public void SetMotor(float motor)
    {
        this.motor = motor;
    }

    public void SetSteering(float steering)
    {
        this.steering = steering;
    }

    public void SetBrake(float brake)
    {
        this.brake = brake;
    }

    public void SetMotorAndSteering(float motor, float steering)
    {
        this.motor = motor;
        this.steering = steering;
    }
}