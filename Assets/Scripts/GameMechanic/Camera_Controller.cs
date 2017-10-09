using UnityEngine;
using System.Collections;

public class Camera_Controller : MonoBehaviour {


	public Rigidbody targBody;
	public Transform target;


	/*																										   Classes*/
	//Transform Settings: Keeps settings used for modifying camera's transform
	[System.Serializable]
	public class TransformSettings{
		public Vector3 targetPosOffset = new Vector3(0,3.4f,0);
		public float lookSmooth = 100f;
		public float distanceFromTarget = -8;
		public float zoomSmooth = 10;
		public float maxZoom = -2;
		public float minZoom = -15;
		public bool smoothFollow = true;
		public float smooth = 0.05f;
		[HideInInspector]
		public float newDistance = -8;
		[HideInInspector]
		public float adjustmentDistance = -8;

		public float xRotation = -20;
		public float yRotation = -180;
		public float maxXRotation = 25;
		public float minXRotation = -85;
		public float vOrbitSmooth = 150;
		public float hOrbitSmooth = 150;
	}

	//InputSettings: used for settings inputs related to controlling camera 
	[System.Serializable]
	public class InputSettings{
		public string ORBIT_HORIZONTAL_SNAP = "OrbitHorizontalSnap";
		public string ORBIT_HORIZONTAL = "OrbitHorizontal";
		public string ORBIT_VERTICAL = "OrbitVertical";
		public string ZOOM = "Mouse ScrollWheel";
	}

	//CameraCollision: used for detecting collision objects between camera and target
	[System.Serializable]
	public class CameraCollision
	{
		public LayerMask collisionLayer;

		[HideInInspector]
		public bool colliding = false;
		[HideInInspector]
		public Vector3[] adjustedCameraClipPoints;
		[HideInInspector]
		public Vector3[] desiredCameraClipPoints;

		Camera camera;

		public void Initialize(Camera cam){
			camera = cam;
			adjustedCameraClipPoints = new Vector3[5];
			desiredCameraClipPoints = new Vector3[5];
		}

		bool CollisionDetectedAtClipPoints(Vector3[] clipPoints, Vector3 fromPosition){
			for(int i = 0; i < clipPoints.Length; i++){
				Ray ray = new Ray(fromPosition, clipPoints[i] - fromPosition);
				float distance = Vector3.Distance(clipPoints[i], fromPosition);
				if(Physics.Raycast(ray, distance, collisionLayer)){
					return true;
				}
			}
			return false;
		}

		public void UpdateCameraClipPoints(Vector3 camPosition, Quaternion atRotation, 
				ref Vector3[] intoArray){
			if(!camera)
				return;

			intoArray = new Vector3[5];

			float z = camera.nearClipPlane;
			float x = Mathf.Tan(camera.fieldOfView/3.41f) * z;
			float y = x / camera.aspect;

			//assign clip point locations
			intoArray[0] = (atRotation * new Vector3(-x,y,z)) + camPosition; //top left
			intoArray[1] = (atRotation * new Vector3(x,y,z)) + camPosition; //top right
			intoArray[2] = (atRotation * new Vector3(-x,-y,z)) + camPosition; //bottom left
			intoArray[3] = (atRotation * new Vector3(x,-y,z)) + camPosition; //bottom right

			intoArray[4] = camPosition - camera.transform.forward;
		}

		public float GetAdjustedDistanceWithRayFrom(Vector3 fromPos){
			float distance = -1;

			for(int i = 0; i < desiredCameraClipPoints.Length; i++ )
			{
				Ray ray = new Ray(fromPos, desiredCameraClipPoints[i] - fromPos);
				RaycastHit hit;
				if(Physics.Raycast(ray, out hit)){
					if(distance == -1){
						distance = hit.distance;
					} 
					else{
						if(hit.distance < distance){
							distance = hit.distance;
						}
					}
				}
			}

			if(distance == -1){
				return 0;
			} 
			else{
				return distance;
			}
		}

