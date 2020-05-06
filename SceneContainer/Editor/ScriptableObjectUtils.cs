using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ScriptableObjectUtils : MonoBehaviour {

	[OnOpenAssetAttribute(1)]
	public static bool step1(int instanceID, int line)
	{
		Object go = EditorUtility.InstanceIDToObject(instanceID);
		string name = go.name;
		if (go.GetType() == typeof(SceneContainer)) {
			Debug.Log("Opening a SceneContainer: (" + name + ")");
			SceneContainer container = go as SceneContainer;
			if (EditorApplication.isPlaying==false) {
				loadScenesInEditor(container);
				loadBackgroundInEditor(container);
				loadGUIInEditor(container);
			}
			else {
				(go as SceneContainer).loadScenes();
				(go as SceneContainer).loadBackground();
				(go as SceneContainer).loadGUI();
			}
		}
		return false; // we did not handle the open
	}


	static void loadScenesInEditor(SceneContainer container) {

		if (container.scenes.Count == 0) Debug.LogWarning("Attempted to load an empty list of scenes in the editor");

		bool firstScene=true;
		foreach (Object go in container.scenes) {
			if (firstScene==true) {
				EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(go),OpenSceneMode.Single);//go.name,UnityEngine.SceneManagement.LoadSceneMode.Additive
				firstScene = false;
			}
			else {
				EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(go),OpenSceneMode.Additive);//go.name,UnityEngine.SceneManagement.LoadSceneMode.Additive
			}
		}
	}

	static void loadBackgroundInEditor(SceneContainer container) {
		foreach (Object go in container.backgrounds) {
			EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(go),OpenSceneMode.Additive);//go.name,UnityEngine.SceneManagement.LoadSceneMode.Additive
		}
	}

	static void loadGUIInEditor(SceneContainer container) {
		if (container.guiIsActive) {
			string GUID = AssetDatabase.FindAssets("gui t:SceneAsset")[0];
			EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(GUID),OpenSceneMode.Additive);//go.name,UnityEngine.SceneManagement.LoadSceneMode.Additive
		}
	}


	//public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
	//{
	//	List<T> assets = new List<T>();
	//	string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
	//	for( int i = 0; i < guids.Length; i++ )
	//	{
	//		string assetPath = AssetDatabase.GUIDToAssetPath( guids[i] );
	//		T asset = AssetDatabase.LoadAssetAtPath<T>( assetPath );
	//		if( asset != null )
	//		{
	//			assets.Add(asset);
	//		}
	//	}
	//	return assets;
	//}


	//public static string CreateAsset<T> () where T : ScriptableObject
	//{
	//	T asset = ScriptableObject.CreateInstance<T> ();
		
	//	string path = AssetDatabase.GetAssetPath (Selection.activeObject);
	//	if (path == "") 
	//	{
	//		path = "Assets";
	//	} 
	//	else if (Path.GetExtension (path) != "") 
	//	{
	//		path = path.Replace (Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
	//	}

	//	string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + "/" + typeof(T).ToString() + ".asset");

	//	AssetDatabase.CreateAsset (asset, assetPathAndName);

	//	AssetDatabase.SaveAssets ();
	//	AssetDatabase.Refresh();
	//	EditorUtility.FocusProjectWindow ();
	//	Selection.activeObject = asset;

	//	return assetPathAndName;
	//}
}
