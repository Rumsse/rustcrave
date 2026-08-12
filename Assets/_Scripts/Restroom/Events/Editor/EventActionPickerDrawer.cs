using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EventActionPickerAttribute))]
public class EventActionPickerDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.PropertyField(position, property, label, true);

        Rect popupPos = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

        string typeName = property.managedReferenceValue == null ? "Choose Action..." : property.managedReferenceValue.GetType().Name;

        if (GUI.Button(popupPos, typeName, EditorStyles.popup))
        {
            GenericMenu menu = new GenericMenu();
            Type baseType = GetBaseType(property);

            menu.AddItem(new GUIContent("None"), property.managedReferenceValue == null, () =>
            {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            });

            if (baseType != null)
            {
                var types = TypeCache.GetTypesDerivedFrom(baseType).Where(t => !t.IsAbstract && !t.IsInterface);
                foreach (Type type in types)
                {
                    menu.AddItem(new GUIContent(type.Name), property.managedReferenceValue?.GetType() == type, () =>
                    {
                        property.managedReferenceValue = Activator.CreateInstance(type);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
            }
            menu.ShowAsContext();
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true);
    }

    private Type GetBaseType(SerializedProperty property)
    {
        string typeName = property.managedReferenceFieldTypename;
        if (string.IsNullOrEmpty(typeName)) return null;
        string[] split = typeName.Split(' ');
        if (split.Length != 2) return null;
        return Type.GetType($"{split[1]}, {split[0]}");
    }
}