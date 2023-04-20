using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DO
{
    public class CameraManager : MonoBehaviour
    {
        public Transform target;
        public float speed = 9;
        public float distance = 10;


        public static CameraManager singleton;

        private void Awake()
        {
            singleton = this; 
        }

        private void FixedUpdate()
        {
            if (target == null)
                return;

            Vector3 direction = (target.position - transform.position).normalized;
            Vector3 targetPosition = target.position - direction * distance;
            targetPosition.y = transform.position.y;

            Vector3 p = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);
            transform.position = p;
        }
    }

}