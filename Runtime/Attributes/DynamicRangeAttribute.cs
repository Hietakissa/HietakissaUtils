using UnityEngine;

public class DynamicRangeAttribute : PropertyAttribute
{
    public readonly DynamicRangeType RangeType;
    public readonly string CollectionName;
    public readonly bool ShouldDisplayName;

    public DynamicRangeAttribute(string collectionName, DynamicRangeType rangeType = DynamicRangeType.ArrayLength, bool shouldDisplayName = false)
    {
        CollectionName = collectionName;
        RangeType = rangeType;
        ShouldDisplayName = shouldDisplayName;
    }

    public enum DynamicRangeType
    {
        ArrayLength
    }
}
