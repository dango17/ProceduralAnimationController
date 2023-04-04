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
        IKGoals goals = new IKGoals(); 

        public float w_rightHand;
        public float w_leftHand;
        public float w_leftFoot;
        public float w_rightFoot;

        Vector3 rightHand, leftHand, rightFoot, leftFoot;
        Transform h;

        bool isMirror;
        bool isLeft;
        Vector3 previousMoveDirection;

        float delta;
        public float lerpSpeed = 1; 

        public void Initalization(Climbing climbing, Transform helper)
        {
            animator = climbing.anim;
            ikBase = climbing.baseIKsnapshot;
            h = helper;
        }

        public void CreatePositions(Vector3 origin, Vector3 moveDir, bool isMidAnim)
        {
            delta = Time.deltaTime; 
            HandleAnimation(moveDir, isMidAnim); 

            if(!isMidAnim)
            {
                UpdateGoals(moveDir); 
            }
            else
            {
                UpdateGoals(previousMoveDirection); 
            }

            IKSnapshot ik = CreateSnapshot(origin);
            CopySnapshot(ref current, ik);

            SetIKPosition(isMidAnim, goals.lf, current.leftFoot, AvatarIKGoal.LeftFoot);
            SetIKPosition(isMidAnim, goals.rf, current.rightFoot, AvatarIKGoal.RightFoot);
            SetIKPosition(isMidAnim, goals.lh, current.leftHand, AvatarIKGoal.LeftHand);
            SetIKPosition(isMidAnim, goals.rh, current.rightHand, AvatarIKGoal.RightHand);

            UpdateIKWeight(AvatarIKGoal.LeftFoot, 1);
            UpdateIKWeight(AvatarIKGoal.RightFoot, 1);
            UpdateIKWeight(AvatarIKGoal.LeftHand, 1);
            UpdateIKWeight(AvatarIKGoal.RightHand, 1); 
        }

        void UpdateGoals(Vector3 moveDir)
        {
            isLeft = (moveDir.x <= 0);

            if(moveDir.x != 0)
            {
                goals.lh = isLeft; 
                goals.rh = !isLeft;
                goals.lf = isLeft;
                goals.rf = !isLeft;
            }
            else
            {
                bool isEnabled = isMirror; 
                if(moveDir.y < 0)
                {
                    isEnabled = !isEnabled;
                }

                //Wont mirror on the left?
                goals.lh = isEnabled;
                goals.rh = !isEnabled;
                goals.lf = isEnabled;
                goals.rf = !isEnabled;
            }
        }

        public void HandleAnimation(Vector3 moveDir, bool isMidAnim)
        {
            if(isMidAnim)
            {
                if(moveDir.y != 0)
                {
                    if(moveDir.y < 0)
                    {

                    }
                    else
                    {

                    }
                    isMirror = !isMirror;
                    animator.SetBool("mirror", isMirror);

                    animator.CrossFade("Climb_Up", 0.2f);
                }
            }
            else
            {
                animator.CrossFade("Climb_Idle", 0.2f); 
            }
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

        public float wallOffset = 0.1f;

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

        void SetIKPosition(bool isMidAnimation, bool isTrue, Vector3 pos, AvatarIKGoal goal)
        {
            if(isMidAnimation)
            {
                if(isTrue)
                {
                    Vector3 p = GetActualPos(pos);
                    UpdateIKPosition(goal, p); 
                }
            }
            else
            {
                if(!isTrue)
                {
                    Vector3 p = GetActualPos(pos);
                    UpdateIKPosition(goal, p); 
                }
            }
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
                    w_leftHand = weight; 
                    break;
                case AvatarIKGoal.RightHand:
                    w_rightHand = weight; 
                    break;
                default:
                    break; 
            }
        }

        public void OnAnimatorIK()
        {
            delta = Time.deltaTime; 

            SetIKPosition(AvatarIKGoal.LeftHand, leftHand, w_leftHand);
            SetIKPosition(AvatarIKGoal.RightHand, rightHand, w_rightHand);
            SetIKPosition(AvatarIKGoal.LeftFoot, leftFoot, w_leftFoot);
            SetIKPosition(AvatarIKGoal.RightFoot, rightFoot, w_rightFoot); 
        }

        public void SetIKPosition(AvatarIKGoal goal, Vector3 targetPosition, float weight)
        {
            IKStates ikState = GetIKStates(goal); 
            if(ikState == null)
            {
                ikState = new IKStates();
                ikState.goal = goal;
                ikStates.Add(ikState); 
            }

            if(weight == 0)
            {
                ikState.isSet = false; 
            }

            if(!ikState.isSet)
            {
                ikState.postition = GoalToBodyBones(goal).position;
                ikState.isSet = true; 
            }
            ikState.positionWeight = weight;
            ikState.postition = Vector3.Lerp(ikState.postition, targetPosition, delta * lerpSpeed); 

            animator.SetIKPositionWeight(goal, ikState.positionWeight);
            animator.SetIKPosition(goal, ikState.postition); 
        }

        Transform GoalToBodyBones(AvatarIKGoal goal)
        {
            switch(goal)
            {
                case AvatarIKGoal.LeftFoot:
                    return animator.GetBoneTransform(HumanBodyBones.LeftFoot); 

                case AvatarIKGoal.RightFoot:
                    return animator.GetBoneTransform(HumanBodyBones.RightFoot);
   
                case AvatarIKGoal.LeftHand:
                    return animator.GetBoneTransform(HumanBodyBones.LeftHand);
      
                default: 
                case AvatarIKGoal.RightHand:
                    return animator.GetBoneTransform(HumanBodyBones.RightHand);
            }
        }

        IKStates GetIKStates(AvatarIKGoal goal)
        {
            IKStates r = null; 
            foreach (IKStates i in ikStates)
            {
                if(i.goal == goal)
                {
                    r = i;
                    break;
                }
            }
            return r; 
        }
        List<IKStates> ikStates = new List<IKStates>(); 

        class IKStates
        {
            public AvatarIKGoal goal;
            public Vector3 postition;
            public float positionWeight;
            public bool isSet = false; 
        }
    }

    public class IKGoals
    {
        public bool rh;
        public bool lh;
        public bool lf;
        public bool rf; 
    }
}
