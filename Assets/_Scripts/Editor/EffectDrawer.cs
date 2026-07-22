using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(EffectBase), true)]
public class EffectDrawer : PropertyDrawer
{
    private static Dictionary<string, Type> _typeMap;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (_typeMap == null) BuildTypeMap();

        // Safety check to prevent the PPtr error from before
        if (property.propertyType != SerializedPropertyType.ManagedReference)
        {
            EditorGUI.LabelField(position, label.text, "Error: Must use [SerializeReference]");
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        // 1. Calculate the Header Rect (where the foldout and button live)
        Rect headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        
        // 2. Draw the Foldout (The arrow)
        // We use a smaller width for the foldout so it doesn't overlap the button
        Rect foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        // 3. Draw the Type Selection Button
        // Position it where the value field usually is
        Rect buttonRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
        
        string typeName = property.managedReferenceFullTypename;
        string displayName = GetShortTypeName(typeName) ?? "Select Effect Type";

        // IMPORTANT: Use GUI.Button or EditorGUI.DropdownButton with a specific color to see if it's there
        if (EditorGUI.DropdownButton(buttonRect, new GUIContent(displayName), FocusType.Keyboard))
        {
            var menu = new GenericMenu();
            
            // Add a 'None' option to clear the effect
            menu.AddItem(new GUIContent("None"), string.IsNullOrEmpty(typeName), () =>
            {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            });

            menu.AddSeparator("");

            foreach (var kvp in _typeMap)
            {
                bool isSelected = typeName.EndsWith(kvp.Value.Name);
                menu.AddItem(new GUIContent(kvp.Key), isSelected, () =>
                {
                    property.managedReferenceValue = Activator.CreateInstance(kvp.Value);
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        }

        // 4. Draw children if expanded and object exists
        if (property.isExpanded && property.managedReferenceValue != null)
        {
            EditorGUI.indentLevel++;
            
            // Draw all visible children below the header
            float currentY = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            
            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();

            if (iterator.NextVisible(true)) // Enter the object
            {
                while (!SerializedProperty.EqualContents(iterator, endProperty))
                {
                    float childHeight = EditorGUI.GetPropertyHeight(iterator, true);
                    Rect childRect = new Rect(position.x, currentY, position.width, childHeight);
                    
                    EditorGUI.PropertyField(childRect, iterator, true);
                    
                    currentY += childHeight + EditorGUIUtility.standardVerticalSpacing;
                    if (!iterator.NextVisible(false)) break;
                }
            }
            
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        if (property.isExpanded && property.managedReferenceValue != null)
        {
            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();
            
            if (iterator.NextVisible(true))
            {
                while (!SerializedProperty.EqualContents(iterator, endProperty))
                {
                    height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
                    if (!iterator.NextVisible(false)) break;
                }
            }
            // Add a tiny bit of padding at the bottom
            height += EditorGUIUtility.standardVerticalSpacing;
        }

        return height;
    }

    static void BuildTypeMap()
    {
        _typeMap = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(asm => asm.GetTypes())
            .Where(t => !t.IsAbstract && typeof(EffectBase).IsAssignableFrom(t))
            .ToDictionary(t => ObjectNames.NicifyVariableName(t.Name), t => t);
    }

    static string GetShortTypeName(string fullTypeName)
    {
        if (string.IsNullOrEmpty(fullTypeName)) return null;
        return fullTypeName.Split('.').Last();
    }
}