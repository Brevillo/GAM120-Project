// Developed by Tom Kail at Inkle
// Released under the MIT Licence as held at https://opensource.org/licenses/MIT
// Modified by Oliver Beebe

// Must be placed within a folder named "Editor"

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Extends how ScriptableObject object references are displayed in the inspector
/// Shows you all values under the object reference
/// Also provides a button to create a new ScriptableObject if property is null.
/// </summary>
[CustomPropertyDrawer(typeof(ScriptableObject), true)]
public class ExtendedScriptableObjectDrawer : PropertyDrawer {

	// Styling
	private const int buttonWidth = 66;
	private const float verticalContentBuffer = 8;
	private static readonly GUIStyle ContentStyle = EditorStyles.helpBox;

	// String constants
	private const string scriptNameProperty = "m_Script";
	private static readonly List<string> ignoreClassFullNames = new() { "TMPro.TMP_FontAsset" };

	// Helper properties
	private static float LineHeight => EditorGUIUtility.singleLineHeight;
	private static float Spacing	=> EditorGUIUtility.standardVerticalSpacing;
	private static float LabelWidth => EditorGUIUtility.labelWidth;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

		float totalHeight = LineHeight;

		if (!AreAnySubPropertiesVisible(property) || !property.isExpanded) return totalHeight;

		var serializedObject = new SerializedObject(property.objectReferenceValue);
		var prop = serializedObject.GetIterator();

		if (prop.NextVisible(true))
			do {

				if (prop.name == scriptNameProperty) continue;

				var subProp = serializedObject.FindProperty(prop.name);
				totalHeight += EditorGUI.GetPropertyHeight(subProp, null, true) + Spacing;
			}
			while (prop.NextVisible(false));

		// Add height for the background
		totalHeight += verticalContentBuffer * 2 - Spacing * 2;
		serializedObject.Dispose();

		return totalHeight;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

        EditorGUI.BeginProperty(position, label, property);

		var type = GetFieldType();
		
		if (type == null || ignoreClassFullNames.Contains(type.FullName)) {
			EditorGUI.PropertyField(position, property, label);	
			EditorGUI.EndProperty ();
			return;
		}

		bool subPropVisible = AreAnySubPropertiesVisible(property);

		// So yeah having a foldout look like a label is a weird hack 
		// but both code paths seem to need to be a foldout or 
		// the object field control goes weird when the codepath changes.
		// I guess because foldout is an interactable control of its own and throws off the controlID?
		GUIStyle foldoutStyle = subPropVisible ? EditorStyles.foldout : EditorStyles.label;

		// Create foldout
		var guiContent = new GUIContent(property.displayName);
		var foldoutRect = new Rect(position.x, position.y, LabelWidth, LineHeight);
        bool expanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, guiContent, true, foldoutStyle);

		// Only update foldout expansion if properties are visible
		if (subPropVisible) property.isExpanded = expanded;

		float offset = LabelWidth + Spacing;
		var propertyRect = new Rect(position.x + offset, position.y, position.width - offset - Spacing / 2f, LineHeight);

		// Make space for "Create" button if necessary
		if ((!property.hasMultipleDifferentValues
            && property.serializedObject.targetObject != null
            && property.serializedObject.targetObject is ScriptableObject)
			|| property.objectReferenceValue == null)
			propertyRect.width -= buttonWidth + Spacing;

		// Create field for object reference
		EditorGUI.ObjectField(propertyRect, property, type, GUIContent.none);

		if (GUI.changed) property.serializedObject.ApplyModifiedProperties();

		bool propertyExists = property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null;

        // Display scriptable object reference
        if (propertyExists && property.isExpanded) {

			// Draw a background that shows us clearly which fields are part of the ScriptableObject
			var backgroundRect = new Rect(0, position.y + LineHeight + Spacing, position.width - Spacing, position.height - LineHeight - Spacing);
			GUI.Box(backgroundRect, "", ContentStyle);

			// Iterate over all the values and draw them
			var serializedObject = new SerializedObject(property.objectReferenceValue);
			var prop = serializedObject.GetIterator();

			float y = position.y + LineHeight + verticalContentBuffer;
			float width = position.width - buttonWidth;
			EditorGUI.indentLevel++;

            if (prop.NextVisible(true))
				do {

					if (prop.name == scriptNameProperty) continue;

					float height = EditorGUI.GetPropertyHeight(prop, new GUIContent(prop.displayName), true);
					EditorGUI.PropertyField(new Rect(position.x, y, width, height), prop, true);
					y += height + Spacing;
				}
				while (prop.NextVisible(false)); 

			if (GUI.changed) serializedObject.ApplyModifiedProperties();

			serializedObject.Dispose();
			EditorGUI.indentLevel--;
		}

		// Display create button
		var buttonRect = new Rect(position.x + position.width - buttonWidth, position.y, buttonWidth, LineHeight);
		if (!propertyExists && GUI.Button(buttonRect, "Create")) {

			// Create a new ScriptableObject in the open Project window folder
			var asset = ScriptableObject.CreateInstance(type);
			ProjectWindowUtil.CreateAsset(asset, $"{type.Name}.asset");

			property.objectReferenceValue = asset;
		}

		property.serializedObject.ApplyModifiedProperties();
		EditorGUI.EndProperty ();
	}

	private static bool AreAnySubPropertiesVisible(SerializedProperty property) {

		if (property.objectReferenceValue == null) return false;

		var serializedObject = new SerializedObject(property.objectReferenceValue);
		var prop = serializedObject.GetIterator();

		while (prop.NextVisible(true)) {

			if (prop.name == scriptNameProperty) continue;

			return true;
		}

		serializedObject.Dispose();
		return false;
	}

	private Type GetFieldType () {

		var type = fieldInfo.FieldType;

		if (type.IsArray) type = type.GetElementType();
		else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) type = type.GetGenericArguments()[0];

		return type;
	}
}