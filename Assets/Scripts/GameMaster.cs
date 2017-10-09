using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameMaster : MonoBehaviour {

	//Conditions to continue
	public static Queue< Func<bool> > conditions = new Queue< Func<bool> >(); 

	//Events that occur
	public static Queue< Action > events = new Queue< Action >();

	//event/condition alternation variable
	private bool eventTurn = true;

	// Use this for initialization
	void Start () {

		//initialize actions and conditions

			//event 1: pan camera to Clyde and bring up text dialogue
			events.Enqueue( () => {
				//preparation
				Camera_Controller mCamera = GameObject.Find("Main Camera").
										GetComponent<Camera_Controller>();
				UI_ScreenInterface mScreen = GameObject.Find("Canvas").GetComponent<UI_ScreenInterface>();
				Transform player = GameObject.Find("Clyde").GetComponent<Transform>();

				//execution
				mScreen.DeliverDialogue( new string[] {" It was early in the morning as I sat on a bench on the sidewalk " +
									 "alongside other kids, waiting for the bus to arrive. I had kept to " +
									 "myself, distancing myself from the surrounding kids."} );
				mCamera.turnOnWorldView(Vector3.up*3.0f,
					 player.position - Vector3.right *10.0f + Vector3.up*3.0f,0,player);
				mCamera.turnOnWorldView(Vector3.up*4.0f,
					player.position - Vector3.right *3.0f + Vector3.up*6.0f,5.0f,player);
			});

			//condition 1: Wait for user to complete dialogue
			conditions.Enqueue( () => {

				//preparation
				UI_ScreenInterface mScreen = GameObject.Find("Canvas").GetComponent<UI_ScreenInterface>();

				//execution
				return mScreen.IsTextRead();

			} );

			//event 2: pan camera to bus as it approaches the bench
			events.Enqueue( () => {
				//preparation
				Camera_Controller mCamera = GameObject.Find("Main Camera").GetComponent<Camera_Controller>();
				Transform fPoint = GameObject.Find("bus/bus_mesh").GetComponent<Transform>();
                Collider[] busPoints = new Collider[4];
                Bus_Movement busPlan = GameObject.Find("bus/bus_physics").GetComponent<Bus_Movement>();

                for(int index = 1; index < 4; index++)
                {
                    busPoints[index - 1] = GameObject.Find("BusTargets/BusTarget" + Convert.ToString(index)).
                                            GetComponent<Collider>();
                }

				//execution
				mCamera.turnOnWorldView(Vector3.up * 3.0f,
					mCamera.worldPos,5.0f,fPoint);

                for(int index = 0; index < 3; index++) {
                    busPlan.addDrivePoint(busPoints[index]);
                }

			});

			//condition 2: to be determined
			conditions.Enqueue( () => {

				//preparation

				//execution
				return false;

			} );

	}
		
	// Update is called once per frame
	void Update () {

		if(eventTurn && events.Count > 0){
			events.Peek()();
			events.Dequeue();
			eventTurn = false;
		}
		else if(conditions.Count > 0){
			if(conditions.Peek()()){
				conditions.Dequeue();
				eventTurn = true;
			}
		}

		//else all events are complete (maybe progress to next scene)

	}

}
