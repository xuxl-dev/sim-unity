using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class humanInput : MonoBehaviour
{
    public PhyCarController controller;
    void Start()
    {
        controller = GetComponent<PhyCarController>();
    }

    // Update is called once per frame
    void Update()
    {
        float motor = 0;
        float steering = 0;
        float brake = 0;

        if (Input.GetKey(KeyCode.W))
        {
            motor = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            motor = -1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            steering = -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            steering = 1;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            brake = 1;
        }

        Heuristic(motor, steering, brake);
    }

    
    public void Heuristic(float motor, float steering, float brake)
    {
        motor = Mathf.Clamp(motor, -1f, 1f);
        steering = Mathf.Clamp(steering, -1f, 1f);
        brake = Mathf.Clamp(brake, 0, 1);

        controller.SetMotorAndSteering(motor, steering);
        controller.SetBrake(brake);
    }
}
