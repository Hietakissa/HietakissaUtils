using UnityEngine;
using UnityEditor;

// Original from EditorAttributes (https://github.com/v0lt13/EditorAttributes/tree/main), modified/improved by me/Hietakissa
[CustomPropertyDrawer(typeof(MinMaxRangeAttribute))]
public class MinMaxRangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        MinMaxRangeAttribute minMaxAttribute = attribute as MinMaxRangeAttribute;

        if (property.propertyType == SerializedPropertyType.Vector2 || property.propertyType == SerializedPropertyType.Vector2Int)
        {
            bool isIntVector = property.propertyType == SerializedPropertyType.Vector2Int;

            float minValue = isIntVector ? property.vector2IntValue.x : property.vector2Value.x;
            float maxValue = isIntVector ? property.vector2IntValue.y : property.vector2Value.y;

            float oldMinValue = minValue;
            float oldMaxValue = maxValue;

            EditorGUI.BeginChangeCheck();

            if (minMaxAttribute.ShowValues)
            {
                float labelWidth = EditorGUIUtility.labelWidth;

                Rect prefixLabelRect = new Rect(position.x, position.y, labelWidth, EditorGUIUtility.singleLineHeight);
                EditorGUI.PrefixLabel(prefixLabelRect, label);

                //float fieldWidth = (position.width - labelWidth - 200f) / 3f;
                float fieldWidth = Mathf.Max(50f, (position.width - labelWidth) / 5f); // Improves and fixes value field scaling, not too wide and not too narrow, doesn't break at extremes
                const float fieldsOffset = 10f;

                Rect minValueRect = new Rect(prefixLabelRect.xMax, position.y, fieldWidth - fieldsOffset, EditorGUIUtility.singleLineHeight);
                Rect sliderRect = new Rect(minValueRect.xMax + fieldsOffset, position.y, position.width - labelWidth - fieldWidth * 2f, EditorGUIUtility.singleLineHeight);
                Rect maxValueRect = new Rect(sliderRect.xMax + fieldsOffset, position.y, fieldWidth - fieldsOffset, EditorGUIUtility.singleLineHeight);

                EditorGUI.MinMaxSlider(sliderRect, ref minValue, ref maxValue, minMaxAttribute.MinRange, minMaxAttribute.MaxRange);

                minValue = isIntVector ? EditorGUI.IntField(minValueRect, (int)minValue) : EditorGUI.FloatField(minValueRect, minValue);
                maxValue = isIntVector ? EditorGUI.IntField(maxValueRect, (int)maxValue) : EditorGUI.FloatField(maxValueRect, maxValue);
            }
            else EditorGUI.MinMaxSlider(position, label, ref minValue, ref maxValue, minMaxAttribute.MinRange, minMaxAttribute.MaxRange);

            if (EditorGUI.EndChangeCheck())
            {
                minValue = Mathf.Clamp(minValue, minMaxAttribute.MinRange, oldMaxValue);
                maxValue = Mathf.Clamp(maxValue, oldMinValue, minMaxAttribute.MaxRange);

                if (isIntVector) property.vector2IntValue = new Vector2Int((int)minValue, (int)maxValue);
                else property.vector2Value = new Vector2(minValue, maxValue);
            }
        }
        else EditorGUILayout.HelpBox("MinMaxSlider Attribute can only be attached to a Vector2 and Vector2Int", MessageType.Warning);
    }
}
