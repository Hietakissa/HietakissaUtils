using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
public class ConditionalFieldDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (ShouldDisplay(attribute, property)) EditorGUI.PropertyField(position, property, label);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (ShouldDisplay(attribute, property)) return 16f;
        else return 0f;
    }

    bool ShouldDisplay(PropertyAttribute attribute, SerializedProperty property)
    {
        ConditionalFieldAttribute conditionalField = attribute as ConditionalFieldAttribute;
        SerializedProperty serializedProperty = property.serializedObject.FindProperty(conditionalField.ObjectName);

        if (serializedProperty == null) return false;
        else if (serializedProperty.propertyType == SerializedPropertyType.Boolean)
        {
            if (serializedProperty.boolValue == conditionalField.TargetBool) return true;
            return false;
        }
        else if (serializedProperty.propertyType == SerializedPropertyType.Enum)
        {
            if (serializedProperty.enumValueIndex == conditionalField.TargetEnum) return true;
            return false;
        }
        else if (serializedProperty.propertyType == SerializedPropertyType.ObjectReference)
        {
            if (serializedProperty.objectReferenceValue != null) return true;
            return false;
        }
        
        return false;
    }
}