		public void CheckColliding(Vector3 targetPosition){
			if (CollisionDetectedAtClipPoints(desiredCameraClipPoints,targetPosition)){
				colliding = true;
			} 
			else{
				colliding = false;
			}
		}
	}

	//DebugSettings: used for debugging camera collision class
	[System.Serializable]
	public class DebugSettings{
		public bool drawDesiredCollisionLines = true;
		public bool drawAdjustedCollisionLines = true;
	}

	/*																										Variables */
	//Class instantiations
	public TransformSettings camTrans = new TransformSettings();
	public InputSettings inputSet = new InputSettings();
	public DebugSettings debug = new DebugSettings();
	public CameraCollision collision = new CameraCollision(); 

	//Local Coordinate variables (local to target Transform)
	Vector3 targetPos = Vector3.zero;
	Vector3 destination = Vector3.zero;
	Vector3 adjustedDestination = Vector3.zero;
	Vector3 camVel = Vector3.zero;
	float vOrbitInput, hOrbitInput, zoomInput, hOrbitSnapInput;

	//World Coordinate variables
	bool useWorldSpace = false;
	bool wasSmoothFollow;
	float standardSmooth;
	Vector3 worldFocus = Vector3.zero;
	Vector3 worldPivot = Vector3.zero;
    Transform worldTarget = null;

	public Vector3 worldPos{
		get{return worldPivot;}
	}
	public Vector3 worldRot{
		get{return worldFocus;}
	}


	//Receive Input
	bool inputSwitch = true;

	/*																										   Methods*/
	void Start(){
		wasSmoothFollow = camTrans.smoothFollow;
		standardSmooth = camTrans.smooth;

		vOrbitInput = hOrbitInput = zoomInput = hOrbitSnapInput = 0;

		MoveToTarget();

		collision.Initialize(Camera.main);
		collision.UpdateCameraClipPoints(transform.position, transform.rotation, 
			ref collision.adjustedCameraClipPoints);
		collision.UpdateCameraClipPoints(destination, transform.rotation, 
			ref collision.desiredCameraClipPoints);

	}

	void Update(){
		if(inputSwitch){
			GetInput();
		}
		else{
			vOrbitInput = hOrbitInput = zoomInput = hOrbitSnapInput = 0;
		}
		OrbitTarget();
		ZoomInOnTarget();
	}

	void GetInput(){
		vOrbitInput = Input.GetAxisRaw(inputSet.ORBIT_VERTICAL);
		hOrbitInput = Input.GetAxisRaw(inputSet.ORBIT_HORIZONTAL);
		hOrbitSnapInput = Input.GetAxisRaw(inputSet.ORBIT_HORIZONTAL_SNAP);
		zoomInput = Input.GetAxisRaw(inputSet.ZOOM);
	}

	void FixedUpdate(){
		MoveToTarget();
		LookAtTarget();
		OrbitTarget();

		if(!useWorldSpace){
			collision.UpdateCameraClipPoints(transform.position, transform.rotation, 
				ref collision.adjustedCameraClipPoints);
			collision.UpdateCameraClipPoints(destination, transform.rotation, 
			ref collision.desiredCameraClipPoints);
		}

		for(int index = 0; index < 5; index++){
			if(debug.drawDesiredCollisionLines){
				Debug.DrawLine(targetPos, collision.desiredCameraClipPoints[index],Color.white);
			}
			if(debug.drawAdjustedCollisionLines){
				Debug.DrawLine(targetPos, collision.adjustedCameraClipPoints[index],Color.green);
			}
		}

		if(!useWorldSpace){
			collision.CheckColliding(targetPos);
			camTrans.adjustmentDistance = collision.GetAdjustedDistanceWithRayFrom(targetPos);
		}
	}

