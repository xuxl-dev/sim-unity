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
    public static PseudoClick Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void ClickAt(float x, float y)
    {
        // simulate a click on the screen
        Vector2 clickPosition = new(x, y);
        PointerEventData pointerEventData = new(EventSystem.current)
        {
            position = clickPosition,

        };
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(pointerEventData, results);
        if (results.Count > 0)
        {
            ExecuteEvents.Execute(results[0].gameObject, pointerEventData, ExecuteEvents.pointerClickHandler);
            Debug.Log("Clicked on " + results[0].gameObject.name);
        }
    }
    class Data
    {
        public float x;
        public float y;
    }
    void Start()
    {
        Sio.Instance.On("click", (data) =>
        {
            var dat = data.GetValue<Data>();
            ClickAt(dat.x, dat.y);
            Debug.Log("click at " + dat.x + " " + dat.y);
        });
    }

    void Update()
    {
    }
}
