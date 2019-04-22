using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCapsule : MonoBehaviour {

    public Transform spineMid;

	void Update () {
        transform.position = spineMid.position;	
	}
}
