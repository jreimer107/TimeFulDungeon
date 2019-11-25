using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

public class ConversationEditor : EditorWindow {

	int selected = 0;
	List<ConversationPage> sections = new List<ConversationPage>();
	string ID;

	[MenuItem("Window/ConversationEditor")]
	public static void ShowWindow() {
		GetWindow<ConversationEditor>("ConversationEditor");
	}

	private void OnGUI() {
		EditorStyles.textField.wordWrap = true;

		ID = EditorGUILayout.TextField("Conversation ID", "");

		EditorGUILayout.BeginHorizontal();

		// All sections
		EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(100));
		
		// Add section button
		EditorGUILayout.LabelField("All Sections", GUILayout.Width(100));
		if (GUILayout.Button("Add New Section", GUILayout.Width(105))) {
			sections.Add(new ConversationPage("", null));
		}
		
		// Section Radios and remove buttons
		EditorGUILayout.BeginHorizontal();

		// Section Radios
		EditorGUILayout.BeginVertical(GUILayout.Width(30));
		selected = GUILayout.SelectionGrid(
			selected, 
			Array.ConvertAll(Enumerable.Range(1, sections.Count).ToArray(), x => x.ToString()), 
			1);
		EditorGUILayout.EndVertical();
		
		// Remove buttons
		EditorGUILayout.BeginVertical(GUILayout.Width(70));
		for (int i = 0; i < sections.Count; i++) {
			if (GUILayout.Button("Remove")) {
				sections.RemoveAt(i);
				if (selected >= i) {
					selected = selected == 0 ? 0 : selected - 1;
				}
				break;
			}
		}
		EditorGUILayout.EndVertical();

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndVertical();

		// Section content
		EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(300));

		EditorGUILayout.LabelField("Selected Section", GUILayout.Width(100));
		if (sections.Count != 0) {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Speaker", GUILayout.Width(100));
			sections[selected].speaker = (NPC)EditorGUILayout.ObjectField(sections[selected].speaker, typeof(NPC), true);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Content", GUILayout.Width(100));
			sections[selected].content = EditorGUILayout.TextArea(sections[selected].content, GUILayout.Width(200));
			EditorGUILayout.EndHorizontal();

			if (sections[selected].content.Length > 366) {
				EditorGUILayout.HelpBox("Content over character limit.", MessageType.Warning);
			}

		}
		
		EditorGUILayout.EndVertical();
		
		EditorGUILayout.EndHorizontal();
		

	}
}

[CreateAssetMenu(fileName = "New Conversation", menuName = "Conversation")]
public class Conversation : ScriptableObject {
	public string ID;
	public List<ConversationPage> sections;

	public Conversation() {
		ID = "";
		sections = new List<ConversationPage>();
	}

}

[System.Serializable]
public class ConversationPage {
	public string content;
	public NPC speaker;

	public ConversationPage(string content, NPC speaker) {
		this.content = content;
		this.speaker = speaker;
	}

	//public override void OnInspectorGUI() {
	//	base.OnInspectorGUI();
	//	showContent = EditorGUILayout.Foldout(
	//		showContent,
	//		string.Format("{0}: {1}", speaker.name, content)
	//	);
	//	if (showContent) {
	//		content = EditorGUILayout.TextArea(content);
	//		speaker = (NPC)EditorGUILayout.ObjectField(speaker, typeof(NPC), true);
	//	}
		
	//}

}

public class NPC : MonoBehaviour {
	public Sprite portrait;
	new public string name;

}

[CustomEditor(typeof(Conversation))]
public class ConversationInspector : Editor {
	enum displayFieldType { DisplayAsAutomaticFields, DisplayAsCustomizableGUIFields }
	displayFieldType DisplayFieldType;

	Conversation t;
	SerializedObject GetTarget;
	SerializedProperty ThisList;
	int ListSize;

	private void OnEnable() {
		t = (Conversation)target;
		GetTarget = new SerializedObject(t);
		ThisList = GetTarget.FindProperty("sections");
	}

	public override void OnInspectorGUI() {
		//Update list
		GetTarget.Update();

		//Add new item to list with button
		if (GUILayout.Button("Add New")) {
			t.sections.Add(new ConversationPage("", null));
		}

		// Display ou list to inspector window
		for (int i = 0; i < ThisList.arraySize; i++) {
			SerializedProperty SectionRef = ThisList.GetArrayElementAtIndex(i);
			SerializedProperty sectionContent = SectionRef.FindPropertyRelative("content");
			SerializedProperty sectionSpeaker = SectionRef.FindPropertyRelative("speaker");

			EditorGUILayout.PropertyField(sectionContent);
			EditorGUILayout.PropertyField(sectionSpeaker);
		}

	}
}