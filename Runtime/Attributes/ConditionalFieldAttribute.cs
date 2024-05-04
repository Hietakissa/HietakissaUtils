using UnityEngine;

public class ConditionalFieldAttribute : PropertyAttribute
{
    public readonly string ObjectName;
    public readonly bool TargetBool;
    public readonly int TargetEnum;

    public ConditionalFieldAttribute(string objectName, bool targetBool = true)
    {
        ObjectName = objectName;
        TargetBool = targetBool;
    }

    public ConditionalFieldAttribute(string objectName, int targetEnum)
    {
        ObjectName = objectName;
        TargetEnum = targetEnum;
    }
}
