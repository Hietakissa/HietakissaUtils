using UnityEditor;
using UnityEngine;

namespace HietakissaUtils.HKTransformTool
{
    class HKResetScaleOperation : HKTransformOperation
    {
        public HKResetScaleOperation(KeyCombination combination) : base(combination)
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
            foreach (Transform t in tool.targets)
            {
                Undo.RecordObjects(tool.targets, "HK Reset Position Operation Apply");

                if (t.localScale != Vector3.one)
                {
                    EditorUtility.SetDirty(t.gameObject);
                    t.localScale = Vector3.one;
                }
                else if (t.lossyScale != Vector3.one)
                {
                    EditorUtility.SetDirty(t.gameObject);
                    t.localScale = new Vector3(1f / t.lossyScale.x, 1f / t.lossyScale.y, 1f / t.lossyScale.z);
                }
            }
        }

        public override void OnCancel()
        {

        }
    }
}
