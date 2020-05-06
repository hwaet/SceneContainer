using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

[CreateAssetMenu]
public class SceneContainer : ScriptableObject {

	[Header("Active Scenes")]
	[Tooltip("Only the first scene will be set as active upon loading")]
	public List<Object> scenes  = new List<Object>();
	[HideInInspector]
	public List<string> sceneNames = new List<string>();

	[Header("Settings")]
	public StageSettings stageSettings;

	[Header("Background Scenes")]
	public List<Object> backgrounds = new List<Object>();
	[HideInInspector]
	public List<string> backgroundNames = new List<string>();

	[Header("GUI")]
	public bool guiIsActive = false;

	//[Header("Scene-speciic Settings")]
	//public GameSettings settings;

	public void OnValidate()
	{
		sceneNames = new List<string>(scenes.Count);
		foreach (Object scene in scenes)
		{
			sceneNames.Add(scene.name.ToString());
		}
		backgroundNames = new List<string>(backgrounds.Count);
		foreach (Object scene in backgrounds)
		{
			backgroundNames.Add(scene.name.ToString());
		}
	}


	public void loadScenes()
	{
		removeEventSystem();

		for (int i=0; i< sceneNames.Count; i++)
		{
			string name = sceneNames[i];

			if (name != null)
			{
				if (i==0)
				{
					SceneManager.LoadScene(name, LoadSceneMode.Single);
				}
				else
				{
					SceneManager.LoadScene(name, LoadSceneMode.Additive);
				}
			}
			else
			{
				Debug.Log("missing scene Object");
			}
		}

	}


	/// <summary>
	/// Rmoves the active eventSystem
	/// </summary>
	private static void removeEventSystem()
	{
		EventSystem es = FindObjectOfType<EventSystem>();
		if (es != null) GameObject.Destroy(es.gameObject);
	}
	

	/// <summary>
	/// Additively load each scene in the background list
	/// </summary>
	public void loadBackground() {
		foreach (string name in backgroundNames)
		{
			SceneManager.LoadScene(name, LoadSceneMode.Additive);
		}
	}

	
	public void loadGUI() {
		if (guiIsActive == true) {
			SceneManager.LoadScene("GUI",LoadSceneMode.Additive);
		}
	}

	
	public void unloadAll() {
		foreach (string name in sceneNames)
		{
			if (SceneManager.GetSceneByName(name).isLoaded == true)
			{
				if (name != null) SceneManager.UnloadSceneAsync(name);
			}
		}

		foreach (string name in backgroundNames)
		{
			if (SceneManager.GetSceneByName(name).isLoaded == true)
			{
				SceneManager.UnloadSceneAsync(name);
			}
		}

		if (guiIsActive == true) {
			if (SceneManager.GetSceneByName("GUI").isLoaded==true) {
				SceneManager.UnloadSceneAsync("GUI");
			}
		}
	}
}
