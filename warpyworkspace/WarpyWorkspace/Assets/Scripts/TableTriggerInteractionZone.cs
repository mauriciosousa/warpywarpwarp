using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableTriggerInteractionZone : MonoBehaviour {

    public AlteredTelepresenceMain main;
    public BodiesManager remoteBodies;

    private BoxCollider _collider;

    void Start () {
        _collider = GetComponent<BoxCollider>();
	}

    //void Update () {

    //       if (remoteBodies.human != null)
    //       {
    //           rightHandInside = _collider.bounds.Contains(rightHandTip.position);
    //           leftHandInside = _collider.bounds.Contains(leftHandTip.position);

    //           //if (rightHandInside) calcTargetPosition(TargetRight, rightHandTip);
    //           //if (leftHandInside) calcTargetPosition(TargetLeft, leftHandTip);
    //       }	
    //}

    public bool isHandInside(Vector3 handtip)
    {
        return _collider.bounds.Contains(handtip);
    }

    public void CalcTargetPosition(Transform target, Transform hand)
    {
        Transform initParent = hand.parent;
        hand.parent = transform.parent;
        Vector3 lp = hand.localPosition;

        target.localPosition = new Vector3(lp.x, lp.y, -lp.z);
        hand.parent = initParent;
    }
}
