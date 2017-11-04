using UnityEngine;

public class Bus_Animation : MonoBehaviour {

	static Animator anim;
    static Animator dAnim;

	[HideInInspector]
	public bool openDoors = false;
	[HideInInspector]
	public bool destReached = false;
	[HideInInspector]
	public bool turnComplete = true;

	// Use this for initialization
	void Start () {
		
		anim = GetComponent<Animator>();
        dAnim = transform.Find("busDoors").GetComponent<Animator>();

		//initial parameter set
		dAnim.SetBool("DoorOpen",openDoors);
		anim.SetBool("DestReached",destReached);
		anim.SetBool("TurnComplete",turnComplete);
	
	}

	public void ToggleDoorOpen(){
		openDoors = !openDoors;
		dAnim.SetBool("DoorOpen",openDoors);
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
