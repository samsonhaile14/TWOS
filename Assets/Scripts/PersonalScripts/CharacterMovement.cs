using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : MonoBehaviour {

	[SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
	[SerializeField] float m_AnimSpeedMultiplier = 1f;
    const float k_Half = 0.5f;

    const string animControl = "ThirdPersonAnimatorController"; //Name of controller used for third person characters

    //required unity components
    [HideInInspector]
    public CharacterController cControl;
	Animator m_Animator;
    
	bool m_IsGrounded;

    //Fields for testing airborneness
    float airTime;
    bool startAirTimer;
    const float MIN_AIR_TIME = 0.75f;

    public bool AnimGrounded
    {
        get
        {
            return m_IsGrounded;
        }
    }

    public bool CanMove()
    {
        if ((m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded") ||
           m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Crouching")))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CanCollide()
    {
        if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded") ||
           m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Crouching") ||
           m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Airborne"))
        {
            return (cControl.enabled = true);
        }
        else
        {
            return (cControl.enabled = false);
        }

    }

	void Start()
	{
		m_Animator = GetComponent<Animator>();
        cControl = GetComponent<CharacterController>();

        if (m_Animator.runtimeAnimatorController.name != animControl)
        {
            Debug.Log("Error, unexpected controller used for character movement. Results may not be as expected");
        }

    }

    void FixedUpdate()
    {
        if (!cControl.enabled)
            return;

        UpdateGroundTime();
        if (airTime >= MIN_AIR_TIME)
            m_IsGrounded = false;
        else
            m_IsGrounded = true;
    }

    void UpdateGroundTime()
    {
        if (!cControl.isGrounded)
        {
            if (!startAirTimer)
                startAirTimer = true;
            else if (airTime < MIN_AIR_TIME)
                airTime += Time.fixedDeltaTime;
        }

        else
        {
            startAirTimer = false;
            airTime = 0;
        }
    }

    public void SetIdleAction(string action,bool value)
    {
        if (m_Animator.runtimeAnimatorController.name != animControl)
            return;

        m_Animator.SetBool(action, value);

        //Update collision settings based on animation state
        CanCollide();
    }

    public void UpdateAnimator(Vector3 move,float fSpeed, float rotAngle, bool isCrouching)
	{
        //no movement when player cannot move
        if (!CanMove())
            return;

		// update the animator parameters
		m_Animator.SetFloat("Forward", fSpeed, 0.1f, Time.deltaTime);
		m_Animator.SetFloat("Turn", rotAngle, 0.1f, Time.deltaTime);
		m_Animator.SetBool("Crouch", isCrouching);
		m_Animator.SetBool("OnGround", m_IsGrounded);
		if (!m_IsGrounded)
		{
            m_Animator.SetFloat("Jump", cControl.velocity.y);
		}

		// calculate which leg is behind, so as to leave that leg trailing in the jump animation
		// (This code is reliant on the specific run cycle offset in our animations,
		// and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
		float runCycle =
			Mathf.Repeat(
				m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
		float jumpLeg = (runCycle < k_Half ? 1 : -1) * fSpeed;
		if (m_IsGrounded)
		{
			m_Animator.SetFloat("JumpLeg", jumpLeg);
		}

		// the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
		// which affects the movement speed because of the root motion.
		if (m_IsGrounded && fSpeed > 0)
		{
			m_Animator.speed = m_AnimSpeedMultiplier;
		}
		else
		{
			// don't use that while airborne
			m_Animator.speed = 1;
		}
	}

}
