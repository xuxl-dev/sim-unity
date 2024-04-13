using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MockWarning : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateWarning(string type) {
        //force-set-warning

        PhyEnvReporter.Instance.Push("force-set-warning", new
        {
            id = "Warning-0000",
            type, // can be  'front-p' | 'front' | 'side' | 'none'
        });
    }

    public void CreateSideWarning(string type) {
        PhyEnvReporter.Instance.Push("force-set-side-warning", new
        {
            id = "Warning-0000",
            type, // can be  'left' | 'right' | 'none'
        });
    }
}
