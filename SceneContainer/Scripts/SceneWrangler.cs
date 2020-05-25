using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[System.Serializable]
public enum LevelLoadingProcess
{
	Idle,
	LoadingScreen,
	FadeAudio,
	SetActiveLoadingScreen,
	LoadNextScene,
	UnloadCurrentScene,
	LoadGui,
	SetActiveNextScene,
	LoadSceneSettings,
	LoadPlayerSaveData,
	Done
}
	

public class SceneWrangler : MonoBehaviour {
//	DialogManager dialogManager;

	[Header("Scene Container")]
	public SceneContainer currentSceneContainer;
	public SceneContainer targetContainer;

	//For tracking scene loading processes
	Scene currentScene;
	Scene loadingScene;
	public LevelLoadingProcess levelState=LevelLoadingProcess.Idle;

	public delegate void sceneAction();
	public static event sceneAction fadeAudio;
	public static event sceneAction sceneLoadStart;
	public static event sceneAction sceneLoadEnd;
	public static event sceneAction sceneLoadGUI;
	//public static event sceneAction sceneUnloadGUI;
	public static event sceneAction sceneLoadPlayerSaveData;
	public static event sceneAction sceneLoadComplete;

	Scene[] currentScenes;

	static string LOADING_SCREEN_NAME = "loadingScreen";
	static string LOADING_SCREEN_ANIM_TAG = "loadingAnim";
	static string LOADING_ANIM_STATE_NAME = "unloadScene";
	static string MENU_CAM_NAME = "menuCamera";

	/// <summary>
	/// Restarts the current scene by swapping the next scene with the current one, and calling the usual scene loading process
	/// </summary>
	[ContextMenu("RestartCurrentScene")]
	public void restart()
	{
		targetContainer = currentSceneContainer;
		StartCoroutine("switchScene");
	}

	/// <summary>
	/// Changes the target scene container to the passed scene container object.
	/// </summary>
	/// <param name="container"></param>
	public void setTarget(SceneContainer container) {
		targetContainer = container;
	}


	[ContextMenu("Load Target Container")]
	public void loadTargetContainer ()
	{
		currentScenes = getCurrentlyLoadedScenes();

		StartCoroutine("switchScene");
	}



	/// <summary>
	/// Iterate through the scene loading process, as defined by the LevelLoadingProgress enum, and complete upon reaching its end. This method will trigger several events at perscribed points in the process.
	/// </summary>
	IEnumerator switchScene()
	{
		while (levelState != LevelLoadingProcess.Done)
		{
			levelState += 1;

			switch (levelState)
			{
				case LevelLoadingProcess.LoadingScreen:
					//			currentScene = SceneManager.GetActiveScene(); //this.gameObject.scene;
					setAsPermanent();
					loadingScene = SceneManager.GetSceneByName(LOADING_SCREEN_NAME);
					if (loadingScene.name == null)
					{
						SceneManager.LoadScene(LOADING_SCREEN_NAME, LoadSceneMode.Additive);
					}
					break;
				case LevelLoadingProcess.SetActiveLoadingScreen:
					PlayerInput input = FindObjectOfType<PlayerInput>();
					if (input!=null) input.gameObject.SetActive(false);
					loadingScene = SceneManager.GetSceneByName(LOADING_SCREEN_NAME);
					if (loadingScene.name != null)	SceneManager.SetActiveScene(loadingScene);
					break;
				case LevelLoadingProcess.FadeAudio:
					if (fadeAudio != null) fadeAudio();
					break;
				case LevelLoadingProcess.LoadNextScene:
					if (sceneLoadStart != null) sceneLoadStart();
					targetContainer.loadScenes();
					if (sceneLoadEnd != null) sceneLoadEnd();
					break;
				case LevelLoadingProcess.SetActiveNextScene:
					Scene activeScene = SceneManager.GetSceneByName(targetContainer.sceneNames[0]);
					if (activeScene.isLoaded == false)
					{
						levelState -= 1; //pause the process until this scene is successfully loaded
					}
					else
					{
						SceneManager.SetActiveScene(activeScene);
					}
					break;
				case LevelLoadingProcess.UnloadCurrentScene:
					if (targetContainer == currentSceneContainer) break;
					if (currentSceneContainer != null) { currentSceneContainer.unloadAll(); }
					else
					{
						foreach (Scene currentScene in currentScenes)
						{
							if (currentScene.isLoaded != false) SceneManager.UnloadSceneAsync(currentScene);
						}
					}
					turnOffMenuCam ();
					break;
				case LevelLoadingProcess.LoadGui:
					if (sceneLoadGUI != null) sceneLoadGUI();
					targetContainer.loadGUI();
					targetContainer.loadBackground();
					break;
				case LevelLoadingProcess.LoadSceneSettings:
					//GameManager gameManager = FindObjectOfType<GameManager>();
					//if (gameManager != null)
					//{
					//	if (targetContainer.stageSettings != null)
					//	{
					//		gameManager.stageSettings = targetContainer.stageSettings;
					//	}
					//	else
					//	{
					//		gameManager.revertToDefaultSettings();
					//	}
					//}
					break;
				case LevelLoadingProcess.LoadPlayerSaveData:
					if (sceneLoadPlayerSaveData != null) sceneLoadPlayerSaveData();
					break;
				case LevelLoadingProcess.Done:
					if (targetContainer.guiIsActive == false)
					{
						addEventSysIfNeeded();
					}
					finishLoading();
					break;
				default:
					break;
			}

			yield return null;
		}
	}


