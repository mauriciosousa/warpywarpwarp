using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRHeadController : MonoBehaviour {

    public BodiesManager _bodies;
    public Transform localHumanHead;

    //private AdaptiveDoubleExponentialFilterVector3 headPosition;
    private KalmanFilterVector3 headPosition;

    void Start()
    {
        //headPosition = new AdaptiveDoubleExponentialFilterVector3();
        headPosition = new KalmanFilterVector3();
    }

    void Update()
    {

        //bool canApplyHeadPosition;
        //Vector3 headposition = _bodies.getHeadPosition(out canApplyHeadPosition);
        //if (canApplyHeadPosition)
        //{
        //    this.transform.position = headposition;
        //}
        //else
        //{
        //    this.transform.position = new Vector3(0, 1.8f, 0);
        //}

        if (_bodies.human != null)
        {
            headPosition.Value = localHumanHead.position;
            this.transform.position = headPosition.Value;
        }
        else
        {
            this.transform.position = new Vector3(0, 1.8f, 0);
        }
    }
}
