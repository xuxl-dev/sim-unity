using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// this class simulates human input by clicking on the screen
/// </summary>
public class PseudoClick : MonoBehaviour
{
    private static PseudoClick instance;

    public static PseudoClick Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("PseudoClick").AddComponent<PseudoClick>();
            }
            return instance;
        }
    }
    public void ClickAt(float x, float y)
    {
        // simulate a click on the screen
        Vector2 clickPosition = new(x, y);
        PointerEventData pointerData = new(EventSystem.current)
        {
            position = clickPosition
        };
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(pointerData, results);
        // filter out non car
        // results.RemoveAll(r => !r.gameObject.CompareTag("car"));

        if (results.Count > 0)
        {
            Debug.Log("clicking on " + results[0].gameObject.name);
            // send a click event to object
            //FIXME does not work
            ExecuteEvents.Execute(results[0].gameObject, pointerData, ExecuteEvents.pointerClickHandler);

        }
    }
    class Data
    {
        public float x;
        public float y;
    }
    void Start()
    {
        if (Sio.IsAvaliable)
        {
            Sio.Instance.On("click", (data) =>
            {
                var dat = data.GetValue<Data>();
                ClickAt(dat.x, dat.y);
            });
        }
    }

    void Update()
    {
    }
}
