using UnityEditor;
using UnityEngine;

public class ButtonAttribute : PropertyAttribute
{
    public readonly string FunctionName;
    public readonly string OverrideName;
    public readonly float Height;

    public ButtonAttribute(string functionName, string overrideName = "", float height = -1f)
    {
        FunctionName = functionName;
        OverrideName = overrideName;
        Height = height == -1f ? EditorGUIUtility.singleLineHeight : height;
    }
}