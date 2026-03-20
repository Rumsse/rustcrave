using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;
        bool shouldShow = GetConditionResult(showIf, property);

        if (shouldShow)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;
        bool shouldShow = GetConditionResult(showIf, property);

        // If false, return 0 height so it disappears completely
        return shouldShow ? EditorGUI.GetPropertyHeight(property, label, true) : 0f;
    }

    private bool GetConditionResult(ShowIfAttribute showIf, SerializedProperty property)
    {
        string path = property.propertyPath;
        int lastDot = path.LastIndexOf('.');
        string conditionPath = (lastDot != -1) 
            ? path.Substring(0, lastDot + 1) + showIf.ConditionName 
            : showIf.ConditionName;

        SerializedProperty conditionProperty = property.serializedObject.FindProperty(conditionPath);

        if (conditionProperty != null && conditionProperty.propertyType == SerializedPropertyType.Boolean)
        {
            // Check if the current bool value matches the attribute's requirement
            return conditionProperty.boolValue == showIf.ShowValue;
        }

        return true; // Default to showing if variable not found
    }
}