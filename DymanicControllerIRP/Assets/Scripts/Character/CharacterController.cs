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
        float climbTimer; 

        public bool isClimbing;
        public bool climbOff;

        public GameObject playerRagdollRig;
        public CapsuleCollider playersMaincollider; 

        Climbing climbing;

        public void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
            rigidbody.angularDrag = 999;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

            cameraHolder = CameraManager.singleton.transform; 
            collider = GetComponent<Collider>();
            animator = GetComponentInChildren<Animator>();
            climbing = GetComponent<Climbing>();

            GetRagdollComponents();

            RagdollOff(); 
        }

        private void FixedUpdate()
        {
            //Return if player is climbing
            if (isClimbing)
                return; 

            //Check if player is grounded
            isGrounded = OnGround();
            Movement();
        }

        void LateUpdate()
        {
           
        }

        public void Movement()
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            //Calculate forward vector of the camera
            camYForward = cameraHolder.forward;
            //Calculate vertical and horizonral movement
            Vector3 v = vertical * cameraHolder.forward;
            Vector3 h = horizontal * cameraHolder.right;

            //Calculate the move direction & the amount of movement 
            moveDirection = (v + h).normalized;
            moveAmount = Mathf.Clamp01((Mathf.Abs(horizontal) + Mathf.Abs(vertical)));

            Vector3 targetDir = moveDirection;
            targetDir.y = 0;
            if (targetDir == Vector3.zero)
                targetDir = transform.forward;

            //Calculate the direction of the look direction
            Quaternion lookDir = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, lookDir, Time.deltaTime * rotateSpeed);
            transform.rotation = targetRotation;

            //Direction of movement 
            Vector3 dir = transform.forward * (moveSpeed * moveAmount);
            dir.y = rigidbody.velocity.y; 
            rigidbody.velocity = dir;
        }

        public void Update()
        {
            if (isClimbing)
            {
                //If player is currently climbing, skip this method 
                climbing.Tick(Time.deltaTime);
                return; 
            }

            isGrounded = OnGround();
            
            if(keepOffGround)
            {
                //If the player has recently jumped and should still be kept off the ground,
                //wait for a short period of time before allowing the character to land again
                if (Time.realtimeSinceStartup - savedTime > 0.5f)
                {
                    keepOffGround = false; 
                }
            }

            Jump(); 

            if(!isGrounded && !keepOffGround)
            {
                //If the player is not currently on the ground and has finished jumping,
                //check if there is a climbable surface nearby and start climbing if there is
                if (!climbOff)
                {
                    isClimbing = climbing.CheckForClimb();
                    if (isClimbing)
                    {
                        DisableController();
                    }
                }
            }

            if(climbOff)
            {
                //If the player has recently finished climbing, wait for a short period of time
                //before allowing the character to climb again
                if (Time.realtimeSinceStartup - climbTimer > 1)
                {
                    climbOff = false; 
                }
            }
            animator.SetFloat("move", moveAmount);
            animator.SetBool("onAir", !isGrounded);
        }

        void Jump()
        {
            if (isGrounded)
            {
                //If the character is on the ground, check if the jump button is pressed and
                //apply a vertical force to the character's Rigidbody if it is
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

            //Check if the character is currently on the ground by casting a ray downwards from
            //the character's position and checking if it hits anything within a certain distance
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

        public void DisableController()
        {
            //Disable the character's Rigidbody and Collider to prevent it from moving or colliding
            //with objects in the scene while it is climbing
            rigidbody.isKinematic = true;
            collider.enabled = false; 
        }

        public void EnableController()
        {
            //Re-enable the character's Rigidbody and Collider and switch the animation state to
            //"onAir" to indicate that the character is no longer climbing
            rigidbody.isKinematic = false;
            collider.enabled = true;
            animator.CrossFade("onAir", 0.2f);
            climbOff = true;
            climbTimer = Time.realtimeSinceStartup;
            isClimbing = false; 
        }

        Collider[] ragdollColliders;
        Rigidbody[] limbsRigidbodies;
        public void GetRagdollComponents()
        {
            ragdollColliders = playerRagdollRig.GetComponentsInChildren<Collider>();
            limbsRigidbodies = playerRagdollRig.GetComponentsInChildren<Rigidbody>(); 
        }

        public void RagdollOn()
        {
            animator.enabled = false;
            playersMaincollider.enabled = false; 

            foreach (Collider col in ragdollColliders)
            {
                col.enabled = true;
            }

            foreach (Rigidbody rigidbody in limbsRigidbodies)
            {
                rigidbody.isKinematic = false;
            }

            GetComponent<Rigidbody>().isKinematic = true;         
        }

        public void RagdollOff()
        {
            foreach(Collider col in ragdollColliders)
            {
                col.enabled = false; 
            }

            foreach(Rigidbody rigidbody in limbsRigidbodies)
            {
                rigidbody.isKinematic = true; 
            }
            GetComponent<Rigidbody>().isKinematic = false; 
        }
    }
}