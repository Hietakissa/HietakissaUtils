using UnityEngine;

public class ButtonAttribute : PropertyAttribute
{
    public readonly string FunctionName;
    public readonly string OverrideName;
    public readonly float Height;

    public ButtonAttribute(string functionName, string overrideName = "", float height = 20f)
    {
        FunctionName = functionName;
        OverrideName = overrideName;
        Height = height;
    }
}