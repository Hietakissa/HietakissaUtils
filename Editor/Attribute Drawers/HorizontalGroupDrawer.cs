using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(HorizontalGroupAttribute))]
public class HorizontalGroupDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        HorizontalGroupAttribute horizontalGroup = attribute as HorizontalGroupAttribute;
        int groupSize = horizontalGroup.GroupSize;

        DrawGroup(position, property, label, groupSize);
    }

    void DrawGroup(Rect position, SerializedProperty property, GUIContent label, int groupSize)
    {
        EditorGUILayout.BeginHorizontal();

        for (int i = 0; i < groupSize; i++)
        {
            const int reservedPixelsPerCharacter = 8;


            float propertyWidth = position.width / groupSize;
            float labelWidth = Mathf.Clamp(property.displayName.Length * reservedPixelsPerCharacter, 0f, propertyWidth * 0.55f);

            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUIUtility.fieldWidth = propertyWidth - labelWidth;

            EditorGUILayout.PropertyField(property, true);

            /// ToDo: figure out a way to better calculate the minimum width for a property, for example a bool should NOT have the same amount of reserved space as a Vector3

            if (!property.Next(false)) break;
        }
        
        EditorGUILayout.EndHorizontal();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 0f;
}
