using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssemblerCursor : MonoBehaviour {

    public Transform cursor;

    public Transform leftHand;
    public Transform rightHand;

    public Collider interactionZone;

	void Start () {
        cursor.gameObject.SetActive(true);
	}
	
	void Update () {

        if (Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.Joystick1Button1))
        {
            bool isLeftHand = interactionZone.bounds.Contains(leftHand.position);
            bool isRightHand = interactionZone.bounds.Contains(rightHand.position);

            Transform newTrans = null;
            if (isLeftHand && !isRightHand) newTrans = leftHand;
            else if (!isLeftHand && isRightHand) newTrans = rightHand;

            if (newTrans != null && cursor.gameObject.active)
            {
                cursor.position = newTrans.position;
            }
        }

		
	}
}
