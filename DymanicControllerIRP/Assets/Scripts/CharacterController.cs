using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DO
{
    public class CharacterController : MonoBehaviour
    {
        float horizontal;
        float vertical;
        Vector3 moveDirection;
        float moveAmount;
        Vector3 camYForward;

        Transform cameraHolder; 

        Rigidbody rigidbody;
        Collider collider;
        Animator animator;

        public float moveSpeed = 4;
        public float rotateSpeed = 9;
        public float jumpSpeed = 15;

        bool isGrounded;
        bool keepOffGround;
        float savedTime; 

        public void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
            rigidbody.angularDrag = 999;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

            cameraHolder = CameraManager.singleton.transform; 
            collider = GetComponent<Collider>();
            animator = GetComponentInChildren<Animator>(); 
        }

        private void FixedUpdate()
        {
            isGrounded = OnGround();
            Movement(); 
        }

        public void Movement()
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            camYForward = cameraHolder.forward;
            Vector3 v = vertical * cameraHolder.forward;
            Vector3 h = horizontal * cameraHolder.right;

            moveDirection = (v + h).normalized;
            moveAmount = Mathf.Clamp01((Mathf.Abs(horizontal) + Mathf.Abs(vertical)));

            Vector3 targetDir = moveDirection;
            targetDir.y = 0;
            if (targetDir == Vector3.zero)
                targetDir = transform.forward;

            Quaternion lookDir = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, lookDir, Time.deltaTime * rotateSpeed);
            transform.rotation = targetRotation;

            Vector3 dir = transform.forward * (moveSpeed * moveAmount);
            dir.y = rigidbody.velocity.y; 
            rigidbody.velocity = dir;
        }

        public void Update()
        {
            isGrounded = OnGround();
            
            if(keepOffGround)
            {
                if(Time.realtimeSinceStartup - savedTime > 0.5f)
                {
                    keepOffGround = false; 
                }
            }

            Jump(); 

            animator.SetFloat("move", moveAmount);
            animator.SetBool("onAir", !isGrounded);
        }

        void Jump()
        {
            if (isGrounded)
            {
                bool jump = Input.GetButton("Jump");
                if (jump)
                {
                    Vector3 v = rigidbody.velocity;
                    v.y = jumpSpeed;
                    rigidbody.velocity = v;
                    savedTime = Time.realtimeSinceStartup;
                    keepOffGround = true;
                }
            }
        }

        bool OnGround()
        {
            if (keepOffGround)
                return false; 

            Vector3 origin = transform.position;
            origin.y += 0.4f;
            Vector3 direction = -transform.up;
            RaycastHit hit; 
            if(Physics.Raycast(origin, direction, out hit, 0.41f))
            {
                return true; 
            }
            return false; 
        }
    }
}