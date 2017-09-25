using UnityEngine;
using System.Collections;

public class Bus_Animation : MonoBehaviour {

	static Animator anim;

	[HideInInspector]
	public bool openDoors = false;
	[HideInInspector]
	public bool destReached = false;
	[HideInInspector]
	public bool turnComplete = true;

	// Use this for initialization
	void Start () {
		
		anim = GetComponent<Animator>();

		//initial parameter set
		anim.SetBool("OpenDoors",openDoors);
		anim.SetBool("DestReached",destReached);
		anim.SetBool("TurnComplete",turnComplete);
	
	}

	public void ToggleDoorOpen(){
		openDoors = !openDoors;
		anim.SetBool("OpenDoors",openDoors);
	}

	public void ToggleDestReached(){
		destReached = !destReached;
		anim.SetBool("DestReached",destReached);
	}

	public void ToggleTurnComplete(){
		turnComplete = !turnComplete;
		anim.SetBool("TurnComplete",turnComplete);
	}

}
