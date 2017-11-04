using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class UserInput : MonoBehaviour {

    private CharacterMovement m_Character; // A reference to the ThirdPersonCharacter on the object
    private Transform m_Cam;                  // A reference to the main camera in the scenes transform
    private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.
    private float turnSpeed = 1;
    public float runSpeed = 1f;

    private bool runLock = false;

    UI_ScreenInterface pauseInterface;

    public void PrepareIdleAction(string action, bool paramValue)
    {
        m_Character.SetIdleAction(action, paramValue);
    }

    private void Start()
    {
        // get the transform of the main camera
        if (Camera.main != null)
        {
            m_Cam = Camera.main.transform;
        }
        else
        {
            Debug.LogWarning(
                "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
            // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
        }

        // get the third person character ( this should never be null due to require component )
        m_Character = GetComponent<CharacterMovement>();
        pauseInterface = GameObject.Find("Canvas").GetComponent<UI_ScreenInterface>();
    }

    private void Update()
    {

    if (!m_Character.CanMove() || pauseInterface.Paused)
        return;

#if !MOBILE_INPUT
        // check for shift press to run
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            runLock = !runLock;                
        }
#endif

        if (!m_Jump)
        {
            m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
        }

    }

    // Fixed update is called in sync with physics
    private void FixedUpdate()
    {

        if (!m_Character.CanCollide() || pauseInterface.Paused)
            return;

        Vector3 m_Move = Vector3.zero;

        // read inputs
        float h = CrossPlatformInputManager.GetAxis("Horizontal");
        float v = CrossPlatformInputManager.GetAxis("Vertical");
        bool crouch = Input.GetKey(KeyCode.C);

        //half speed
        v *= 0.5f;

        // calculate move direction to pass to character
        if (m_Character.CanMove())
        {
            if (v < 0)
                v = 0;

            v *= runSpeed;

            if (m_Cam != null)
            {
                // calculate camera relative direction to move:
                m_Move = v * (transform.forward);// + h * m_Cam.right;
            }
            else
            {
                // we use world-relative directions in the case of no main camera
                m_Move = v * Vector3.forward;// + h * Vector3.right;
            }
        }
        else
        {
            v = 0;
            h = 0;
        }

#if !MOBILE_INPUT
        // walk speed multiplier
        if (runLock)
        {
            m_Move *= 2f;
            v *= 2f;
        }
#else
        //double speed by default
        m_Move *= 2f;
        v *= 2f;
#endif


        //impart gravity continuously
        m_Move += new Vector3(0,-40f,0);

        // pass all parameters to the character controller
        m_Character.cControl.Move(m_Move*Time.fixedDeltaTime);

        //make animation changes
        m_Character.UpdateAnimator(m_Move, v, h * turnSpeed, crouch);

        transform.Rotate(transform.up, h * turnSpeed);

        m_Jump = false;
    }

}
