using UnityEngine;

public class MinMaxRangeAttribute : PropertyAttribute
{
    public readonly float MinRange;
    public readonly float MaxRange;

    public readonly bool ShowValues;

    public MinMaxRangeAttribute(float minRange, float maxRange, bool showValues = true)
    {
        MinRange = minRange;
        MaxRange = maxRange;
        ShowValues = showValues;
    }
}