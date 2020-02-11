using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Custom inspector for the conversation scriptable object.
/// </summary>
[CustomEditor(typeof(Conversation))]
public class ConversationInspector : Editor {
	private Conversation t;
	// private SerializedObject GetTarget;
	private NPC newSpeaker;
	private int removeSpeaker;
	private int selectSpeaker;
	private int insertIndex;
	private string[] names;
	private int moveToIndex, moveFromIndex;
	private int removeIndex;

	private void OnEnable() {
		t = (Conversation) target;
		// GetTarget = new SerializedObject(t);
		newSpeaker = null;
		removeSpeaker = -1;
		selectSpeaker = -1;
		insertIndex = 0;
		names = t.GetSpeakerNames();
		moveToIndex = 0;
		moveFromIndex = 0;
		removeIndex = 0;
		// Undo.RecordObject(target, "Conversation Editor");
	}

	/// <summary>
	/// Draws conversation inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Undo.RecordObject(target, "Conversation Editor");
		// GetTarget.Update();
		Undo.RecordObject(t, "Conversation editor");
		EditorStyles.textField.wordWrap = true;

		EditorGUILayout.LabelField("Speaker Editing");

		// Add speaker object box
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Add new Speaker") &&
			newSpeaker != null &&
			!t.HasSpeaker(newSpeaker)) {
			t.AddSpeaker(newSpeaker);
			names = t.GetSpeakerNames();
			newSpeaker = null;
		}
		GUILayout.FlexibleSpace();
		newSpeaker = (NPC) EditorGUILayout.ObjectField(
			newSpeaker, typeof(NPC), true
		);
		EditorGUILayout.EndHorizontal();

		// Remove speaker dropdown
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Remove speaker") &&
			removeSpeaker >= 0 &&
			removeSpeaker < t.SpeakerCount) {
			t.RemoveSpeaker(removeSpeaker);
			names = t.GetSpeakerNames();
			removeSpeaker = -1;
		}
		GUILayout.FlexibleSpace();
		removeSpeaker = EditorGUILayout.Popup(removeSpeaker, names);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Section Editing");

		// Add new section button
		if (GUILayout.Button("Add New Section")) {
			t.AddEmptySection();
		}

		// Insert section button
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Insert Section At")) {
			t.Insert(insertIndex, new ConversationPage());
		}
		GUILayout.FlexibleSpace();
		insertIndex = EditorGUILayout.IntField(insertIndex);
		EditorGUILayout.EndHorizontal();

		// Move section button
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Move Section")) {
			t.Move(moveToIndex, moveFromIndex);
			moveToIndex = 0;
			moveFromIndex = 0;
		}
		EditorGUILayout.LabelField("To", GUILayout.Width(25));
		moveToIndex = EditorGUILayout.IntField(moveToIndex).Clamp(0, t.Count);
		EditorGUILayout.LabelField("From", GUILayout.Width(35));
		moveFromIndex = EditorGUILayout.IntField(moveFromIndex).Clamp(0, t.Count);
		EditorGUILayout.EndHorizontal();

		// Remove section button
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Remove Section")) {
			t.RemoveAt(removeIndex);
			removeIndex = 0;
		}
		GUILayout.FlexibleSpace();
		removeIndex = EditorGUILayout.IntField(removeIndex).Clamp(0, t.Count);
		EditorGUILayout.EndHorizontal();

		// Display section list to inspector window
		for (int i = 0; i < t.Count; i++) {
			ConversationPage section = t[i];
			NPC speaker = t.GetSpeaker(section);
			string speakerName = "";
			string content = section.content;
			bool show = section.shownInInspector;

			if (speaker != null) {
				speakerName = speaker.name;
			}

			bool hasName = !string.IsNullOrEmpty(speakerName);
			bool hasContent = !string.IsNullOrEmpty(content);

			// Section foldout name
			string foldoutText = string.Format("{0}) Empty Section", i);
			if (hasName && hasContent) {
				foldoutText = string.Format(
					"{0}) {1}: {2}",
					i, name, content
				);
			} else if (hasContent) {
				foldoutText = string.Format("{0}) {1}", i, content);
			} else if (hasName) {
				foldoutText = string.Format("{0}) {1}", i, speakerName);
			}

			// Section foldout
			section.shownInInspector = EditorGUILayout.Foldout(
				show, foldoutText
			);
			if (show) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Content");
				section.content = EditorGUILayout.TextArea(content);
				EditorGUILayout.EndHorizontal();
				if (content != null && content.Length > 366) {
					EditorGUILayout.HelpBox(
						"Content over character limit.",
						MessageType.Warning
					);
				}

				// Speaker selection dropdown
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("speaker");
				selectSpeaker = EditorGUILayout.Popup(selectSpeaker, names);
				if (selectSpeaker >= 0 && selectSpeaker < t.Count) {
					section.speakerIndex = selectSpeaker;
				}
				EditorGUILayout.EndHorizontal();
			}
		}
		// GetTarget.ApplyModifiedProperties();
		if (GUI.changed) {
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}
		// EditorUtility.SetDirty(t);
	}
}