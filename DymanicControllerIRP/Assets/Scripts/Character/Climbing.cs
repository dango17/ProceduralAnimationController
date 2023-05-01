using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DO
{
    public class Climbing : MonoBehaviour
    {
        public Animator anim; 
        public bool isClimbing;

        bool isinPositition;
        bool isLerping; 

        float t;
        Vector3 startPos;
        Vector3 targetPos; 
        Quaternion startRot;
        Quaternion targetRot;

        public float positionOffset;

        public float offsetFromWall = 0.3f;
        public float speed_multiplier = 0.2f;
        public float climbSpeed = 3;
        public float rotateSpeed = 5;

        public float rayTowardsMoveDir = 0.5f;
        public float rayForwardTowardsWall = 1; 

        public float horizontal;
        public float vertical; 

        public IKSnapshot baseIKsnapshot;

        public ClimbingAnimation climbingAnimator;
        CharacterController characterController; 
        public LayerMask ignoreLayers = ~(1 << 8);

        public bool isMidAnim; 

        Transform helper;
        float delta; 

        private void Start()
        {
            characterController = GetComponent<CharacterController>(); 
            Initalize(); 
        }

        public void Initalize()
        {
            //Create and store the climbing helper
            helper = new GameObject().transform;
            helper.name = "Climb Helper";

            //Initialize the climbing animator script
            climbingAnimator.Initalization(this, helper);
            ignoreLayers = ~(1 << 8); 
        }

        public bool CheckForClimb()
        { 
            Vector3 origin = transform.position;
            origin.y += 0.02f;
            Vector3 dir = transform.forward;
            RaycastHit hit; 
            if(Physics.Raycast(origin, dir, out hit, 0.5f, ignoreLayers))
            {
                //Set the helpers position to the climbing position and initialize climbing
                helper.position = PosWithOffset(origin, hit.point);
                InitalizeClimbing(hit);

                return true; 
            }
            return false; 
        }

        void InitalizeClimbing(RaycastHit hit)
        {
            isClimbing = true;

            //Enable the ClimbingAnimation script
            climbingAnimator.enabled = true; 

            //Set the helper rortation to face away from surface of a wall
            helper.transform.rotation = Quaternion.LookRotation(-hit.normal);

            //Set the start and target positions for the climbing animations
            startPos = transform.position;
            targetPos = hit.point + (hit.normal * offsetFromWall);

            //Reset animation time
            t = 0;
            isinPositition = false;
            //Crossdfade with the climb idle animation 
            anim.CrossFade("Climb_Idle", 2); 
        }

        public void Tick(float delta_time)
        {
            this.delta = delta_time; 
            if(!isinPositition)
            {
                //Get in position for climbing 
                GetInPosition(); 
                return; 
            }

            if(!isLerping)
            {
                //Check for climbing cancellation 
                bool cancel = Input.GetKeyUp(KeyCode.X);
                if(cancel)
                {
                    CancelClimb();
                    return; 
                }

                //Get the horizontal and vertical input 
                horizontal = Input.GetAxis("Horizontal");
                vertical = Input.GetAxis("Vertical");

                //Calculate the total move amount 
                float moveAmount = Mathf.Abs(horizontal) + Mathf.Abs(vertical);

                //Calculate the move direction vectors
                Vector3 h = helper.right * horizontal;
                Vector3 v = helper.up * vertical;
                Vector3 moveDir = (h + v).normalized;

                if(isMidAnim)
                {
                    //return if we're in the middle of an animation 
                    if (moveDir == Vector3.zero)
                        return; 
                }
                else
                {
                    bool canMove = CanMove(moveDir);
                    if (!canMove || moveDir == Vector3.zero)
                        return;
                }

                //Toggle midAnim
                isMidAnim = !isMidAnim; 

                t = 0;

                isLerping = true;

                //Set the start and target positions 
                startPos = transform.position;
                Vector3 tp = helper.position - transform.position;
                float d = Vector3.Distance(helper.position, startPos) / 2;
                tp *= positionOffset;
                tp += transform.position;
                targetPos = (isMidAnim) ? tp : helper.position; 

                //Create positions for the climbing animations
                climbingAnimator.CreatePositions(targetPos, moveDir, isMidAnim); 

            }
            else
            {
                //Lerp towards the target position 
                t += delta_time * climbSpeed;
                if(t > 1)
                {
                    t = 1;
                    isLerping = false; 
                }
                Vector3 climbPosition = Vector3.Lerp(startPos, targetPos, t);
                transform.position = climbPosition;

                //Rotate towards the helpers rotation 
                transform.rotation = Quaternion.Slerp(transform.rotation, helper.rotation, delta_time * rotateSpeed);
                LookForGround();
            }
        }

        bool CanMove(Vector3 moveDir)
        {
            //Raycast variables d
            Vector3 origin = transform.position;
            float dis = rayTowardsMoveDir;
            Vector3 dir = moveDir;

            //Draw debug lines
            DebugLine.singleton.SetLine(origin, origin + (dir * dis), 0);
            DebugLine.singleton.SetLineColor(Color.blue, 0);

            RaycastHit hit; 
            if(Physics.Raycast(origin, dir, out hit, dis))
            {
                //Check for corner, return false if found 
                return false; 
            }

            //Move origin and change direction for 
            origin += moveDir * dis;
            dir = helper.forward;
            float dis2 = rayForwardTowardsWall;

            //Raycast towards the wall 
            DebugLine.singleton.SetLine(origin, origin + (dir * dis2), 1);
            DebugLine.singleton.SetLineColor(Color.blue, 0);
            if (Physics.Raycast(origin, dir, out hit, dis2))
            {
                //Update helper object position and rotation if a wall if found 
                helper.position = PosWithOffset(origin, hit.point);
                helper.rotation = Quaternion.LookRotation(-hit.normal);
                return true; 
            }

            //Move origin and change direction for the next raycast
            origin = origin + (dir * dis2);
            dir = -moveDir;
            DebugLine.singleton.SetLine(origin, origin + dir, 1);
            if (Physics.Raycast(origin, dir, out hit, rayForwardTowardsWall))
            {
                //Update helper object position and rotation if a wall if found 
                helper.position = PosWithOffset(origin, hit.point);
                helper.rotation = Quaternion.LookRotation(-hit.normal);
                return true;
            }
            //Incase previous raycast fails, raycast downward  
            origin += dir * dis2;
            dir = -Vector3.up;

            //Draw downward debug line
            DebugLine.singleton.SetLine(origin, origin + dir, 2);
            DebugLine.singleton.SetLineColor(Color.blue, 0);
            if (Physics.Raycast(origin, dir, out hit, dis2))
            {
                //Check the angle between the helpers objects forward vector and the hit normal 
                float angle = Vector3.Angle(-helper.forward, hit.normal); 
                if(angle < 40)
                {
                    //Update the helpers object position and rotation if the angle is small enough 
                    helper.position = PosWithOffset(origin, hit.point);
                    helper.rotation = Quaternion.LookRotation(-hit.normal);
                    return true;
                }
            }
            //If all raycasts fail, return false
            return false; 
        }

        void GetInPosition()
        {
            t += delta * 3; 

            if (t > 1)
            {
                //Set variables when we are in position 
                t = 1;
                isinPositition = true;
                horizontal = 0;
                vertical = 0; 

                //Create IK positions for the animation 
                climbingAnimator.CreatePositions(targetPos, Vector3.zero, false); 
            }

            //Move towards the target position
            Vector3 targetPosition = Vector3.Lerp(startPos, targetPos, t);
            transform.position = targetPosition;

            //Rotate the helpers rotation 
            transform.rotation = Quaternion.Slerp(transform.rotation, helper.rotation, delta * rotateSpeed); 
        }

        Vector3 PosWithOffset(Vector3 origin, Vector3 target)
        {
            //Get the normalized direction vector from the origin to the target position 
            Vector3 direction = origin - target;
            direction.Normalize();

            //Get the offset vector from the direction vector and offsetFromWall
            Vector3 offset = direction * offsetFromWall;

            //Return the target position with the offset applied
            return target + offset; 
        }

        public void LookForGround()
        {
            Vector3 origin = transform.position;
            Vector3 direction = -transform.up;

            //Check if the player is no longer on the ground
            RaycastHit hit; 
            if(Physics.Raycast(origin, direction, out hit, rayTowardsMoveDir + 0.05f, ignoreLayers))
            {
                CancelClimb(); 
            }
        }

        public void CancelClimb()
        {
            //disable climbing animator 
            isClimbing = false;
            characterController.EnableController();
            climbingAnimator.enabled = false;
        }
    }

    [System.Serializable]
    public class IKSnapshot
    {
        //Store the positions for the right andf left hands and feet
        public Vector3 rightHand, leftHand, leftFoot, rightFoot; 
    }
}
