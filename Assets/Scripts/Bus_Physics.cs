using UnityEngine;
using System.Collections;
using System;

public class Bus_Physics : MonoBehaviour {

	// Use this for initialization
	void Start () {
		int src = LayerMask.GetMask("Player Encloser");
		int dest = LayerMask.GetMask("Player");
		int encl = LayerMask.GetMask("Enclosed");

		Physics.IgnoreLayerCollision((int)Math.Log(src,2),(int)Math.Log(dest,2));
		Physics.IgnoreLayerCollision((int)Math.Log(src,2),(int)Math.Log(encl,2));
	}
	
	// Update is called once per frame
	void Update () {
	
	}
		
}
