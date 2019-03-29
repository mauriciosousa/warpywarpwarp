using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKHuman : MonoBehaviour {

    public bool doWarp = false;
    [Space(10)]

    [Header("Left Arm")]
    public Transform leftShoulder;
    public Transform leftElbow;
    public Transform leftWrist;
    [Space(10)]

    [Header("Right Arm")]
    public Transform rightShoulder;
    public Transform rightElbow;
    public Transform rightWrist;
    [Space(10)]

    public Transform ElbowTarget;
    public Transform Target;
    [Space(20)]
    [Header("IK Stuff")]
    public Vector3 uppperArm_OffsetRotation;
    public Vector3 leftElbow_OffsetRotation;
    public Vector3 leftWrist_OffsetRotation;
    [Space(10)]
    public bool leftWristMatchesTargetRotation = true;
    [Space(10)]
    public bool debug;

    float angle;
    float leftShoulder_Length;
    float leftElbow_Length;
    float arm_Length;
    float TargetDistance;
    float adyacent;


    void Start () {
		
	}
	
	public void doWarpNow () {
        if (!doWarp) return;

        /*
        // Elbow Target First
        Vector3 A = leftWrist.position;
        Vector3 B = leftElbow.position;
        Vector3 C = leftShoulder.position;

        Vector3 BA = A - B;
        Vector3 BC = C - B;
        Vector3 dir = -(BA + BC).normalized;

        ElbowTarget.position = B + 0.1f * (dir);
        */

        // Then IK 
        if (leftShoulder != null && leftElbow != null && leftWrist != null && ElbowTarget != null && Target != null)
        {
            leftShoulder.LookAt(Target, ElbowTarget.position - leftShoulder.position);
            leftShoulder.Rotate(uppperArm_OffsetRotation);

            Vector3 cross = Vector3.Cross(ElbowTarget.position - leftShoulder.position, leftElbow.position - leftShoulder.position);

            leftShoulder_Length = Vector3.Distance(leftShoulder.position, leftElbow.position);
            leftElbow_Length = Vector3.Distance(leftElbow.position, leftWrist.position);
            arm_Length = leftShoulder_Length + leftElbow_Length;
            TargetDistance = Vector3.Distance(leftShoulder.position, Target.position);
            TargetDistance = Mathf.Min(TargetDistance, arm_Length - arm_Length * 0.001f);

            adyacent = ((leftShoulder_Length * leftShoulder_Length) - (leftElbow_Length * leftElbow_Length) + (TargetDistance * TargetDistance)) / (2 * TargetDistance);

            angle = Mathf.Acos(adyacent / leftShoulder_Length) * Mathf.Rad2Deg;

            leftShoulder.RotateAround(leftShoulder.position, cross, -angle);

            leftElbow.LookAt(Target, cross);
            leftElbow.Rotate(leftElbow_OffsetRotation);

            if (leftWristMatchesTargetRotation)
            {
                leftWrist.rotation = Target.rotation;
                leftWrist.Rotate(leftWrist_OffsetRotation);
            }

            if (debug)
            {
                if (leftElbow != null && ElbowTarget != null)
                {
                    Debug.DrawLine(leftElbow.position, ElbowTarget.position, Color.blue);
                }

                Debug.DrawLine(leftShoulder.position, leftShoulder.position + cross*10, Color.red);
            }

        }
    }
}
