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

        //preparation
            Renderer startArrow = GameObject.Find("Waypoint/Arrow").GetComponent<Renderer>();
            CharacterMovement userMovement = GameObject.Find("Player").GetComponent<CharacterMovement>();
            CharacterMovement janeMovement = GameObject.Find("Jane").GetComponent<CharacterMovement>();
            CharacterMovement driverMovement = GameObject.Find("BusDriver").GetComponent<CharacterMovement>();
            CharacterMovement adrianMovement = GameObject.Find("Adrian").GetComponent<CharacterMovement>();

        //execution
        startArrow.enabled = false;

        janeMovement.SetIdleAction("Sit", true);       
        userMovement.SetIdleAction("Sit", true);
        janeMovement.SetIdleAction("Sit", true);
        driverMovement.SetIdleAction("Sit", true);
        adrianMovement.SetIdleAction("Sit", true);

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        //event 1: pan camera to Clyde and bring up text dialogue
        events.Enqueue( () => {
			//preparation
				Camera_Controller mCamera = GameObject.Find("Main Camera").
										GetComponent<Camera_Controller>();
				UI_ScreenInterface mScreen = GameObject.Find("Canvas").GetComponent<UI_ScreenInterface>();
                Transform uTransform = GameObject.Find("Player").GetComponent<Transform>();

            //execution

                //Set Textbox dialogue
                mScreen.DeliverDialogue( new string[] {" It was early in the morning as I sat on a bench on the sidewalk " +
									    "alongside other kids, waiting for the bus to arrive. I had kept to " +
									    "myself, distancing myself from the surrounding kids."} );

                //Shift camera closer to player
				mCamera.turnOnWorldView(Vector3.up*10.0f,
					    uTransform.position - Vector3.right *10.0f + Vector3.up*10.0f,0,uTransform);
				mCamera.turnOnWorldView(Vector3.up*10.0f,
					uTransform.position - Vector3.right *5.0f + Vector3.up*10.0f,5.0f,uTransform);
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
                    Collider[] busPoints = new Collider[4];
                    Bus_Movement busPlan = GameObject.Find("bus/bus_physics").GetComponent<Bus_Movement>();

                    for(int index = 1; index < 4; index++)
                    {
                        busPoints[index - 1] = GameObject.Find("BusTargets/BusTarget" + Convert.ToString(index)).
                                                GetComponent<Collider>();
                    }

				//execution
                    //look at bus as it comes
				    mCamera.turnOnWorldView(Vector3.up * 3.0f + Vector3.forward *15.0f,
					    mCamera.worldPos,5.0f,busPlan.transform);

                    //set points guiding bus
                    for(int index = 0; index < 3; index++) {
                        busPlan.addDrivePoint(busPoints[index]);
                    }

			});

			//condition 2: wait for bus to reach destination
			conditions.Enqueue( () => {

                //preparation
                    Bus_Movement busPlan = GameObject.Find("bus/bus_physics").GetComponent<Bus_Movement>();
                    Bus_Animation busAnim = GameObject.Find("bus/bus_mesh").GetComponent<Bus_Animation>();

                //execution
                    //wait till bus finishes driving
                    return !(busPlan.isDriving() || !busAnim.openDoors);

			} );

        //event 3: Open control back to player and get Amber moving
        events.Enqueue(() => {
            //preparation
                Camera_Controller mCamera = GameObject.Find("Main Camera").GetComponent<Camera_Controller>();
                CharacterMovement pAnim = GameObject.Find("Player").GetComponent<CharacterMovement>();
                UserInput uInput = GameObject.Find("Player").GetComponent<UserInput>();
                AiInput aInput = GameObject.Find("Amber").GetComponent<AiInput>();
                Renderer arrow = GameObject.Find("Waypoint/Arrow").GetComponent<Renderer>();
                

                Collider[] aiPoints = new Collider[4];

                for (int index = 0; index < 4; index++)
                {
                    aiPoints[index] = GameObject.Find("AiTargets/AiTarget" + Convert.ToString(index)).
                                            GetComponent<Collider>();
                }

            //execution
                //set camera back to player controls
                mCamera.turnOffWorldView();

                //set points guiding AI
                for (int index = 0; index < 4; index++)
                    aInput.AddWalkPoint(aiPoints[index]);

                aInput.EndWithDirection(aiPoints[3].transform.forward);

                //Unseat Clyde so he can start moving again
                uInput.PrepareIdleAction("Sit", false);

                //Give Amber instruction to sit if she finds her seat
                aInput.PrepareIdleAction("Sit", true);

                //activate guiding marker
                arrow.enabled = true;
        });

        //condition 3: Wait for Player to navigate to waypoint
        conditions.Enqueue(() => {

            //preparation
            TriggerReporter tReporter = GameObject.Find("PlayerTarget").GetComponent<TriggerReporter>();

            //execution
            return tReporter.TriggeredCollision;
        });

        //event 4: Deactivate Player controls, sit down player and amber, and proceed with remainder of stage events
        events.Enqueue(() => {
            //Camera_Controller mCamera = GameObject.Find("Main Camera").GetComponent<Camera_Controller>();
            Transform targetDir = GameObject.Find("PlayerTarget").transform;
            CharacterMovement pAnim = GameObject.Find("Player").GetComponent<CharacterMovement>();
            UserInput uInput = GameObject.Find("Player").GetComponent<UserInput>();
            Renderer arrow = GameObject.Find("Waypoint/Arrow").GetComponent<Renderer>();


            //execution
                //set camera to approriate view

                //sit down Amber

                //Sit down player again
                uInput.transform.rotation = Quaternion.LookRotation(targetDir.forward);
                uInput.PrepareIdleAction("Sit", true);

                //deactivate guiding marker
                arrow.enabled = false;

        });

        //condition 4: TBD
        conditions.Enqueue(() => {


            //execution
            return false;
        });

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
