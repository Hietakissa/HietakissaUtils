using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HietakissaUtils.HKTransformTool
{
    class HKMoveOperation : HKTransformOperation
    {
        public HKMoveOperation(KeyCombination combination) : base(combination)
        {

        }

        public override bool ShouldUpdateCommonOrigin => true;


        Dictionary<Transform, Vector3> startPositions = new Dictionary<Transform, Vector3>();
        Dictionary<Transform, Vector3> endPositions = new Dictionary<Transform, Vector3>();

        public override void OnStart()
        {
            Debug.Log($"Move operation start");

            foreach (Transform t in tool.targets)
            {
                startPositions[t] = t.position;
            }
        }

        public override void OnUpdate()
        {
            Transform cameraTransform = tool.Cam.transform;
            Vector3 inputProjectedOnWorkingAxis;

            if (tool.HasValueOverride) inputProjectedOnWorkingAxis = new Vector3(tool.WorkingAxis.x, tool.WorkingAxis.y, tool.WorkingAxis.z) * tool.OverrideValue;
            else
            {
                inputProjectedOnWorkingAxis = cameraTransform.right * tool.EasedMouseInput.x + cameraTransform.up * tool.EasedMouseInput.y + cameraTransform.forward * tool.EasedMouseInput.y;
                inputProjectedOnWorkingAxis = Vector3.Scale(tool.WorkingAxis, inputProjectedOnWorkingAxis);
            }
            

            foreach (Transform t in tool.targets)
            {
                if (startPositions.TryGetValue(t, out Vector3 pos))
                {
                    Vector3 localTranslation = inputProjectedOnWorkingAxis;
                    if (tool.WorkingMode == WorkingMode.Local) localTranslation = t.TransformDirection(inputProjectedOnWorkingAxis);

                    Vector3 finalPosition = pos + localTranslation;
                    endPositions[t] = finalPosition;
                    t.position = finalPosition;
                }
            }
        }

        public override void OnApply()
        {
            foreach (Transform t in tool.targets)
            {
                if (startPositions.TryGetValue(t, out Vector3 pos))
                {
                    EditorUtility.SetDirty(t.gameObject);
                    t.position = pos;
                }
            }

            Undo.RecordObjects(tool.targets, "HK Move Operation Apply");

            foreach (Transform t in tool.targets)
            {
                if (endPositions.TryGetValue(t, out Vector3 pos))
                {
                    t.position = pos;
                }
            }


            startPositions.Clear();
            endPositions.Clear();
            Debug.Log($"Move operation apply");
        }

        public override void OnCancel()
        {
            foreach (Transform t in tool.targets)
            {
                if (startPositions.TryGetValue(t, out Vector3 pos)) t.position = pos;
            }

            startPositions.Clear();
            endPositions.Clear();
            Debug.Log($"Move operation cancel");
        }
    }
}
