using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetBounds : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        Bounds totalBounds = renderer ? renderer.bounds : new Bounds();

        foreach (Renderer childRenderer in GetComponentsInChildren<Renderer>())
        {
            totalBounds.Encapsulate(childRenderer.bounds);
        }

        // 输出整体包围盒的中心点和尺寸
        Debug.Log("Total Bounds Center: " + totalBounds.center);
        Debug.Log("Total Bounds Size: " + totalBounds.size);

    }

    // Update is called once per frame
    void Update()
    {

    }
}
