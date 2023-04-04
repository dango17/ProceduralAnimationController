using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DO
{
    public class ClimbingAnimation : MonoBehaviour
    {
        Animator animator;

        IKSnapshot ikBase;
        IKSnapshot current = new IKSnapshot();
        IKSnapshot next = new IKSnapshot();

        public float w_rightHand;
        public float w_lefthHand;
        public float w_leftFoot;
        public float w_rightFoot;

        Vector3 rightHand, leftHand, rightFoot, leftFoot;
        Transform h; 

        public void Initalization(Climbing climbing, Transform helper)
        {
            animator = climbing.anim;
            ikBase = climbing.baseIKsnapshot;
            h = helper;
        }

        public void CreatePositions(Vector3 origin)
        {
            IKSnapshot ik = CreateSnapshot(origin);
            CopySnapshot(ref current, ik);

            UpdateIKPosition(AvatarIKGoal.LeftFoot, current.leftFoot);
            UpdateIKPosition(AvatarIKGoal.RightFoot, current.rightFoot);
            UpdateIKPosition(AvatarIKGoal.LeftHand, current.leftHand);
            UpdateIKPosition(AvatarIKGoal.RightHand, current.rightHand);

            UpdateIKWeight(AvatarIKGoal.LeftFoot, 1);
            UpdateIKWeight(AvatarIKGoal.RightFoot, 1);
            UpdateIKWeight(AvatarIKGoal.LeftHand, 1);
            UpdateIKWeight(AvatarIKGoal.RightHand, 1); 
        }

        public IKSnapshot CreateSnapshot(Vector3 o)
        {
            IKSnapshot r = new IKSnapshot();
            Vector3 _lh = LocalToWorldSpace(ikBase.leftHand);
            r.leftHand = GetActualPos(_lh);

            Vector3 _rh = LocalToWorldSpace(ikBase.rightHand);
            r.rightHand = GetActualPos(_rh);

            Vector3 _lf = LocalToWorldSpace(ikBase.leftFoot);
            r.leftFoot = GetActualPos(_lf);

            Vector3 _rf = LocalToWorldSpace(ikBase.rightFoot);
            r.rightFoot = GetActualPos(_rf);
            return r; 
        }

        public float wallOffset = 0f;

        Vector3 GetActualPos(Vector3 o)
        {
            Vector3 r = o;
            Vector3 origin = o;
            Vector3 dir = h.forward;
            origin += -(dir * 0.2f);
            RaycastHit hit; 
            if(Physics.Raycast(origin, dir, out hit, 1.5f))
            {
                Vector3 _r = hit.point + (hit.normal * wallOffset);
                r = _r; 
            }
            return r; 
        }

        Vector3 LocalToWorldSpace(Vector3 position)
        {
            Vector3 r = h.position;
            r += h.right * position.x;
            r += h.forward * position.z;
            r += h.up * position.y;
            return r; 
        }

        public void CopySnapshot(ref IKSnapshot to, IKSnapshot from)
        {
            to.rightHand = from.rightHand;
            to.leftHand = from.leftHand;
            to.leftFoot = from.leftFoot;
            to.rightFoot = from.rightFoot; 
        }

        public void UpdateIKPosition(AvatarIKGoal goal, Vector3 pos)
        {
          switch (goal)
          {
                case AvatarIKGoal.LeftFoot:
                    leftFoot = pos; 
                    break;
                case AvatarIKGoal.RightFoot:
                    rightFoot = pos; 
                    break;
                case AvatarIKGoal.LeftHand:
                    leftHand = pos; 
                    break;
                case AvatarIKGoal.RightHand:
                    rightHand = pos; 
                    break;
                default:
                    break; 
            }
        }

        public void UpdateIKWeight(AvatarIKGoal goal, float weight)
        {
            switch(goal)
            {
                case AvatarIKGoal.LeftFoot:
                    w_leftFoot = weight; 
                    break;
                case AvatarIKGoal.RightFoot:
                    w_rightFoot = weight; 
                    break;
                case AvatarIKGoal.LeftHand:
                    w_lefthHand = weight; 
                    break;
                case AvatarIKGoal.RightHand:
                    w_rightHand = weight; 
                    break;
                default:
                    break; 
            }
        }

        private void OnAnimatorIK()
        {
            SetIKPosition(AvatarIKGoal.LeftHand, leftHand, w_rightHand);
            SetIKPosition(AvatarIKGoal.RightHand, rightHand, w_rightHand);
            SetIKPosition(AvatarIKGoal.LeftFoot, leftFoot, w_leftFoot);
            SetIKPosition(AvatarIKGoal.RightFoot, rightFoot, w_rightFoot); 
        }

        void SetIKPosition(AvatarIKGoal goal, Vector3 targetPosition, float weight)
        {
            animator.SetIKPositionWeight(goal, weight);
            animator.SetIKPosition(goal, targetPosition); 
        }
    }
}