	void MoveToTarget(){
        
		if(useWorldSpace){
			targetPos = worldFocus;
			destination = worldPivot;
            if (worldTarget != null)
                targetPos += worldTarget.position;
		}
		else{
			targetPos = targBody.position + Vector3.up * camTrans.targetPosOffset.y + 
				Vector3.forward * camTrans.targetPosOffset.z + 
				transform.TransformDirection(Vector3.right * camTrans.targetPosOffset.x);
			destination = Quaternion.Euler(camTrans.xRotation, 
				camTrans.yRotation+target.eulerAngles.y,0) * 
				-Vector3.forward * camTrans.distanceFromTarget;
			destination += targetPos;
		}

		//adjust position when collision detected and not using world space
		if(collision.colliding && !useWorldSpace){
			adjustedDestination = 
				Quaternion.Euler(camTrans.xRotation,camTrans.yRotation+target.eulerAngles.y,0) * 
				Vector3.forward * camTrans.adjustmentDistance;
			adjustedDestination += targetPos;

			if(camTrans.smoothFollow){
				transform.position = Vector3.SmoothDamp(transform.position, adjustedDestination, 
					ref camVel, camTrans.smooth);
			}
			else{
				transform.position = adjustedDestination;
			}
		}
		else{
			if(camTrans.smoothFollow){
				transform.position = Vector3.SmoothDamp(transform.position, destination, ref camVel, camTrans.smooth);
			}
			else{
				transform.position = destination;
			}
		}
	}

	void LookAtTarget(){

		Quaternion targetRotation;

		//determining coordinate space
		if(useWorldSpace){
            //assign world space offset of focus point
            Vector3 interestPoint = worldFocus - transform.position;

            //if focus point in world space has a set target, focus on target with offset
            if (worldTarget != null)
                interestPoint += worldTarget.position;
            
			targetRotation = Quaternion.LookRotation(interestPoint);
		}
		else{
            //standard focus on player object
			targetRotation = Quaternion.LookRotation(targetPos - transform.position);
		}

		//smoothing rotation
		if(camTrans.smoothFollow){
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, camTrans.smooth*Time.deltaTime);
		}
		else{
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, camTrans.lookSmooth * Time.deltaTime);
		}
	}

	void OrbitTarget(){
		if(hOrbitSnapInput > 0){
			camTrans.yRotation = -180;
		}

		camTrans.xRotation += -vOrbitInput * camTrans.vOrbitSmooth * Time.deltaTime;
		camTrans.yRotation += -hOrbitInput * camTrans.hOrbitSmooth * Time.deltaTime;

		if(camTrans.xRotation > camTrans.maxXRotation){
			camTrans.xRotation = camTrans.maxXRotation;
		}
		if(camTrans.xRotation < camTrans.minXRotation){
			camTrans.xRotation = camTrans.minXRotation;
		}

	}

    /*Description:
     * Uses input controls to change distance between character and camera
    */
    void ZoomInOnTarget(){
		camTrans.distanceFromTarget += zoomInput * camTrans.zoomSmooth;

		if(camTrans.distanceFromTarget > camTrans.maxZoom){
			camTrans.distanceFromTarget = camTrans.maxZoom;
		}
		if(camTrans.distanceFromTarget < camTrans.minZoom){
			camTrans.distanceFromTarget = camTrans.minZoom;
		}
	}

    /*Description:
     * Changes camera view to world space, where the the camera's position is set in world space as worldPivot and 
     *      the focus point of the camera is set with worldFocus. Optionally, the focus point of the camera can be assigned
     *      to a transform with offset worldFocus.
    */
	public void turnOnWorldView(Vector3 worldFocus, Vector3 worldPivot,float smoothDuration,Transform subject = null){
		this.worldFocus = worldFocus;
		this.worldPivot = worldPivot;
		camTrans.smooth = smoothDuration;
		useWorldSpace = true;

		if(camTrans.smooth > 0){
			camTrans.smoothFollow = true;
		}
		else{
			camTrans.smoothFollow = false;
			MoveToTarget();
			LookAtTarget();
		}

        worldTarget = subject;

	}

    /*Description:
     * Returns camera settings to standard (i.e. focus on player object with standard camera controls)
    */
    public void turnOffWorldView(){
		useWorldSpace = false;
		camTrans.smoothFollow = wasSmoothFollow;
		camTrans.smooth = standardSmooth;
	}
}
