using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

[InitializeOnLoad]
public class CreateTags: Editor {

	/// <summary>
	/// This task runs automatically when the asset is loaded in unity. This should force the user to always have these tags available
	/// </summary>
	static CreateTags() {
		EditorApplication.update += createDefaultTags;
	}



	static void createDefaultTags() {
		string[] dialogTags = new string[]{"loadingScreen","menuCam","loadingAnim"};


		foreach (string tag in dialogTags) {
			AddTag(tag);
		}
		EditorApplication.update -= createDefaultTags;
	}


	/// <summary>
	/// helper function to add an individual tag
	/// </summary>
	/// <param name="tag">Tag.</param>
	public static void AddTag(string tag)
	{
		UnityEngine.Object[] tagManagerAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
		if ((tagManagerAsset != null) && (tagManagerAsset.Length > 0))
		{
			SerializedObject serializedTags = new SerializedObject(tagManagerAsset[0]);
			SerializedProperty tags = serializedTags.FindProperty("tags");

			for (int i = 0; i < tags.arraySize; ++i)
			{
				if (tags.GetArrayElementAtIndex(i).stringValue == tag)
				{
					return;     // Tag already present, nothing to do.
				}
			}

			Debug.Log("Initializing step on package load: Adding tag " + tag.ToString());
			tags.InsertArrayElementAtIndex(0);
			tags.GetArrayElementAtIndex(0).stringValue = tag;
			serializedTags.ApplyModifiedProperties();
			serializedTags.Update();
		}
	}

}
