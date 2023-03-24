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
