using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStateController : MonoBehaviour
{
    public RotationScript rotationScript;
    public Animator animator;
    public int isJoggingHash;
    public int isWalkingHash;

    public bool isWalking;
    public bool isJogging;

    // Start is called before the first frame update
    public float acceleration = 0.02f;
    public float decceleration = 0.02f;
    public float thresh = 0.1f;
    public float inputThresh = 0.8f;

    public float X;
    public float Y;

    public float yIn;
    public float xIn;
    public bool shift;

    void Start()
    {
        isJoggingHash = Animator.StringToHash("Jogging");
        isWalkingHash = Animator.StringToHash("Walking");
        X = 0;
        Y = 0;
        shift = false;
    }

    // Update is called once per frame
    void Update()
    {
        isJogging = animator.GetBool(isJoggingHash);
        isWalking = animator.GetBool(isWalkingHash);
        yIn = Input.GetAxis("Vertical");
        xIn = Input.GetAxis("Horizontal");
        xIn += rotationScript.xIn;
        shift = Input.GetKey(KeyCode.LeftShift);

        if (xIn == 0)
        {
            X = Decelerate(X);
        }
        else
        {
            X = Accelerate(X, xIn);
            X = Mathf.Clamp(X, -1f, 1f);
        }

        if (yIn == 0)
        {
            Y = Decelerate(Y);
        }
        else
        {
            Y = Accelerate(Y, yIn);
            Y = Mathf.Clamp(Y, -1f, 1f);
        }


        if (xIn == 0 && yIn == 0)
        {
            if (isWalking)
            {
                animator.SetBool(isWalkingHash, false);
            }
            else if (isJogging)
            {
                animator.SetBool(isJoggingHash, false);
            }
        }
        else if (shift)
        {
            animator.SetBool(isJoggingHash, true);
            animator.SetBool(isWalkingHash, false);
        }
        else
        {
            animator.SetBool(isWalkingHash, true);
            animator.SetBool(isJoggingHash, false);
        }

        if(Mathf.Abs(X) < thresh && xIn < inputThresh ) { X = 0; }
        if(Mathf.Abs(Y) < thresh && yIn < inputThresh ) { Y = 0; }

        animator.SetFloat("X", X);
        animator.SetFloat("Y", Y);
    }

    float Decelerate(float i)
    {
        if(Mathf.Abs(i) < thresh)
        {
            return i;
        }
        else if (i > 0)
        {
            return (i -= decceleration);
        }
        else
        {
            return(i += decceleration);
        }
    }


    float Accelerate(float i, float j)
    {
        if (Mathf.Abs(j) < inputThresh)
        {
            return Decelerate(i);
        }
        else if (j > 0)
        {
            return (i += acceleration);
        }
        else
        {
            return (i -= acceleration);
        }
    }
}
