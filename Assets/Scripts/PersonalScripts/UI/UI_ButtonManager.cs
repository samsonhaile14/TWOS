using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class UI_ButtonManager : MonoBehaviour {

	private string nextScene = "";

	void Start(){

	}

	public void NewGameBtn(string sceneName){
		nextScene = sceneName;
		StartCoroutine("sceneTransition");
	}

	IEnumerator sceneTransition(){
		float fadeTime = GameObject.Find("GameMaster").GetComponent<Fade>().BeginFade(1);
		yield return new WaitForSeconds(fadeTime);
		SceneManager.LoadScene(nextScene);
	}

	public void QuitBtn(){
		Application.Quit();
	}
		
}
