using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssemblerCursor : MonoBehaviour {

    public Transform cursor;

    public float delta;

    public Transform leftHandTip;
    public Transform leftHand;

    public Transform rightHandTip;
    public Transform rightHand;

    public Collider interactionZone;

    public bool canDo = false;

	void Start () {
	}
	
	void Update () {

        cursor.gameObject.GetComponent<Renderer>().enabled = canDo;

        if (!canDo) return;

        if (Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.Joystick1Button1))
        {
            bool isLeftHand = interactionZone.bounds.Contains(leftHandTip.position);
            bool isRightHand = interactionZone.bounds.Contains(rightHandTip.position);

            Transform hand = null;
            Transform tip = null;
            if (isLeftHand && !isRightHand)
            {
                tip = leftHandTip;
                hand = leftHand;
            }
            else if (!isLeftHand && isRightHand)
            {
                tip = rightHandTip;
                hand = rightHand;
            }

            if (tip != null && cursor.gameObject.active)
            {
                Vector3 dir = (tip.position - hand.position).normalized;

                cursor.position = tip.position + delta*dir;
            }
        }
	}
}
