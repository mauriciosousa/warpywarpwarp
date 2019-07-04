﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProxemicDistances
{
    Intimate,
    Personal,
    Social,
    Public
}

public class ProxemicsAnalysis : MonoBehaviour {

    public IsColliding localHumanCollider;
    public bool humansColliding;

    public Transform localHumanSpineJoint;
    public Transform remoteHumanSpineJoint;

    public float distance = 0f;

    public ProxemicDistances distanceClassification = ProxemicDistances.Public;

    public Transform VRHead;

    public bool isFocusedOnWorkspace;

    public List<Collider> colliders;

    void Start () {
        isFocusedOnWorkspace = false;
	}
	
	
	void Update () {
        humansColliding = localHumanCollider.COLLIDING;

        Vector3 localParticipantPosition = new Vector3(localHumanSpineJoint.position.x, 0, localHumanSpineJoint.position.z);
        Vector3 remoteParticipantPosition = new Vector3(remoteHumanSpineJoint.position.x, 0, remoteHumanSpineJoint.position.z);

        distance = Vector3.Distance(localParticipantPosition, remoteParticipantPosition);

        if (distance <= 0.46f) distanceClassification = ProxemicDistances.Intimate;
        else if (distance > 0.46f && distance <= 1.2f) distanceClassification = ProxemicDistances.Personal;
        else if (distance > 1.2f && distance <= 3.7) distanceClassification = ProxemicDistances.Social;
        else distanceClassification = ProxemicDistances.Public;

        //Debug.DrawRay(VRHead.position, Camera.main.transform.forward, Color.red);
        //RaycastHit hit;

        //isFocusedOnWorkspace = false;
        //for (int i = 0; i < colliders.Count; i++)
        //{
        //    if (colliders[i].Raycast(new Ray(VRHead.position, Camera.main.transform.forward), out hit, float.PositiveInfinity))
        //    {
        //        isFocusedOnWorkspace = true;
        //        break;
        //    }
        //}
    }
}
