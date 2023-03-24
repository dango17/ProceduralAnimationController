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
        public float inAngleDistance = 1; 

        public float horizontal;
        public float vertical; 

        Transform helper;
        float delta; 

        private void Start()
        {
            Initalize(); 
        }

        public void Initalize()
        {
            //Helper will store rotations
            helper = new GameObject().transform;
            helper.name = "Climb Helper";

            CheckForClimb();
        }

        public void CheckForClimb()
        {
            Vector3 origin = transform.position;
            origin.y += 1.4f;
            Vector3 dir = transform.forward;
            RaycastHit hit; 
            if(Physics.Raycast(origin, dir, out hit, 5))
            {
                helper.position = PosWithOffset(origin, hit.point);
                InitalizeClimbing(hit); 
            }
        }

        void InitalizeClimbing(RaycastHit hit)
        {
            isClimbing = true;

            helper.transform.rotation = Quaternion.LookRotation(-hit.normal);
            startPos = transform.position;
            targetPos = hit.point + (hit.normal * offsetFromWall);
            t = 0;
            isinPositition = false;
            anim.CrossFade("Climb_Idle", 2); 
        }

        void Update()
        {
            delta = Time.deltaTime; 
            Tick(delta); 
        }

        //Pass delta from Tick one TPC is completed 
        public void Tick(float delta)
        {
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

                bool canMove = CanMove(moveDir);
                if (!canMove || moveDir == Vector3.zero)
                    return;

                t = 0;
                isLerping = true;
                startPos = transform.position;
                //Vector3 targetPosition = helper.position - transform.position;
                targetPos = helper.position; 
            }
            else
            {
                t += delta * climbSpeed;
                if(t > 1)
                {
                    t = 1;
                    isLerping = false; 
                }
                Vector3 climbPosition = Vector3.Lerp(startPos, targetPos, t);
                transform.position = climbPosition;
                transform.rotation = Quaternion.Slerp(transform.rotation, helper.rotation, delta * rotateSpeed);
            }
        }

        bool CanMove(Vector3 moveDir)
        {
            Vector3 origin = transform.position;
            float dis = positionOffset;
            Vector3 dir = moveDir;
            Debug.DrawRay(origin, dir * dis, Color.red);
            RaycastHit hit; 

            if(Physics.Raycast(origin, dir, out hit, dis))
            {
                return false; 
            }

            origin += moveDir * dis;
            dir = helper.forward;
            float dis2 = inAngleDistance;

            Debug.DrawRay(origin, dir * dis2); 
            if(Physics.Raycast(origin, dir, out hit, dis))
            {
                helper.position = PosWithOffset(origin, hit.point);
                helper.rotation = Quaternion.LookRotation(-hit.normal);
                return true; 
            }

            //Incase previous raycast fails 
            origin += dir * dis2;
            dir = -Vector3.up;

            Debug.DrawRay(origin, dir, Color.yellow); 
            if(Physics.Raycast(origin, dir, out hit, dis2))
            {
                float angle = Vector3.Angle(helper.up, hit.normal); 
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
            t += Time.deltaTime;

            if (t > 1)
            {
                t = 1;
                isinPositition = true; 

                //Enable IK here
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
    }
}
