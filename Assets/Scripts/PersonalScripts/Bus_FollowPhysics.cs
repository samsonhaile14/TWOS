using UnityEngine;
using System.Collections;

public class Bus_FollowPhysics : MonoBehaviour {

	Transform busPhysics;

	// Use this for initialization
	void Start () {

		busPhysics = GameObject.Find(transform.parent.name + "/bus_physics").
			GetComponent<Transform>();
        
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		transform.position = busPhysics.position;
        transform.rotation = busPhysics.rotation;
	}
}
