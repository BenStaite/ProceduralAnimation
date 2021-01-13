using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierTest : MonoBehaviour
{

    public GameObject b1, b2, b3;

    public GameObject ball;

    public int i;
    // Start is called before the first frame update
    void Start()
    {
        i = 0;
    }

    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return
            oneMinusT * oneMinusT * p0 +
            2f * oneMinusT * t * p1 +
            t * t * p2;
    }

    // Update is called once per frame
    void Update()
    {
        float moveDuration = 100000f;
        float timeElapsed = 0;
        Vector3 startPoint = b1.transform.position;
        Vector3 endPoint = b3.transform.position;
        Vector3 centerPoint = b2.transform.position ;
        do
        {

            timeElapsed += Time.deltaTime;
            float normalizedTime = timeElapsed / moveDuration;

            // Quadratic bezier curve
            ball.transform.position =
              Vector3.Lerp(
                Vector3.Lerp(startPoint, centerPoint, normalizedTime),
                Vector3.Lerp(centerPoint, endPoint, normalizedTime),
                normalizedTime
             );
        }
        while (timeElapsed < moveDuration);
    }
}
