using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DO
{
    public class DebugLine : MonoBehaviour
    {
        public int maxRenderers;
        List<LineRenderer> lines = new List<LineRenderer>();

        //Create a new line renderer with given index
        void CreateLine(int i)
        {
            GameObject go = new GameObject();
            lines.Add(go.AddComponent<LineRenderer>());
            lines[i].widthMultiplier = 0.05f;
            if (i == 1) // set default color to red for second line
            {
                lines[i].startColor = Color.red;
                lines[i].endColor = Color.red;
            }
            else 
            {
                lines[i].startColor = Color.blue;
                lines[i].endColor = Color.blue;
            }
        }

        //Sets start and end points of the line renderer at given index
        public void SetLine(Vector3 startpos, Vector3 endpos, int index)
        {
            if (index > lines.Count - 1)
                CreateLine(index);

            lines[index].SetPosition(0, startpos);
            lines[index].SetPosition(1, endpos);

            if (index == 1)
            {
                SetLineColor(Color.red, 1);
            }

        }

        //Sets the color of the line renderer at given index
        public void SetLineColor(Color color, int index)
        {
            if (index > lines.Count - 1)
                return;

            lines[index].startColor = color;
            lines[index].endColor = color;
            lines[index].material.color = color;
        }

        public static DebugLine singleton;
        private void Awake()
        {
            singleton = this; 
        }
    }
}