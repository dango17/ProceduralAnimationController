using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DO
{
    public class ClimbingAnimation : MonoBehaviour
    {
        Animator animator;

        //The character's initial IK pose and current/next IK poses
        IKSnapshot ikBase;
        IKSnapshot current = new IKSnapshot();
        IKSnapshot next = new IKSnapshot();

        //The goals for each IK limb
        IKGoals goals = new IKGoals();

        //The weights of each IK limb
        public float w_rightHand;
        public float w_leftHand;
        public float w_leftFoot;
        public float w_rightFoot;

        //The positions of each IK limb
        public Vector3 rightHand, leftHand, rightFoot, leftFoot;
        //transform component used for positioning the limbs
        Transform h;

        //Whether the character is mirrored or not
        public bool isMirror;
        //is the player moving left? 
        bool isLeft;

        //The previous movement direction of the character
        Vector3 previousMoveDirection;
        float delta;

        //The speed at which the character moves between IK poses
        public float lerpSpeed = 1;

        //Initialize the character's IK goals and animator component
        public void Initalization(Climbing climbing, Transform helper)
        {
            goals.lh = true;
            goals.rh = false;
            goals.lf = false;
            goals.rf = true;

            animator = climbing.anim;
            ikBase = climbing.baseIKsnapshot;
            h = helper;
        }

        //Create the character's next IK pose based on the specified origin and movement direction
        public void CreatePositions(Vector3 origin, Vector3 moveDir, bool isMidAnim)
        {
            delta = Time.deltaTime;
            HandleAnimation(moveDir, isMidAnim);

            //If the player is not currently mid-anim, update its IK goals based on its movement direction
            if (!isMidAnim)
            {
                UpdateGoals(moveDir);
            }
            else
            {
                UpdateGoals(previousMoveDirection);
            }

            //Create a new IK snapshot based on the specified origin
            IKSnapshot ik = CreateSnapshot(origin);
            CopySnapshot(ref current, ik);

            //Update the positions of each IK limb
            SetIKPosition(isMidAnim, goals.lf, current.leftFoot, AvatarIKGoal.LeftFoot);
            SetIKPosition(isMidAnim, goals.rf, current.rightFoot, AvatarIKGoal.RightFoot);
            SetIKPosition(isMidAnim, goals.lh, current.leftHand, AvatarIKGoal.LeftHand);
            SetIKPosition(isMidAnim, goals.rh, current.rightHand, AvatarIKGoal.RightHand);

            //Update the weights of each IK limb
            UpdateIKWeight(AvatarIKGoal.LeftFoot, 1);
            UpdateIKWeight(AvatarIKGoal.RightFoot, 1);
            UpdateIKWeight(AvatarIKGoal.LeftHand, 1);
            UpdateIKWeight(AvatarIKGoal.RightHand, 1);
        }

        //Update the player's IK goals based on its movement direction
        void UpdateGoals(Vector3 moveDir)
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            //dot product to determine if the movement is more forward or right
            float dotForward = Vector3.Dot(moveDir, forward);
            float dotRight = Vector3.Dot(moveDir, right);

            isLeft = (moveDir.x <= 0);

            //set the goals based on previous dot product
            if (Mathf.Abs(dotForward) > Mathf.Abs(dotRight))
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

                //set the goals based on whether the movement is up or down
                goals.lh = isEnabled;
                goals.rh = !isEnabled;
                goals.lf = isEnabled;
                goals.rf = !isEnabled;
            }
        }

        //handles the animation based on the movement direction and if the animation is in the middle of playing
        public void HandleAnimation(Vector3 moveDir, bool isMidAnim)
        {
            if(isMidAnim)
            {
                //if the animation is in the middle of playing and the movement is up, copy the current snapshot and switch the mirror flag
                if (moveDir.y != 0)
                {
                    CopySnapshot(ref current, ikBase); 

                    if(moveDir.y < 0)
                    {
                        //Do nothing 
                    }
                    else
                    {
                        //Do nothing
                    }
                    isMirror = !isMirror;
                    animator.SetBool("mirror", isMirror);

                    //if the animation is not in the middle of playing, crossfade to the idle animation
                    animator.CrossFade("Climb_Up", 0.2f);
                }
            }
            else
            {
                animator.CrossFade("Climb_Idle", 0.2f); 
            }
        }

        //create a snapshot of the current IK system
        public IKSnapshot CreateSnapshot(Vector3 o)
        {
            //Create a new IKSnapshot object
            IKSnapshot r = new IKSnapshot();

            //Convert local position of the left hand to world space
            Vector3 _lh = LocalToWorldSpace(ikBase.leftHand);
            //Get the actual position of the left hand using raycasting and assign it to the snapshot object, same for other limbs
            r.leftHand = GetActualPos(_lh); 

            Vector3 _rh = LocalToWorldSpace(ikBase.rightHand);
            r.rightHand = GetActualPos(_rh);

            Vector3 _lf = LocalToWorldSpace(ikBase.leftFoot);
            r.leftFoot = GetActualPos(_lf);

            Vector3 _rf = LocalToWorldSpace(ikBase.rightFoot);
            r.rightFoot = GetActualPos(_rf);

            //return result
            return r; 
        }
        //Offset from the wall used for raycast
        public float wallOffset = 0.1f;

        Vector3 GetActualPos(Vector3 o)
        {
            //Set the result to the original position
            Vector3 r = o;
            //Set the origin of the raycast to the original position
            Vector3 origin = o;
            //Get the forward direction of the player avatar
            Vector3 dir = h.forward; 
            RaycastHit hit;
            //Perform a raycast in the forward direction to check for obstacles within a certain range
            if (Physics.Raycast(origin, dir, out hit, 1.5f))
            {
                //Calculate the actual position by adding the offset to the point of intersection with the obstacle
                Vector3 _r = hit.point + (hit.normal * wallOffset);
                //Set the result to the actual position
                r = _r; 
            }
            //Return the result
            return r; 
        }

        Vector3 LocalToWorldSpace(Vector3 position)
        {
            //Set the result of the position to the players avatar
            Vector3 r = h.position;
            //Move the result along the right direction of the avatar by the x component of the input position
            r += h.right * position.x;
            //Same again also for the z and y components of input position
            r += h.forward * position.z;
            r += h.up * position.y;
            //Return the result
            return r; 
        }

        public void CopySnapshot(ref IKSnapshot to, IKSnapshot from)
        {
            //Copy the hand and feet positions from the source snapshot to the target snapshot
            to.rightHand = from.rightHand;
            to.leftHand = from.leftHand;
            to.leftFoot = from.leftFoot;
            to.rightFoot = from.rightFoot; 
        }

        //Method sets the position of an IK goal on the avatar
        void SetIKPosition(bool isMidAnimation, bool isTrue, Vector3 pos, AvatarIKGoal goal)
        {
            if(isMidAnimation)
            {
                if(isTrue)
                {
                    //Get the actual position to set the IK goal to based on the target position
                    Vector3 p = GetActualPos(pos);
                    //Update the position of the specified IK goal with the new position
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

        //Updates the position of an IK goal on the avatar
        public void UpdateIKPosition(AvatarIKGoal goal, Vector3 pos)
        {
          //Update the position of the specified IK goal with the new position
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

        //Updates the weight of an IK goal on the avatar
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

        //Method is called by the animator every frame to update the IK goals on the avatar
        public void OnAnimatorIK()
        {
            delta = Time.deltaTime;

            //Update the position and weight of each IK goal on the avata
            SetIKPosition(AvatarIKGoal.LeftHand, leftHand, w_leftHand);
            SetIKPosition(AvatarIKGoal.RightHand, rightHand, w_rightHand);
            SetIKPosition(AvatarIKGoal.LeftFoot, leftFoot, w_leftFoot);
            SetIKPosition(AvatarIKGoal.RightFoot, rightFoot, w_rightFoot); 
        }

        //Method sets the position and weight of an IK goal on the avatar
        public void SetIKPosition(AvatarIKGoal goal, Vector3 targetPosition, float weight)
        {
            //Get the IKStates object for the specified goal
            IKStates ikState = GetIKStates(goal);

            //If there's no IKStates object for this goal, create one
            if (ikState == null)
            {
                ikState = new IKStates();
                ikState.goal = goal;
                ikStates.Add(ikState); 
            }

            //If weight is zero, mark this IKState as not set
            if (weight == 0)
            {
                ikState.isSet = false; 
            }

            //if this IKState hasn't been set yet, set its position to the corresponding body bone
            if (!ikState.isSet)
            {
                ikState.postition = GoalToBodyBones(goal).position;
                ikState.isSet = true; 
            }

            //Update the IKState's position and position weight
            ikState.positionWeight = weight;
            ikState.postition = Vector3.Lerp(ikState.postition, targetPosition, delta * lerpSpeed);

            //Set the animator's IK position weight and position for the specified goal
            animator.SetIKPositionWeight(goal, ikState.positionWeight);
            animator.SetIKPosition(goal, ikState.postition); 
        }

        //Returns the corresponding body bone for the specified IK goal
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

        //Get the IKStates object for the specified goal
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

        //Represents the state of an IK goal
        class IKStates
        {
            public AvatarIKGoal goal;
            public Vector3 postition;
            public float positionWeight;
            public bool isSet = false; 
        }

        //Represents the set of IK goals
        public class IKGoals
        {
            public bool rh;
            public bool lh;
            public bool lf;
            public bool rf;
        }
    }
}