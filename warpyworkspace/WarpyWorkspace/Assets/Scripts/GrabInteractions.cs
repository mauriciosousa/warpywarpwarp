using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabInteractions : MonoBehaviour {

    public GameObject[] GrabbableObjects;

    public Transform LeftThumbTip;
    public Transform[] OtherLeftTips;

    public Transform RightThumbTip;
    public Transform[] OtherRightTips;

    public bool CanGrab;

    public float THUMB_TRIGGER_DISTANCE = 0.05f;

    public GameObject GrabbedObject;

    void Start () {
		
	}
	
	void Update () {

        if (!_check(LeftThumbTip, OtherLeftTips))
        {
            _check(RightThumbTip, OtherRightTips);
        }
    }

    private bool _check(Transform thumb, Transform[] tip)
    {
        Vector3 pinchPosition = Vector3.zero;
        bool havePinch = false;
        for (int i = 0; i < tip.Length; i++)
        {
            if ((thumb.position - tip[i].position).magnitude < THUMB_TRIGGER_DISTANCE)
            {
                pinchPosition = (thumb.position + tip[i].position) / 2;
                havePinch = true;
                break;
            }
        }

        if (havePinch)
        {
            if (GrabbedObject != null && GrabbedObject.GetComponent<Collider>().bounds.Contains(pinchPosition))
            {
                GrabbedObject.transform.position = pinchPosition;
                return true;
            }
            else if (GrabbedObject == null)
            {
                float distance = float.PositiveInfinity;
                GameObject choosenOne = null;
                for (int i = 0; i < GrabbableObjects.Length; i++)
                {
                    if (GrabbableObjects[i] != null && Vector3.Distance(GrabbableObjects[i].transform.position, pinchPosition) < distance)
                    {
                        choosenOne = GrabbableObjects[i];
                        distance = Vector3.Distance(GrabbableObjects[i].transform.position, pinchPosition);
                    }
                }

                if (choosenOne != null && choosenOne.GetComponent<Collider>().bounds.Contains(pinchPosition))
                {
                    GrabbedObject = choosenOne;
                    GrabbedObject.transform.position = pinchPosition;
                    return true;
                }
            }
        }
        else
        {
            GrabbedObject = null;
        }

        return false;
    }
}
