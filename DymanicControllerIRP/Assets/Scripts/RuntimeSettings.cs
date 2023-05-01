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
        [Header("Speeds/Stats/Sliders")]
        public TextMeshProUGUI lerpSpeedText;
        public Slider lerpSpeedSlider; 
        public TextMeshProUGUI climbSpeedText;
        public Slider climbSpeedSlider; 
        public TextMeshProUGUI WalloffsetText;
        public Slider walloffSetSlider; 
        public TextMeshProUGUI verticalDirectionText;
        public TextMeshProUGUI horizontalDirectionText;
        public TextMeshProUGUI deltaTimeText;
        [Header("Camera Settings")]
        public TextMeshProUGUI cameraHeightText;
        public Slider cameraHeightSlider;
        public TextMeshProUGUI cameraSpeedText;
        public Slider cameraSpeedSlider;
        public TextMeshProUGUI cameraDistanceText;
        public Slider cameraDistanceSlider; 

        [HideInInspector] private float deltaTime;
        [HideInInspector] ClimbingAnimation climbingAnimation;
        [HideInInspector] Climbing climbing;
        [HideInInspector] CameraManager cameraManager; 

        public void Start()
        {
            climbingAnimation = FindObjectOfType<ClimbingAnimation>();
            climbing = FindObjectOfType<Climbing>();
            cameraManager = FindObjectOfType<CameraManager>(); 
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

            //Climbing Speed & Slider
            climbing.climbSpeed = climbSpeedSlider.value;
            climbSpeedText.text = $"Climb Speed: {climbing.climbSpeed:0.##}";

            //Lerp Speed & Slider
            climbingAnimation.lerpSpeed = lerpSpeedSlider.value;
            lerpSpeedText.text = $"Lerp Speed: {climbingAnimation.lerpSpeed:0.##}";

            //Wall Offset & Slider
            climbingAnimation.wallOffset = walloffSetSlider.value;
            WalloffsetText.text = $"Wall Offset: {climbingAnimation.wallOffset:0.##}";

            //Vertical & Horizontal direction
            verticalDirectionText.text = climbing.vertical.ToString();
            horizontalDirectionText.text = climbing.horizontal.ToString();

            //Delta Time 
            float delta = climbingAnimation.delta;
            deltaTimeText.text = string.Format("Delta Time: {0:0.0000}", delta);

            //Camera Height & Slider
            cameraManager.height = cameraHeightSlider.value;
            cameraHeightText.text = $"Camera Height: {cameraManager.height:0.##}";

            //Camera Speed & Slider 
            cameraManager.rotationSpeed = cameraSpeedSlider.value;
            cameraSpeedText.text = $"Camera Speed: {cameraManager.rotationSpeed:0.##}";

            //Camera Distance & Slider
            cameraManager.distance = cameraDistanceSlider.value;
            cameraDistanceText.text = $"Camera Distance: {cameraManager.distance:0.##}"; 

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

