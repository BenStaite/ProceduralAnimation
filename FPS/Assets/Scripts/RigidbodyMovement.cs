using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyMovement : MonoBehaviour
{

    public AlienAi ai;
    public Rigidbody rb;
    public float moveForce;
    public Vector3 move;
    public float speed;
    public float rotationSpeed;
    public float minMove;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        if(Vector3.Distance(transform.position, ai.moveTarget) > minMove)
        {
            Vector3 move = ai.moveTarget - transform.position;
            move = move.normalized;
            move.y = 0;
            rb.MovePosition(rb.position + move * Time.deltaTime * speed);
        }

        Vector3 rotDir = ai.rotTarget - transform.position;
        Vector3 localTarget = transform.InverseTransformPoint(ai.rotTarget);
        var angle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

        Vector3 eulerAngleVelocity  = new Vector3(0, angle*rotationSpeed, 0);
        Quaternion deltaRotation  = Quaternion.Euler(eulerAngleVelocity * Time.deltaTime);
        rb.MoveRotation(rb.rotation * deltaRotation);
    }
}
