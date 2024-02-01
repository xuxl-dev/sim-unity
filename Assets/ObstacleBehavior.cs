using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleBehavior : MonoBehaviour
{
    [System.Serializable]
    public class KeyFrame
    {
        public Vector3 position;
        // public Quaternion rotation;
        public float time;
    }
    public List<KeyFrame> keyFrames = new List<KeyFrame>();
    void Start()
    {
        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        for (int i = 0; i < keyFrames.Count - 1; i++)
        {
            var beg = keyFrames[i];
            var end = keyFrames[i + 1];
            var t = beg.time;
            yield return Animate(beg, end, t);
        }
    }

    IEnumerator Animate(KeyFrame beg, KeyFrame end, float t)
    {
        var beg_pos = beg.position;
        var end_pos = end.position;
        float time_used = 0.0f;
        while (time_used < t)
        {
            time_used += Time.deltaTime;
            var pos = Vector3.Lerp(beg_pos, end_pos, time_used / t);
            transform.position = pos;
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
