using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HietakissaUtils.HKTransformTool
{
    class HKRotateOperation : HKTransformOperation
    {
        public HKRotateOperation(KeyCombination combination) : base(combination)
        {

        }


        Dictionary<Transform, Quaternion> startRotations = new Dictionary<Transform, Quaternion>();
        Dictionary<Transform, Vector3> startPositions = new Dictionary<Transform, Vector3>();
        Dictionary<Transform, Quaternion> endRotations = new Dictionary<Transform, Quaternion>();
        Dictionary<Transform, Vector3> endPositions = new Dictionary<Transform, Vector3>();

        public override void OnStart()
        {
            Debug.Log($"Rotate operation start");

            foreach (Transform t in tool.targets)
            {
                startRotations[t] = t.rotation;
                startPositions[t] = t.position;
            }
        }

        public override void OnUpdate()
        {
            foreach (Transform t in tool.targets)
            {
                if (startRotations.TryGetValue(t, out Quaternion rot))
                {
                    // Use average of input X and input Y multiplied by a constant rotation-specific sensitivity
                    float angle = tool.HasValueOverride ? tool.OverrideValue : (tool.MouseInput.x + tool.MouseInput.y) * 0.5f * 6f;

                    t.position = startPositions[t];
                    t.rotation = rot;
                    if (tool.WorkingMode == WorkingMode.Global)
                    {
                        t.RotateAround(tool.GetOriginForTransform(t), tool.WorkingAxis, angle);
                        //t.RotateAround(Vector3.zero, tool.WorkingAxis, angle);
                    }
                    else t.Rotate(tool.WorkingAxis, angle, Space.Self);
                    endRotations[t] = t.rotation;
                    endPositions[t] = t.position;
                }
            }
        }

        public override void OnApply()
        {
            foreach (Transform t in tool.targets)
            {
                if (startRotations.TryGetValue(t, out Quaternion rot))
                {
                    EditorUtility.SetDirty(t.gameObject);
                    t.rotation = rot;
                    t.position = startPositions[t];
                }
            }

            Undo.RecordObjects(tool.targets, "HK Rotate Operation Apply");

            foreach (Transform t in tool.targets)
            {
                if (endRotations.TryGetValue(t, out Quaternion rot))
                {
                    t.rotation = rot;
                    t.position = endPositions[t];
                }
            }

            startRotations.Clear();
            startPositions.Clear();
            endRotations.Clear();
            endPositions.Clear();
        }

        public override void OnCancel()
        {
            foreach (Transform t in tool.targets)
            {
                if (startRotations.TryGetValue(t, out Quaternion rot))
                {
                    t.rotation = rot;
                    t.position = startPositions[t];
                }
            }

            startRotations.Clear();
            startPositions.Clear();
            endRotations.Clear();
            endPositions.Clear();
        }
    }
}
