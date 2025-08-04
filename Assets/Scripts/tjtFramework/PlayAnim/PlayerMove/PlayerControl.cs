using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private CharacterController controller;

    [SerializeField]
    private float moveSpeed = 5;
    [SerializeField]
    private float turnSpeed = 10;
    [SerializeField]
    private Transform cameraTF;

    private Animator animator;
    private float moveState;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        moveState = 0;
    }

    void Update()
    {
        float verticalInput = Input.GetAxisRaw("Vertical");
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        var moveVector = new Vector3(horizontalInput, 0, verticalInput);

        moveState = 2;
        if(Input.GetKey(KeyCode.LeftShift))
        {
            moveState = 3;
        }
        else if(Input.GetKey(KeyCode.LeftControl))
        {
            moveState = 1;
        }

        if(moveVector.sqrMagnitude > 0)
        {
            var moveDir = cameraTF.forward * verticalInput + cameraTF.right * horizontalInput;
            moveDir = new Vector3(moveDir.x, 0, moveDir.z);
            moveDir.Normalize();

            var targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Lerp(targetRotation, targetRotation, turnSpeed * Time.deltaTime);
            //controller.Move(moveDir * moveSpeed * Time.deltaTime);

            animator.SetFloat("MoveState", moveState);
        }
        else
        {
            animator.SetFloat("MoveState", 0);
        }
    }

    //private void OnAnimatorMove()
    //{
    //    controller.Move(animator.deltaPosition *  Time.deltaTime);
    //    transform.rotation *= animator.deltaRotation;
    //}
}
