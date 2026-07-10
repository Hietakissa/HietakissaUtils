using UnityEngine;

namespace HietakissaUtils.Attributes
{
    public class HorizontalGroupAttribute : PropertyAttribute
    {
        public readonly int GroupSize;

        public HorizontalGroupAttribute(int groupSize)
        {
            GroupSize = groupSize;
        }
    }
}
