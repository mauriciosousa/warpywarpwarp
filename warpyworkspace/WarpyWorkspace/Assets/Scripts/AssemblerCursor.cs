using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssemblerCursor : MonoBehaviour {

    public Transform cursor;
    public Transform rope;

    public Vector2 delta;

    public Transform leftHandTip;
    public Transform leftHand;

    public Transform rightHandTip;
    public Transform rightHand;

    public Collider interactionZone;

    public bool canDo = false;

    private OneEuroFilter<Vector3> filter;

    [Space(5)]
    [Header("OneEuroFilter Params:")]
    public float freq = 100.0f;
    public float mincutoff = 1.0f;
    public float beta = 0.001f;
    public float dcutoff = 1.0f;

    void Start () {
        filter = new OneEuroFilter<Vector3>(freq, mincutoff, beta, dcutoff);
    }
	
	void Update () {

        filter.UpdateParams(freq, mincutoff, beta, dcutoff);

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
                Vector3 dir;// = (tip.position - hand.position).normalized;

                dir = interactionZone.transform.forward * delta.x + interactionZone.transform.up * delta.y;

                this.transform.position = filter.Filter(hand.position + dir);// filter.Filter(tip.position + delta*dir);
                rope.gameObject.SetActive(true);
                rope.position = (hand.position + this.transform.position) * 0.5f;
                rope.up = hand.position - this.transform.position;
                rope.localScale = new Vector3(rope.localScale.x, Vector3.Distance(hand.position, this.transform.position) * 0.5f, rope.localScale.z);
            }
        }
        else
            rope.gameObject.SetActive(false);
	}
}
