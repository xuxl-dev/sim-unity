using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightBehavior : MonoBehaviour
{
    public Material red;
    public Material yellow;
    public Material green;
    public Material off;
    public Transform[] lights;
    public List<CarAgent> cars = new();

    [System.Serializable]
    public class TrafficLightState
    {
        public string color;
        public int time;
    }

    public bool DoRepeat = true;

    public List<TrafficLightState> states = new();

    int seconds_left = 0;
    string current_color = "red";
    string previous_color = "red";
    
    void Start()
    {
        StartCoroutine(BeginTrafficLight());
    }

    void Update()
    {

    }

    private IEnumerator WaitForSeconds(int seconds)
    {
        seconds_left = seconds;
        while (seconds_left > 0)
        {
            yield return new WaitForSeconds(1);
            seconds_left--;
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
                yield return StartCoroutine(WaitForSeconds(state.time));
            }
            if (!DoRepeat)
            {
                break;
            }
        }
    }
    // type TrafficLightEvent = {
    //   type: 'event';
    //   event: 'trafficlight';
    //   payload: {
    //     previous: 'red' | 'yellow' | 'green';
    //     color: 'red' | 'yellow' | 'green';
    //     time: number;
    //   }
    // }
    internal void Sync()
    {
        foreach (var car in cars)
        {
            Sio.Instance.Emit("trafficlight", new
            {
                id = car.name,
                type = "event",
                @event = "trafficlight",
                payload = new
                {
                    previous = previous_color,
                    color = current_color,
                    time = seconds_left
                }
            });
        }
    }
}
