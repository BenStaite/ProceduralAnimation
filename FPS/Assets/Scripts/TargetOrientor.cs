using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetOrientor : MonoBehaviour
{
    public Animator anim;
    public Vector3 Direction;
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float X = anim.GetFloat("X");
        float Y = anim.GetFloat("Y");
        Direction = (player.transform.right * X + player.transform.forward * Y);
        Direction.y = 0;
        transform.rotation = Quaternion.LookRotation(Direction,Vector3.up);
        Debug.DrawRay(transform.position, Direction, Color.yellow);
        Debug.DrawRay(transform.position, player.transform.forward, Color.cyan);
    }
}
