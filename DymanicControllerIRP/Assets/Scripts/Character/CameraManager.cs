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
            if (target == null)
                return;

            desiredRotation += Input.GetAxis("Mouse X") * rotationSpeed;
            desiredAngle -= Input.GetAxis("Mouse Y") * rotationSpeed;
            desiredAngle = Mathf.Clamp(desiredAngle, minAngle, maxAngle);

            currentRotation = Mathf.LerpAngle(currentRotation, desiredRotation, Time.deltaTime * rotationSpeed);
            currentAngle = Mathf.Lerp(currentAngle, desiredAngle, Time.deltaTime * rotationSpeed);

            Quaternion rotation = Quaternion.Euler(currentAngle, currentRotation, 0);
            Vector3 position = target.position - (rotation * Vector3.forward * distance);

            position.y = Mathf.Lerp(transform.position.y, target.position.y + height, Time.deltaTime * heightDamping);

            transform.rotation = rotation;
            transform.position = position;
        }
    }
}