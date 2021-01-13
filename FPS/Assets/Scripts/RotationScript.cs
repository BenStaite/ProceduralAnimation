using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationScript : MonoBehaviour
{

    public float xIn;
    public float yIn;
    public float acceleration = 0.5f;
    public float decceleration = 0.9f;
    public float X;
    public float Y;



    // Start is called before the first frame update
    void Start()
    {
        X = 0;
        Y = 0;
    }

    // Update is called once per frame
    void Update()
    {

        xIn = Input.GetAxis("Mouse X");
        yIn = Input.GetAxis("Mouse Y");

        if (xIn == 0)
        {
            X = X * decceleration;
        }
        else
        {
            X += xIn * acceleration;
        }


        if (yIn == 0)
        {
            Y = Y * decceleration;
        }
        else
        {
            Y += yIn * acceleration;
        }

        transform.Rotate(new Vector3(0, X));



    }
}
