using HietakissaUtils;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(DynamicRangeAttribute))]
public class DynamicRangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        DynamicRangeAttribute rangeAttribute = attribute as DynamicRangeAttribute;
        SerializedProperty collectionProperty = property.serializedObject.FindProperty(rangeAttribute.CollectionName);


        if (collectionProperty == null)
        {
            EditorGUILayout.HelpBox($"DynamicRange attribute couldn't find the Array '{rangeAttribute.CollectionName}'.", MessageType.Error);
            return;
        }

        int maxIndex = collectionProperty.arraySize - 1;
        if (maxIndex == -1)
        {
            if (property.propertyType == SerializedPropertyType.Integer) property.intValue = -1;
            else if (property.propertyType == SerializedPropertyType.Float) property.floatValue = -1;

            property.serializedObject.ApplyModifiedProperties();
            return;
        }

        if (property.propertyType == SerializedPropertyType.Integer)
        {
            property.intValue = Mathf.Clamp(property.intValue, 0, maxIndex);

            if (rangeAttribute.ShouldDisplayName) EditorGUI.LabelField(position, new GUIContent($"Selected: {collectionProperty.GetArrayElementAtIndex(property.intValue).displayName}"));
            EditorGUILayout.IntSlider(property, 0, maxIndex);
        }
        else if (property.propertyType == SerializedPropertyType.Float)
        {
            property.floatValue = Mathf.Clamp(property.floatValue, 0, maxIndex);

            if (rangeAttribute.ShouldDisplayName) EditorGUI.LabelField(position, new GUIContent($"Selected: {collectionProperty.GetArrayElementAtIndex(property.floatValue.RoundToNearest()).displayName}"));
            EditorGUILayout.Slider(property, 0, maxIndex);
        }
        else EditorGUILayout.HelpBox("DynamicRange attribute is only valid on Int and Float.", MessageType.Error);

        property.serializedObject.ApplyModifiedProperties();
    }
}