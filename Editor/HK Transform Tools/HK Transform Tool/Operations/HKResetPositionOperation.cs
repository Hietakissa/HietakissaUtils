using UnityEditor;
using UnityEngine;

namespace HietakissaUtils.HKTransformTool
{
    class HKResetPositionOperation : HKTransformOperation
    {
        public HKResetPositionOperation(KeyCombination combination) : base(combination)
        {

        }


        public override void OnStart()
        {
            tool.ApplyOperation();
        }

        public override void OnUpdate()
        {
            
        }

        public override void OnApply()
        {
            Undo.RecordObjects(tool.targets, "HK Reset Position Operation Apply");

            for (int i = tool.targets.Length - 1; i >= 0; i--)
            {
                Transform t = tool.targets[i];

                if (t.localPosition != Vector3.zero)
                {
                    EditorUtility.SetDirty(t.gameObject);
                    t.localPosition = Vector3.zero;
                }
                else if (t.position != Vector3.zero)
                {
                    EditorUtility.SetDirty(t.gameObject);
                    t.position = Vector3.zero;
                }
            }

            //Undo.RecordObjects(tool.targets, "HK Reset Position Operation Apply");
            //
            //foreach (Transform t in tool.targets)
            //{
            //    if (t.localPosition != Vector3.zero)
            //    {
            //        EditorUtility.SetDirty(t.gameObject);
            //        t.localPosition = Vector3.zero;
            //    }
            //    else if (t.position != Vector3.zero)
            //    {
            //        EditorUtility.SetDirty(t.gameObject);
            //        t.position = Vector3.zero;
            //    }
            //}
        }

        public override void OnCancel()
        {

        }
    }
}
