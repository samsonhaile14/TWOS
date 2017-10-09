using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bus_Movement : MonoBehaviour {

	//Automated driving points
	Queue<Collider> drivePoints = new Queue<Collider>();
	Bus_Animation animControl;
    Rigidbody rBody;

	float speed = 0.1f;
    float rSpeed = 0.1f;
    
    Quaternion targetRotation;
    Vector3 targetDirection;

	// Use this for initialization
	void Start () {
        //Setting up collision properties for bus
        int src = LayerMask.GetMask("Player Encloser");
        int dest = LayerMask.GetMask("Player");
        int encl = LayerMask.GetMask("Enclosed");

        Physics.IgnoreLayerCollision((int)Mathf.Log(src, 2), (int)Mathf.Log(dest, 2));
        Physics.IgnoreLayerCollision((int)Mathf.Log(src, 2), (int)Mathf.Log(encl, 2));

        //get Animator for actual bus mesh
        animControl = GameObject.Find(transform.parent.name + "/bus_mesh").
			GetComponent<Bus_Animation>();

        rBody = GetComponent<Rigidbody>();

        targetRotation = transform.rotation;
        targetDirection = Vector3.zero;
        rBody.interpolation = RigidbodyInterpolation.Interpolate;
        
	}

	// Update is called once per frame
	void FixedUpdate () {
		driveTowards();

        adjustBus();
	}

	public void addDrivePoint(Collider dest){
		drivePoints.Enqueue(dest);
	}

    void adjustBus(){
        rBody.MoveRotation(Quaternion.Slerp(transform.rotation,targetRotation,rSpeed * Time.fixedDeltaTime));
        rBody.MovePosition(transform.position + speed * targetDirection * Time.fixedDeltaTime);
    }

    void driveTowards(){
		if(drivePoints.Count > 0){
            Vector2 dirVec = new Vector2(drivePoints.Peek().transform.position.x - transform.position.x,
                                         drivePoints.Peek().transform.position.z - transform.position.z);

            targetRotation = Quaternion.LookRotation(new Vector3(dirVec.x, 0, dirVec.y), transform.up);
            targetDirection = new Vector3(dirVec.x, 0, dirVec.y);

        }
        else
        {
            //halt bus movement
            targetDirection = Vector3.zero;
            targetRotation = transform.rotation;

            //Prepare bus animations for boarding
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

	void OnTriggerEnter(Collider obj){
		if( (drivePoints.Count) > 0 && (obj == drivePoints.Peek()) )
			drivePoints.Dequeue();
	}
}
