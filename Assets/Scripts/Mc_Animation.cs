using UnityEngine;
using System.Collections;

public class Mc_Animation : MonoBehaviour {

	static Animator anim;
	private Rigidbody body;
	public float speed = 2.0f;
	public float rotationSpeed = 75.0f;

	public string[] dontWalkStates;

	// Use this for initialization
	void Start () {
	
		anim = GetComponent<Animator>();
		body = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		if(anim.runtimeAnimatorController.name == "person.anim"){
			playerMode();
		}
	}

	void playerMode(){
		AnimatorStateInfo curState = anim.GetCurrentAnimatorStateInfo(0);

		if(Input.GetKeyDown("left shift")){
			speed *= 2;
		}

		else if(Input.GetKeyDown("space")){			
			anim.SetBool("IsSitting",!anim.GetBool("IsSitting"));
		}

		else if(Input.GetKeyUp("left shift")){
			speed /= 2;
		}

		if(!(inStateCategory(dontWalkStates, curState))){
			float translation = Input.GetAxis("Vertical") * speed;
			float rotation = Input.GetAxis("Horizontal") * rotationSpeed;
			translation *= Time.deltaTime;
			rotation *= Time.deltaTime;

			body.velocity = translation*transform.forward;
			transform.Rotate(0,rotation,0);

			if(translation != 0){
				anim.SetBool("IsWalking", true);
			}
			else{
				anim.SetBool("IsWalking",false);
			}
		}

	}

	private bool inStateCategory(string[] states,AnimatorStateInfo curState){
		foreach(string state in states){
			if(curState.IsName(state)){
				return true;
			}
		}

		return false;
	}
}
