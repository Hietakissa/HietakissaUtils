using UnityEditor;
using UnityEngine;

namespace HietakissaUtils.HKTransformTool
{
    class HKResetRotationOperation : HKTransformOperation
    {
        public HKResetRotationOperation(KeyCombination combination) : base(combination)
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

                if (t.localRotation != Quaternion.identity)
                {
                    EditorUtility.SetDirty(t.gameObject);
                    t.localRotation = Quaternion.identity;
                }
                else if (t.rotation != Quaternion.identity)
                {
                    EditorUtility.SetDirty(t.gameObject);
                    t.rotation = Quaternion.identity;
                }
            }

            //foreach (Transform t in tool.targets)
            //{
            //    if (t.localRotation != Quaternion.identity)
            //    {
            //        EditorUtility.SetDirty(t.gameObject);
            //        t.localRotation = Quaternion.identity;
            //    }
            //    else if (t.rotation != Quaternion.identity)
            //    {
            //        EditorUtility.SetDirty(t.gameObject);
            //        t.rotation = Quaternion.identity;
            //    }
            //}
        }

        public override void OnCancel()
        {

        }
    }
}
