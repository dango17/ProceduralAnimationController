using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DO
{
    public class Piston : MonoBehaviour
    {
        public float pushDistance = 1.0f; // Distance the piston will push out
        public float pushTime = 1.0f; // Time it takes for the piston to fully extend and retract
        public bool startPushed = true; // Whether the piston should start in the pushed out position
        public float pushSpeed = 1.0f; // Speed at which the piston moves

        private Vector3 startPosition;
        private Vector3 endPosition;
        private float pushTimer = 0.0f;

        void Start()
        {
            startPosition = transform.position;
            endPosition = startPosition + (transform.up * pushDistance);

            if (startPushed)
            {
                transform.position = endPosition;
            }
        }

        void Update()
        {
            // Increment the push timer based on the push speed
            pushTimer += Time.deltaTime * pushSpeed;

            // Calculate the new position of the piston based on the sine function
            float pushAmount = Mathf.Sin(pushTimer / pushTime * Mathf.PI);
            Vector3 newPosition = Vector3.Lerp(startPosition, endPosition, 0.5f + pushAmount * 0.5f);

            // Update the position of the piston
            transform.position = newPosition;
        }

        // Toggle the isPushingOut variable when the piston collides with something
        void OnCollisionEnter(Collision collision)
        {
            //isPushingOut = !isPushingOut;
        }

        // Draw the range of motion of the piston in the editor
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + transform.up * pushDistance);
            Gizmos.DrawLine(transform.position - transform.up * pushDistance, transform.position);
        }
    }
}