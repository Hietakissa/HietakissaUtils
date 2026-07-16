using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HietakissaUtils.HKTransformTool
{
    class HKScaleOperation : HKTransformOperation
    {
        public HKScaleOperation(KeyCombination combination) : base(combination)
        {

        }


        Dictionary<Transform, Vector3> startScales = new Dictionary<Transform, Vector3>();
        Dictionary<Transform, Vector3> startPositions = new Dictionary<Transform, Vector3>();
        Dictionary<Transform, Vector3> endScales = new Dictionary<Transform, Vector3>();
        Dictionary<Transform, Vector3> endPositions = new Dictionary<Transform, Vector3>();

        Vector3 origin;

        public override void OnStart()
        {
            Debug.Log($"Scale operation start");

            foreach (Transform t in tool.targets)
            {
                startPositions[t] = t.position;
                startScales[t] = t.localScale;
            }

            origin = tool.CommonOrigin;
        }

        public override void OnUpdate()
        {
            Vector3 workingAxis = new Vector3(tool.WorkingAxis.x, tool.WorkingAxis.y, tool.WorkingAxis.z);

            float scaleMult;
            Vector3 scaleAxis;
            if (tool.HasValueOverride)
            {
                scaleMult = tool.OverrideValue;
                scaleAxis = Vector3Int.one - workingAxis + workingAxis * scaleMult;
            }
            else
            {
                scaleMult = tool.MouseInput.x + tool.MouseInput.y;
                scaleAxis = Vector3Int.one + workingAxis * scaleMult;
            }

            foreach (Transform t in tool.targets)
            {
                if (startScales.TryGetValue(t, out Vector3 scale))
                {
                    if (tool.WorkingMode == WorkingMode.Global)
                    {
                        if (tool.OriginMode == OriginMode.Median)
                        {

                        }
                        else
                        {
                            //Vector3 orientation = (t.right + t.up + t.forward).normalized;
                            //Vector3 projectedWorkingAxis = Vector3.Scale(orientation, workingAxis);
                            //Vector3 projectedScaleAxis = Vector3.one + projectedWorkingAxis * scaleMult;
                            //t.localScale = Vector3.Scale(scale, projectedScaleAxis);
                            //Debug.Log($"working axis: {workingAxis}, projected scale: {projectedScaleAxis}");

                            //Vector3 localAxis = t.right + t.up + t.forward;
                            //Vector3 localScaleAxis = Vector3.Scale(localAxis, scaleAxis);
                            //t.localScale = Vector3.Scale(scale, localScaleAxis);

                            Quaternion q = Quaternion.FromToRotation(Vector3.zero, t.rotation.eulerAngles);
                            //Vector3 localScaleAxis = t.InverseTransformDirection(scaleAxis);
                            Vector3 localScaleAxis = q * scaleAxis;
                            t.localScale = Vector3.Scale(scale, localScaleAxis);

                            Debug.Log($"SA:{scaleAxis} LSA:{localScaleAxis}, R:{t.right} U:{t.up} F:{t.forward}");
                        }
                    }
                    else
                    {
                        // local scale
                        t.localScale = Vector3.Scale(scale, scaleAxis);
                    }

                    endPositions[t] = t.position;
                    endScales[t] = t.localScale;
                }
            }
        }

        public override void OnApply()
        {
            foreach (Transform t in tool.targets)
            {
                if (startScales.TryGetValue(t, out Vector3 scale))
                {
                    EditorUtility.SetDirty(t.gameObject);
                    t.position = startPositions[t];
                    t.localScale = scale;
                }
            }

            Undo.RecordObjects(tool.targets, "HK Rotate Operation Apply");

            foreach (Transform t in tool.targets)
            {
                if (endScales.TryGetValue(t, out Vector3 scale))
                {
                    t.position = endPositions[t];
                    t.localScale = scale;
                }
            }

            startPositions.Clear();
            startScales.Clear();
            endPositions.Clear();
            endScales.Clear();
        }

        public override void OnCancel()
        {
            foreach (Transform t in tool.targets)
            {
                if (startScales.TryGetValue(t, out Vector3 scale))
                {
                    t.position = startPositions[t];
                    t.localScale = scale;
                }
            }

            startPositions.Clear();
            startScales.Clear();
            endPositions.Clear();
            endScales.Clear();
        }
    }
}
