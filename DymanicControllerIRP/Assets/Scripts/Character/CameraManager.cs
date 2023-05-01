using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DO
{
    public class CameraManager : MonoBehaviour
    {
        public Transform target;
        public float distance = 10;
        public float height = 3.0f;
        public float rotationSpeed = 3.0f;
        public float heightDamping = 2.0f;
        public float minAngle = -15.0f;
        public float maxAngle = 15.0f;

        private float currentRotation = 0.0f;
        private float desiredRotation = 0.0f;
        private float currentAngle = 0.0f;
        private float desiredAngle = 0.0f;

        public static CameraManager singleton;

        private void Awake()
        {
            singleton = this;
        }

        private void FixedUpdate()
        {
            //If no target is set, return
            if (target == null)
                return;

            //Get input for camera rotation and pitch
            desiredRotation += Input.GetAxis("Mouse X") * rotationSpeed;
            desiredAngle -= Input.GetAxis("Mouse Y") * rotationSpeed;

            //Clamp the desired pitch angle to the min and max angle values
            desiredAngle = Mathf.Clamp(desiredAngle, minAngle, maxAngle);

            //Interpolate current and desired camera rotation and pitch
            currentRotation = Mathf.LerpAngle(currentRotation, desiredRotation, Time.deltaTime * rotationSpeed);
            currentAngle = Mathf.Lerp(currentAngle, desiredAngle, Time.deltaTime * rotationSpeed);

            //Calculate camera rotation and position from current rotation, pitch and target position
            Quaternion rotation = Quaternion.Euler(currentAngle, currentRotation, 0);
            Vector3 position = target.position - (rotation * Vector3.forward * distance);

            //Interpolate camera height from current position to target position with damping
            position.y = Mathf.Lerp(transform.position.y, target.position.y + height, Time.deltaTime * heightDamping);

            //Set camera rotation and position
            transform.rotation = rotation;
            transform.position = position;
        }
    }
}