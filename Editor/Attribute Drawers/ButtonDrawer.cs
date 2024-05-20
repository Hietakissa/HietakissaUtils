using System.Reflection;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ButtonAttribute))]
public class ButtonDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ButtonAttribute button = attribute as ButtonAttribute;
        object buttonObject = property.serializedObject.targetObject;
        

        MethodInfo method = buttonObject.GetType().GetMethod(button.FunctionName);

        if (!IsMethodValid(method)) EditorGUILayout.LabelField("Method could not be found, or has parameters without a default value.");
        else
        {
            if (GUI.Button(position, button.OverrideName == "" ? method.Name : button.OverrideName))
            {
                method.Invoke(buttonObject, null);
            }
        }


        bool IsMethodValid(MethodInfo method)
        {
            if (method == null) return false;

            bool hasDefaults = true;
            ParameterInfo[] parameters = method.GetParameters();
            foreach (ParameterInfo parameter in parameters)
            {
                if (!parameter.HasDefaultValue)
                {
                    hasDefaults = false;
                    break;
                }
            }

            return hasDefaults;
            //method == null || method.GetParameters().Length > 0)
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (attribute as ButtonAttribute).Height;
    }
}
