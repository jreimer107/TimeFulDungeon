#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;

/// <summary>
/// Custom inspector for the conversation scriptable object.
/// </summary>
[CustomEditor(typeof(Conversation))]
public class ConversationInspector : Editor {
	private Conversation t;
	private SerializedObject GetTarget;
	private SerializedProperty sections;
	private SerializedProperty speakers;
	private string[] names;
	private int count;

	// private NPC newSpeaker;
	// private int removeSpeaker;
	private int selectSpeaker;
	private int insertIndex;
	private int moveToIndex, moveFromIndex;
	private int removeIndex;

	private void OnEnable() {
		t = (Conversation)target;
		GetTarget = new SerializedObject(t);
		// names = new List<string>();

		// newSpeaker = null;
		// removeSpeaker = -1;
		insertIndex = 0;
		moveToIndex = 0;
		moveFromIndex = 0;
		removeIndex = 0;

		// EditorStyles.textField.wordWrap = true;
	}

	/// <summary>
	/// Draws conversation inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		serializedObject.Update();
		sections = serializedObject.FindProperty("sections");
		speakers = serializedObject.FindProperty("speakers");
		names = t.GetSpeakerNames();

		count = sections.arraySize;

		#region SpeakerEditing
		// EditorGUILayout.LabelField("Speaker Editing");

		// // Add speaker object box
		// EditorGUILayout.BeginHorizontal();
		// if (GUILayout.Button("Add new Speaker") &&
		// 	newSpeaker != null && !t.HasSpeaker(newSpeaker)) {
		// 	speakers.arraySize++;
		// 	speakers.GetArrayElementAtIndex(speakers.arraySize - 1).objectReferenceValue = newSpeaker;

		// 	Debug.Log(speakers.GetArrayElementAtIndex(speakers.arraySize - 1).serializedObject);
		// 	Debug.Log(speakers.GetArrayElementAtIndex(speakers.arraySize - 1).serializedObject.targetObject);
		// 	// names = t.GetSpeakerNames();
		// 	Debug.Log(speakers.arraySize);
		// 	Debug.Log(speakers.GetArrayElementAtIndex(speakers.arraySize - 1).objectReferenceValue);
		// 	newSpeaker = null;
		// }
		// GUILayout.FlexibleSpace();
		// newSpeaker = (NPC)EditorGUILayout.ObjectField(newSpeaker, typeof(NPC), true);
		// EditorGUILayout.EndHorizontal();

		// // Remove speaker dropdown
		// EditorGUILayout.BeginHorizontal();
		// if (GUILayout.Button("Remove speaker") && removeSpeaker >= 0 && removeSpeaker < speakers.arraySize) {
		// 	for (int i = 0; i < sections.arraySize; i++) {
		// 		SerializedProperty section = sections.GetArrayElementAtIndex(i);
		// 		SerializedProperty speakerIndex = section.FindPropertyRelative("speakerIndex");
		// 		if (speakerIndex.intValue == removeIndex) {
		// 			speakerIndex.intValue = -1;
		// 		}
		// 	}
		// 	// names = t.GetSpeakerNames();
		// 	removeSpeaker = -1;
		// }
		// GUILayout.FlexibleSpace();
		// removeSpeaker = EditorGUILayout.Popup(removeSpeaker, names);
		// EditorGUILayout.EndHorizontal();

		// EditorGUILayout.Space();
		#endregion

		#region SectionEditing
		EditorGUILayout.LabelField("Section Editing");

		// Add new section button
		if (GUILayout.Button("Add New Section")) {
			sections.arraySize++;
		}

		// Insert section button
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Insert Section At")) {
			sections.InsertArrayElementAtIndex(insertIndex);
		}
		GUILayout.FlexibleSpace();
		insertIndex = EditorGUILayout.IntField(insertIndex);
		EditorGUILayout.EndHorizontal();

		// Move section button
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Move Section")) {
			sections.MoveArrayElement(moveFromIndex, moveToIndex);
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.LabelField("To", GUILayout.Width(50));
		moveToIndex = EditorGUILayout.IntField(moveToIndex).Clamp(0, count);
		EditorGUILayout.LabelField("From", GUILayout.Width(50));
		moveFromIndex = EditorGUILayout.IntField(moveFromIndex).Clamp(0, count);
		EditorGUILayout.EndHorizontal();

		// Remove section button
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Remove Section") && sections.arraySize > 0) {
			sections.DeleteArrayElementAtIndex(removeIndex);
		}
		GUILayout.FlexibleSpace();
		removeIndex = EditorGUILayout.IntField(removeIndex).Clamp(0, count);
		EditorGUILayout.EndHorizontal();
		#endregion

		// Display section list to inspector window
		for (int i = 0; i < sections.arraySize; i++) {
			SerializedProperty section = sections.GetArrayElementAtIndex(i);
			SerializedProperty speaker = section.FindPropertyRelative("speaker");
			SerializedProperty content = section.FindPropertyRelative("content");
			SerializedProperty show = section.FindPropertyRelative("shownInInspector");

			string speakerName = speaker.objectReferenceValue ? (speaker.objectReferenceValue as NPC).name : "";
			bool hasName = !string.IsNullOrEmpty(speakerName);
			bool hasContent = !string.IsNullOrEmpty(content.stringValue);

			string shortContent = "";
			int shortLength = content.stringValue.Length.Min(20);
			int firstNewline = content.stringValue.IndexOf('\n', 0, shortLength);
			if (firstNewline != -1 && firstNewline < shortLength) {
				shortLength = firstNewline;
			}
			shortContent = content.stringValue.Substring(0, shortLength);

			string foldoutText = string.Format("{0}) Empty Section", i);
			if (hasName && hasContent) {
				foldoutText = string.Format(
					"{0}) {1}: {2}",
					i, speakerName, shortContent
				);
			} else if (hasContent) {
				foldoutText = string.Format("{0}) {1}", i, shortContent);
			} else if (hasName) {
				foldoutText = string.Format("{0}) {1}", i, speakerName);
			}

			// Section foldout
			show.boolValue = EditorGUILayout.Foldout(
				show.boolValue, foldoutText
			);
			if (show.boolValue) {
				content.stringValue = EditorGUILayout.TextArea(content.stringValue, GUILayout.Height(100));
				// EditorGUILayout.PropertyField(content, new GUIContent("content"), GUILayout.ExpandHeight(true), GUILayout.Height(100));
				if (content.stringValue.Length > 366) {
					EditorGUILayout.HelpBox(
						"Content over character limit.",
						MessageType.Warning
					);
				}
				EditorGUILayout.PropertyField(speaker, new GUIContent("speaker"));

				// Speaker selection dropdown
				// EditorGUILayout.BeginHorizontal();
				// EditorGUILayout.PrefixLabel("speaker");
				// selectSpeaker = EditorGUILayout.Popup(selectSpeaker, names);
				// if (selectSpeaker >= 0 && selectSpeaker < t.Count) {
				// section.speakerIndex = selectSpeaker;
				// }
				// EditorGUILayout.EndHorizontal();
			}
		}
		serializedObject.ApplyModifiedProperties();
	}
}

#endif