using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightBehaviorPhy : MonoBehaviour
{
    public Material red;
    public Material yellow;
    public Material green;
    public Material off;
    public Transform[] lights;
    public List<PhyCar> cars = new();

    [System.Serializable]
    public class TrafficLightState
    {
        public string color;
        public int time;
    }

    public bool DoRepeat = true;

    public List<TrafficLightState> states = new();

    int milliseconds_left = 0;
    internal string current_color = "red";
    internal string previous_color = "red";
    
    void Start()
    {
        StartCoroutine(BeginTrafficLight());
    }

    void Update()
    {

    }

    int resolution_ms = 10;
    private IEnumerator WaitForMilliseconds(int ms)
    {
        milliseconds_left = ms;
        while (milliseconds_left > 0)
        {
            yield return new WaitForSeconds(resolution_ms / 1000f);
            milliseconds_left -= resolution_ms;
            Sync();
        }
    }

    public IEnumerator BeginTrafficLight()
    {
        if (states.Count == 0)
        {
            yield break;
        }
        while (true)
        {
            foreach (var state in states)
            {
                previous_color = current_color;
                if (state.color == "red")
                {
                    lights[0].GetComponent<Renderer>().material = red;
                    lights[1].GetComponent<Renderer>().material = off;
                    lights[2].GetComponent<Renderer>().material = off;
                }
                else if (state.color == "yellow")
                {
                    lights[0].GetComponent<Renderer>().material = off;
                    lights[1].GetComponent<Renderer>().material = yellow;
                    lights[2].GetComponent<Renderer>().material = off;
                }
                else if (state.color == "green")
                {
                    lights[0].GetComponent<Renderer>().material = off;
                    lights[1].GetComponent<Renderer>().material = off;
                    lights[2].GetComponent<Renderer>().material = green;
                }
                current_color = state.color;
                // yield return new WaitForSeconds(state.time);
                yield return StartCoroutine(WaitForMilliseconds(state.time * 1000));
            }
            if (!DoRepeat)
            {
                break;
            }
        }
    }

    internal void Sync()
    {
        // foreach (var car in cars)
        // {
        //     Sio.Emit("trafficlight", new
        //     {
        //         @event = "trafficlight",
        //         payload = new
        //         {
        //             previous = previous_color,
        //             color = current_color,
        //             time = milliseconds_left,
        //             abs_time = DateTime.Now.ToBinary()
        //         }
        //     });
        // }
    }
}
