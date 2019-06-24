using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRHeadController : MonoBehaviour {

    public BodiesManager _bodies;
    public Transform localHumanHead;

    void Start()
    {

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
            this.transform.position = localHumanHead.position;
        }
        else
        {
            this.transform.position = new Vector3(0, 1.8f, 0);
        }
    }
}
