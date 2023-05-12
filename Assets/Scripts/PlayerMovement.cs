using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.0f;

    [SerializeField] private float jumpSize = 6.0f;

    private Rigidbody theRigidbody;

    private bool canJump;

    void Start()
    {
       theRigidbody = GetComponent<Rigidbody>();
       canJump = false;
    }

    void FixedUpdate()
    {
        transform.Translate(-transform.forward * moveSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(-transform.right * moveSpeed * Time.deltaTime);
        } 
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(transform.right * moveSpeed * Time.deltaTime);
        }
    }

    void Update()
    {
        // TODO: Check if colliding with ground
        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            Debug.Log("Jump");
            GetComponent<Rigidbody>().AddForce(Vector3.up * jumpSize, ForceMode.Impulse);
            canJump = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Set") || collision.gameObject.CompareTag("Ground"))
        {
            canJump = true;
        }
    }
}
