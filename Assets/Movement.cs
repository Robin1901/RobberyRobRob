using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public Rigidbody rb;

    public float moveForeward = 2000f;
    public float moveBackwards = 2000f;
    public float moveLeft = 2000f;
    public float moveRight = 2000f;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKey("w"))
        {
            rb.AddForce(0, 0, moveForeward * Time.deltaTime);
        }
        if (Input.GetKey("s"))
        {
            rb.AddForce(0, 0, -moveBackwards * Time.deltaTime);
        }
        if (Input.GetKey("a"))
        {
            rb.AddForce(-moveLeft * Time.deltaTime, 0, 0);
        }
        if (Input.GetKey("d"))
        {
            rb.AddForce(moveRight * Time.deltaTime, 0, 0);
        }



    }
}
    
