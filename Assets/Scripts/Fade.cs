using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Fade : MonoBehaviour {

	public Texture2D fadeOutTexture;

	public float fadeSpeed = 0.4f;
	private int drawDepth = -1000;
	private float alpha = 1f;
	private int fadeDir = -1;

	void OnGUI(){
		alpha += fadeDir * fadeSpeed * Time.deltaTime;
		alpha = Mathf.Clamp01(alpha);

		GUI.color = new Color(GUI.color.r,GUI.color.g,GUI.color.b,alpha);
		GUI.depth = drawDepth;
		GUI.DrawTexture( new Rect (0,0,Screen.width,Screen.height),fadeOutTexture);
	}

	public float BeginFade(int direction){
		fadeDir = direction;
		return (1f/fadeSpeed);
	}

	void OnEnable(){
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnDisable(){
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode){
		if(scene.name == "Opening"){
			BeginFade(-1);
		}
	}
}
