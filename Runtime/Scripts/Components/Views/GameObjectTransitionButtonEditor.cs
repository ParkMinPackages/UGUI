#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace ParkMinPackages.UGUI.Components.Views
{
	[CustomEditor(typeof(GameObjectTransitionButton))]
	public class GameObjectTransitionButtonEditor : ButtonEditor
	{
		SerializedProperty normalState;
		SerializedProperty highlightedState;
		SerializedProperty pressedState;
		SerializedProperty selectedState;
		SerializedProperty disabledState;
		GameObjectTransitionButton _target;

		protected override void OnEnable() {
			base.OnEnable();
			normalState = serializedObject.FindProperty("_normalState");
			highlightedState = serializedObject.FindProperty("_highlightedState");
			pressedState = serializedObject.FindProperty("_pressedState");
			selectedState = serializedObject.FindProperty("_selectedState");
			disabledState = serializedObject.FindProperty("_disabledState");
			_target = target as GameObjectTransitionButton;
		}

		public override void OnInspectorGUI() {
			// Draw base Unity Button inspector
			base.OnInspectorGUI();

			// Draw our custom fields
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("State Objects", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(normalState);
			EditorGUILayout.PropertyField(highlightedState);
			EditorGUILayout.PropertyField(pressedState);
			EditorGUILayout.PropertyField(selectedState);
			EditorGUILayout.PropertyField(disabledState);

			EditorGUILayout.Space();
			if (GUILayout.Button("Auto Assign State Objects")) {
				try {
					Transform transform = _target.transform;

					normalState.objectReferenceValue = transform.GetChild(0).gameObject;
					highlightedState.objectReferenceValue = transform.GetChild(0).gameObject;
					pressedState.objectReferenceValue = transform.GetChild(0).gameObject;
					selectedState.objectReferenceValue = transform.GetChild(0).gameObject;
					disabledState.objectReferenceValue = transform.GetChild(0).gameObject;

					highlightedState.objectReferenceValue = transform.GetChild(1).gameObject;
					pressedState.objectReferenceValue = transform.GetChild(2).gameObject;
					selectedState.objectReferenceValue = transform.GetChild(3).gameObject;
					disabledState.objectReferenceValue = transform.GetChild(4).gameObject;
				}
				catch (Exception e) { }
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif