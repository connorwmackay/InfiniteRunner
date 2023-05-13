using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.0f;

    [SerializeField] private float jumpSize = 6.0f;

    private Rigidbody theRigidbody;

    private bool canJump;

    private Animator _animator;

    void Start()
    {
       theRigidbody = GetComponent<Rigidbody>();
       _animator = GetComponent<Animator>();
       canJump = false;
    }

    void FixedUpdate()
    {
        transform.Translate(transform.forward * moveSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(transform.right * moveSpeed * Time.deltaTime);
        } 
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(-transform.right * moveSpeed * Time.deltaTime);
        }
    }

    void SetIsJumping(bool value)
    {
        if (_animator != null)
        {
            foreach (var parameter in _animator.parameters)
            {
                if (parameter.name == "isJumping" && parameter.type == AnimatorControllerParameterType.Bool)
                {
                   _animator.SetBool("isJumping", value);
                   Debug.Log("Is Jumping: " + value);
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            Debug.Log("Jump");
            GetComponent<Rigidbody>().AddForce(Vector3.up * jumpSize, ForceMode.Impulse);
            SetIsJumping(true);
            canJump = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Set") || collision.gameObject.CompareTag("Ground"))
        {
            SetIsJumping(false);
            canJump = true;
        }
    }
}
