using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
public class AiInput : MonoBehaviour {

    CharacterMovement m_Character;
    float turnSpeed = 1;
    public float runSpeed = 0.5f;

    float curTime = -1;
    const float rotTime = 1.0f;
    const float rotError = 0.5f;

    Queue<Collider> walkPoints = new Queue<Collider>();
    Vector3 endingDir = Vector3.zero;
    string queuedAction = "";
    bool queuedValue;

    // Use this for initialization
    void Start () {
        // get the third person character 
        m_Character = GetComponent<CharacterMovement>();
    }

    public void AddWalkPoint(Collider point)
    {
        walkPoints.Enqueue(point);
    }

    public void EndWithDirection(Vector3 dir)
    {
        dir.y = 0;
        endingDir = dir;
    }

    public void PrepareIdleAction(string action,bool paramValue)
    {
        queuedAction = action;
        queuedValue = paramValue;
    }

    void FixRotation(Vector3 dirV,Quaternion dirQ, ref float v, ref float h)
    {
        //turn if we aren't looking in the right direction
        if (Vector3.Angle(transform.forward,dirV) > rotError)
        {

            //time the length of slerp
            if (curTime == -1)
                curTime = 0;
            else
                curTime += Time.fixedDeltaTime;

            //rotate and determine which direction we rotated
            Vector3 oldRight = transform.right;

            transform.rotation = Quaternion.Slerp(transform.rotation, dirQ, curTime / rotTime);

            float dotDir = Vector3.Dot(oldRight, transform.forward);

            //determine turning direction for animation;
            if (dotDir < 0)
                h = -0.5f;
            else if (dotDir > 0)
                h = 0.5f;

        }
        else
        {

            //start moving once we're pointed in the right direction
            v = 0.5f;            
        }

    }

    // Update is called once per frame
    void FixedUpdate () {
        if (!m_Character.CanCollide())
            return;

        Vector3 m_Move = Vector3.zero;

        //calculate inputs
        float v = 0;
        float h = 0;

        //determine where to turn towards/move towards
        if(walkPoints.Count > 0)
        {
            Vector3 dest = walkPoints.Peek().transform.position;

            //we are looking in transform.forward, change to dest - transform.position
            Vector3 dir = dest - transform.position;
            dir.y = 0;
            Quaternion dirQ = Quaternion.LookRotation(dir, Vector3.up);            

            FixRotation(dir,dirQ, ref v, ref h);

        }
        //Finish path staring in direction endingDir
        else if(endingDir != Vector3.zero)
        {
            Quaternion dirQ = Quaternion.LookRotation(endingDir, Vector3.up);
            if (Vector3.Angle(transform.forward, endingDir) > rotError)
                FixRotation(endingDir, dirQ, ref v, ref h);
            else
                endingDir = Vector3.zero;
        }
        //After character has completed route, if idle action exists, perform action
        else if(queuedAction != "")
        {
            m_Character.SetIdleAction(queuedAction, queuedValue);
            queuedAction = "";
        }

        // calculate move direction to pass to character
        if (m_Character.CanMove())
        {
            v *= runSpeed;

            // we use local-relative directions in the case of no main camera
            m_Move = v * transform.forward;

        }
        else
        {
            v = 0;
        }

        //impart gravity continuously
        m_Move += new Vector3(0, -20f, 0);

        // pass all parameters to character controller
        m_Character.cControl.Move(m_Move * Time.fixedDeltaTime);

        //make animation changes
        m_Character.UpdateAnimator(m_Move, v, h * turnSpeed, false);

    }

    private void OnTriggerEnter(Collider collider)
    {

        if ((walkPoints.Count > 0) && (walkPoints.Peek() == collider))
        {
            walkPoints.Dequeue();
            curTime = -1;
        }

    }

}
