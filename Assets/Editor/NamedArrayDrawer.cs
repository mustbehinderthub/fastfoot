using UnityEngine;

#if UNITY_EDITOR
using System;
using UnityEditor;
#endif

// Defines an attribute that makes the array use enum values as labels.
// Use like this:
//      [NamedArray(typeof(eDirection))] public GameObject[] m_Directions;


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(NamedArrayAttribute))]
public class NamedArrayDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Properly configure height for expanded contents.
        return EditorGUI.GetPropertyHeight(property, label, property.isExpanded);
    }
    private Editor editor = null;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Replace label with enum name if possible.
        try
        {
            var config = attribute as NamedArrayAttribute;
            var enum_names = System.Enum.GetNames(config.TargetEnum);
            int pos = int.Parse(property.propertyPath.Split('[', ']')[1]);
            var enum_label = enum_names.GetValue(pos) as string;
            // Make names nicer to read (but won't exactly match enum definition).
            enum_label = ObjectNames.NicifyVariableName(enum_label.ToLower());
            label = new GUIContent(enum_label);

            // Draw label
            EditorGUI.PropertyField(position, property, label, true);

            // Draw foldout arrow
            if (property.objectReferenceValue != null)
            {
                property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none);
            }

            // Draw foldout properties
            if (property.isExpanded)
            {
                // Make child fields be indented
                EditorGUI.indentLevel++;

                // Draw object properties
                if (!editor)
                    Editor.CreateCachedEditor(property.objectReferenceValue, null, ref editor);
                editor.OnInspectorGUI();

                // Set indent back to what it was
                EditorGUI.indentLevel--;
            }
        }
        catch
        {
            // keep default label
        }
        EditorGUI.PropertyField(position, property, label, property.isExpanded);
    }
}

[CustomPropertyDrawer(typeof(ScriptableObject), true)]
public class ScriptableObjectDrawer : PropertyDrawer
{
    // Cached scriptable object editor
    private Editor editorr = null;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Draw label
        EditorGUI.PropertyField(position, property, label, true);

        // Draw foldout arrow
        if (property.objectReferenceValue != null)
        {
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none);
        }

        // Draw foldout properties
        if (property.isExpanded && property != null)
        {
            // Make child fields be indented
            EditorGUI.indentLevel++;

            // Draw object properties
            if (!editorr)
                Editor.CreateCachedEditor(property.objectReferenceValue, null, ref editorr);
            editorr.OnInspectorGUI();

            // Set indent back to what it was
            EditorGUI.indentLevel--;

        }
    }
}
#endif
