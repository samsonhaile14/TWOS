using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class IKHandling : MonoBehaviour {

    Animator anim;

    //vars representing left foot, right foot,left hand, and right hand in that order
    Vector3 lFPos, rFPos,lHPos,rHPos;
    Quaternion lFRot, rFRot,lHRot,rHRot;
    Transform leftFoot, rightFoot, leftHand, rightHand, hip;
    float lFWeight, rFWeight, lHWeight, rHWeight;

    //offset of bones/ik with respect to mesh
    public float feetOffsetY;
    public float sitOffsetY;

    //IK states and settings
    public int curState = 0;

	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();
        
        leftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        rightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);
        leftHand = anim.GetBoneTransform(HumanBodyBones.LeftHand);
        rightHand = anim.GetBoneTransform(HumanBodyBones.RightHand);
        hip = anim.GetBoneTransform(HumanBodyBones.Hips);

        lFRot = leftFoot.rotation;
        rFRot = rightFoot.rotation;
        lHRot = leftHand.rotation;
        rHRot = rightHand.rotation;

        lFPos = leftFoot.position;
        rFPos = rightFoot.position;
        lHPos = leftHand.position;
        rHPos = rightHand.position;

        //set all weights to zero
        lFWeight = rFWeight = lHWeight = rHWeight = 0;

    }
	
	// Update is called once per frame
	void FixedUpdate () {

        switch (curState)
        {
            case 0: //for active movement
                GroundFeet();//Used to keep feet alligned with ground
                break;
            case 1: //for sitting
                GroundFeet();                
                CushionSeat();
                break;
            default:
                break;
        }
    }


    //Assigns ik bones specified weights and values
    private void OnAnimatorIK(int layerIndex)
    {

        if (anim.runtimeAnimatorController.name == "ThirdPersonAnimatorController")
        {
            lFWeight = anim.GetFloat("LeftFoot");
            rFWeight = anim.GetFloat("RightFoot");
        }
        else
        {
            lFWeight = rFWeight = lHWeight = rHWeight = 0;
        }

        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, lFWeight);
        anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, rFWeight);
        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, lHWeight);
        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rHWeight);


        anim.SetIKPosition(AvatarIKGoal.LeftFoot, lFPos + new Vector3(0,feetOffsetY,0));
        anim.SetIKPosition(AvatarIKGoal.RightFoot, rFPos + new Vector3(0, feetOffsetY, 0));
        anim.SetIKPosition(AvatarIKGoal.LeftHand, lHPos);
        anim.SetIKPosition(AvatarIKGoal.RightHand, rHPos);

        anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, lFWeight);
        anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, rFWeight);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, lHWeight);
        anim.SetIKRotationWeight(AvatarIKGoal.RightHand, rHWeight);

        anim.SetIKRotation(AvatarIKGoal.LeftFoot, lFRot);
        anim.SetIKRotation(AvatarIKGoal.RightFoot, rFRot);
        anim.SetIKRotation(AvatarIKGoal.LeftHand, lHRot);
        anim.SetIKRotation(AvatarIKGoal.RightHand, rHRot);

    }

/// <summary>
/// Methods that Specify functionality of IK states
/// </summary>

    //keeps feet at ground level, parallel to ground
    private void GroundFeet()
    {

        RaycastHit leftHit;
        RaycastHit rightHit;

        Vector3 lPos = leftFoot.position;
        Vector3 rPos = rightFoot.position;

        if (Physics.Raycast(lPos, -Vector3.up, out leftHit, 5))
        {
            lFPos = leftHit.point;
            lFRot = Quaternion.FromToRotation(transform.up, leftHit.normal) * transform.rotation;
        }

        if (Physics.Raycast(rPos, -Vector3.up, out rightHit, 5))
        {
            rFPos = rightHit.point;
            rFRot = Quaternion.FromToRotation(transform.up, rightHit.normal) * transform.rotation;
        }

    }

    //Sets body position, with relation to seat
    private void CushionSeat()
    {
        RaycastHit hit;
        Vector3 bodyPos;
        Quaternion bodyRot;

        if(Physics.Raycast(hip.position,-Vector3.up,out hit, 3))
        {
            bodyPos = hit.point;
            bodyRot = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
            anim.MatchTarget(bodyPos + Vector3.up * sitOffsetY, bodyRot, AvatarTarget.Body, new MatchTargetWeightMask(Vector3.one, 1f), 0f, 1f);
        }


    }

}
