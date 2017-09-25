using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bus_Movement : MonoBehaviour {

	//Automated driving points
	Queue<Collider> drivePoints = new Queue<Collider>();
	Bus_Animation animControl;

	float speed = 4.0f;

	// Use this for initialization
	void Start () {
		animControl = GameObject.Find(transform.parent.name + "/bus_mesh").
			GetComponent<Bus_Animation>();
	}

	// Update is called once per frame
	void Update () {
		driveTowards();
	}

	public void addDrivePoint(Collider dest){
		drivePoints.Enqueue(dest);
	}

	void driveTowards(){
		if(drivePoints.Count > 0){
			Debug.Log("moving");
			transform.position += (speed * transform.forward);
			Quaternion targetRotation = Quaternion.LookRotation(
				transform.position - drivePoints.Peek().transform.position);
			Quaternion rot = Quaternion.Lerp(transform.rotation, 
				targetRotation, 5.0f*Time.deltaTime);
			transform.rotation = Quaternion.Euler(transform.rotation.x,rot.y,transform.rotation.z);

			if(transform.rotation.y != rot.y){
				if(animControl.turnComplete){
					animControl.ToggleTurnComplete();
				}
			}
		}
		else{
			if(!animControl.destReached){
				animControl.ToggleDestReached();
			}
			if(!animControl.turnComplete){
				animControl.ToggleTurnComplete();
			}		
			if(!animControl.openDoors){
				animControl.ToggleDoorOpen();
			}
		}

	}

	void OnCollisionEnter(Collision obj){
		if( (drivePoints.Count) > 0 && (obj.collider == drivePoints.Peek()) ){
			drivePoints.Dequeue();
		}
	}
}