	/// <summary>
	/// Returns the list of actively loaded scenes. This basically repeats the functionality of the deprecated SceneManager.getAllScenes() in the official api
	/// </summary>
	/// <returns></returns>
	private static Scene[] getCurrentlyLoadedScenes()
	{
		Scene[] currentScenes = new Scene[SceneManager.sceneCount];
		for (int count = 0; count < SceneManager.sceneCount; count++)
		{
			currentScenes[count] = SceneManager.GetSceneAt(count);
		}
		return currentScenes;
	}

	/// <summary>
	/// Sets the current object as non-destructable during a scene load. This will allow this scenewrangler to survive the new scene loading process, and perform any necessary post-loading steps
	/// </summary>
	private void setAsPermanent()
	{
		this.transform.SetParent(transform.root.parent, true);
		DontDestroyOnLoad(transform.gameObject);
	}


	/// <summary>
	/// Method call to wrap up any last steps of the scene loading process. The last of which is self-destructing.
	/// </summary>
	void finishLoading() 
	{		
		GameObject loadingScreenGameObj = GameObject.FindGameObjectWithTag(LOADING_SCREEN_ANIM_TAG);
		if (loadingScreenGameObj != null)
		{
			Animator anim = loadingScreenGameObj.GetComponent<Animator>();
			anim.Play(LOADING_ANIM_STATE_NAME, -1);
			Object.Destroy(anim.gameObject, 2f);
		}
		Object.Destroy(this.gameObject);
		levelState = LevelLoadingProcess.Done;

		if (sceneLoadComplete != null) sceneLoadComplete();
	}


	/// <summary>
	/// The unity GUI system requires an eventsystem component to exist in the active scene. This method creates that as needed.
	/// </summary>
	private void addEventSysIfNeeded() {
		EventSystem currentES = FindObjectOfType<EventSystem>();
		if (currentES==null) {
			GameObject es = new GameObject("EventSystem");
			es.AddComponent<EventSystem> ();
			es.AddComponent<StandaloneInputModule> ();
			es.AddComponent<BaseInputModule> ();
			es.GetComponent<EventSystem>().UpdateModules();
			print ("adding event system");
		}
	}


	/// <summary>
	/// when loading in a menu scene, the audio listener on the camera should be disabled
	/// </summary>
	private void turnOffMenuCam() {
		Camera menuCamera=null;

		Camera[] menuCams = FindObjectsOfType<Camera> ();
		foreach (Camera cam in menuCams) {
			if (cam.name == MENU_CAM_NAME) {
				menuCamera = cam;
			}
		}
		if (menuCamera != null) {
			menuCamera.GetComponent<AudioListener> ().enabled = false;
		}
	}
}
