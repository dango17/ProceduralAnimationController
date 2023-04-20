using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace DO
{
    public class RuntimeSettings : MonoBehaviour
    {
        [Header("Framerate")]
        public TextMeshProUGUI framerateText;
        [Header("IK-Goals")]
        public TextMeshProUGUI IKGoalValuesText;
        [Header("Toggles/Bools")]
        public Toggle isMirrored;
        public Toggle isMidAnimation;
        public Toggle isClimbingToggle;
        [Header("Hand & Feet Coords")]
        public TextMeshProUGUI rightHandText;
        public TextMeshProUGUI leftHandText;
        public TextMeshProUGUI rightFootText;
        public TextMeshProUGUI leftFootText;
        [Header("Speeds/Stats")]
        public TextMeshProUGUI lerpSpeedText;
        public TextMeshProUGUI climbSpeedText;
        public TextMeshProUGUI WalloffsetText;
        public TextMeshProUGUI verticalDirectionText;
        public TextMeshProUGUI horizontalDirectionText; 
        [HideInInspector] private float deltaTime;
        [HideInInspector] ClimbingAnimation climbingAnimation;
        [HideInInspector] Climbing climbing;

        public void Start()
        {
            climbingAnimation = FindObjectOfType<ClimbingAnimation>();
            climbing = FindObjectOfType<Climbing>(); 
        }

        public void Update()
        {
            //Print the Framerate of the application 
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            framerateText.text = "FPS: " + Mathf.Round(fps);

            //IK Weights
            float rightHandValue = climbingAnimation.w_rightHand;
            float leftHandValue = climbingAnimation.w_leftHand;
            float leftFootValue = climbingAnimation.w_leftFoot;
            float rightFootValue = climbingAnimation.w_rightFoot;

            IKGoalValuesText.text =
             "Right Hand: " + rightHandValue.ToString() + "\n" +
             "Left Hand: " + leftHandValue.ToString() + "\n" +
             "Left Foot: " + leftFootValue.ToString() + "\n" +
             "Right Foot: " + rightFootValue.ToString();

            //Current Hands and Feet Positions
            rightHandText.SetText(climbingAnimation.rightHand.ToString());
            leftHandText.SetText(climbingAnimation.leftHand.ToString());
            rightFootText.SetText(climbingAnimation.rightFoot.ToString());
            leftFootText.SetText(climbingAnimation.leftFoot.ToString());

            climbSpeedText.text = "Climb Speed: " + climbing.climbSpeed;
            lerpSpeedText.text = "Lerp Speed: " + climbingAnimation.lerpSpeed;
            WalloffsetText.text = "Wall Offset: " + climbingAnimation.wallOffset;

            verticalDirectionText.text = climbing.vertical.ToString();
            horizontalDirectionText.text = climbing.horizontal.ToString(); 

            #region Toggles/Bools
            //Toggle isMidAnimBool 
            if (climbing.isMidAnim == true)
            {
                isMidAnimation.isOn = true; 
            }
            else
            {
                isMidAnimation.isOn = false;
            }
            //Toggle isMirrorBool
            if(climbingAnimation.isMirror == true)
            {
                isMirrored.isOn = true; 
            }
            else
            {
                isMirrored.isOn = false;
            }
            if(climbing.isClimbing == true)
            {
                isClimbingToggle.isOn = true; 
            }
            else
            {
                isClimbingToggle.isOn = false; 
            }
            #endregion
        }
    }
}

