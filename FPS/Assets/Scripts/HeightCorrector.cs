using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightCorrector : MonoBehaviour
{

    [Range(0f, 10f)]
    public float gravity;

    [Range(0f, 2f)]
    public float tolerance;

    public LayerMask layerMask;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position + Vector3.up * 2f, Vector3.down);
        if (Physics.Raycast(ray, out hit,10f, layerMask))
        {
            if(hit.transform.tag == "Walkable")
            {
                if (transform.position.y > hit.point.y + tolerance)
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y - (gravity * Time.deltaTime), transform.position.z);
                }
            }
        }
    }
}
