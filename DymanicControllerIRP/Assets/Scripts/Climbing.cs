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
            //Helper will store rotations
            helper = new GameObject().transform;
            helper.name = "Climb Helper";

            climbingAnimator.Initalization(this, helper);
            ignoreLayers = ~(1 << 8); 
            //CheckForClimb();
        }

        public bool CheckForClimb()
        {
            Vector3 origin = transform.position;
            origin.y += 0.02f;
            Vector3 dir = transform.forward;
            RaycastHit hit; 
            if(Physics.Raycast(origin, dir, out hit, 0.5f, ignoreLayers))
            {
                helper.position = PosWithOffset(origin, hit.point);
                InitalizeClimbing(hit);

                return true; 
            }
            return false; 
        }

        void InitalizeClimbing(RaycastHit hit)
        {
            isClimbing = true;
            climbingAnimator.enabled = true; 

            helper.transform.rotation = Quaternion.LookRotation(-hit.normal);
            startPos = transform.position;
            targetPos = hit.point + (hit.normal * offsetFromWall);
            t = 0;
            isinPositition = false;
            anim.CrossFade("Climb_Idle", 2); 
        }

        //Pass delta from Tick one TPC is completed 
        public void Tick(float delta_time)
        {
            this.delta = delta_time; 
            if(!isinPositition)
            {
                GetInPosition(); 
                return; 
            }

            if(!isLerping)
            {
                horizontal = Input.GetAxis("Horizontal");
                vertical = Input.GetAxis("Vertical");
                float moveAmount = Mathf.Abs(horizontal) + Mathf.Abs(vertical);

                Vector3 h = helper.right * horizontal;
                Vector3 v = helper.up * vertical;
                Vector3 moveDir = (h + v).normalized;

                if(isMidAnim)
                {
                    if (moveDir == Vector3.zero)
                        return; 
                }
                else
                {
                    bool canMove = CanMove(moveDir);
                    if (!canMove || moveDir == Vector3.zero)
                        return;
                }

                isMidAnim = !isMidAnim; 

                t = 0;
                isLerping = true;
                startPos = transform.position;
                Vector3 tp = helper.position - transform.position;
                float d = Vector3.Distance(helper.position, startPos) / 2;
                tp *= positionOffset;
                tp += transform.position;
                targetPos = (isMidAnim) ? tp : helper.position; 

                climbingAnimator.CreatePositions(targetPos, moveDir, isMidAnim); 
            }
            else
            {
                t += delta_time * climbSpeed;
                if(t > 1)
                {
                    t = 1;
                    isLerping = false; 
                }
                Vector3 climbPosition = Vector3.Lerp(startPos, targetPos, t);
                transform.position = climbPosition;
                transform.rotation = Quaternion.Slerp(transform.rotation, helper.rotation, delta_time * rotateSpeed);
                LookForGround(); 
            }
        }

        bool CanMove(Vector3 moveDir)
        {
            Vector3 origin = transform.position;
            float dis = rayTowardsMoveDir;
            Vector3 dir = moveDir;

            DebugLine.singleton.SetLine(origin, origin + (dir * dis), 0); 

            RaycastHit hit; 
            if(Physics.Raycast(origin, dir, out hit, dis))
            {
                //Check for corner
                return false; 
            }

            origin += moveDir * dis;
            dir = helper.forward;
            float dis2 = rayForwardTowardsWall;

            //Raycast towards the wall 
            DebugLine.singleton.SetLine(origin, origin + (dir * dis2), 1);
            if(Physics.Raycast(origin, dir, out hit, dis2))
            {
                helper.position = PosWithOffset(origin, hit.point);
                helper.rotation = Quaternion.LookRotation(-hit.normal);
                return true; 
            }

            origin = origin + (dir * dis2);
            dir = -moveDir;
            DebugLine.singleton.SetLine(origin, origin + dir, 1);
            if (Physics.Raycast(origin, dir, out hit, rayForwardTowardsWall))
            {
                helper.position = PosWithOffset(origin, hit.point);
                helper.rotation = Quaternion.LookRotation(-hit.normal);
                return true;
            }

            //return false; 

            //Incase previous raycast fails 
            origin += dir * dis2;
            dir = -Vector3.up;

            DebugLine.singleton.SetLine(origin, origin + dir, 2);
            if(Physics.Raycast(origin, dir, out hit, dis2))
            {
                float angle = Vector3.Angle(-helper.forward, hit.normal); 
                if(angle < 40)
                {
                    helper.position = PosWithOffset(origin, hit.point);
                    helper.rotation = Quaternion.LookRotation(-hit.normal);
                    return true;
                }
            }
            return false; 
        }

        void GetInPosition()
        {
            t += delta * 3; 

            if (t > 1)
            {
                t = 1;
                isinPositition = true;
                horizontal = 0;
                vertical = 0; 
                climbingAnimator.CreatePositions(targetPos, Vector3.zero, false); 
            }

            Vector3 targetPosition = Vector3.Lerp(startPos, targetPos, t);
            transform.position = targetPosition;
            transform.rotation = Quaternion.Slerp(transform.rotation, helper.rotation, delta * rotateSpeed); 
        }

        Vector3 PosWithOffset(Vector3 origin, Vector3 target)
        {
            Vector3 direction = origin - target;
            direction.Normalize();
            Vector3 offset = direction * offsetFromWall;

            return target + offset; 
        }

        public void LookForGround()
        {
            Vector3 origin = transform.position;
            Vector3 direction = -transform.up;
            RaycastHit hit; 
            if(Physics.Raycast(origin, direction, out hit, rayTowardsMoveDir + 0.05f, ignoreLayers))
            {
                isClimbing = false;
                characterController.EnableController();
                climbingAnimator.enabled = false; 
            }
        }
    }

    [System.Serializable]
    public class IKSnapshot
    {
        public Vector3 rightHand, leftHand, leftFoot, rightFoot; 
    }
}
