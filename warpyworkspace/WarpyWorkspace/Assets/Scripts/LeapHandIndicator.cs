using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeapHandIndicator : MonoBehaviour {


    public Transform wrist;

    void Start () {
		
	}
	

	void Update () {

        if (wrist)
        {
            this.transform.position = wrist.position;
            this.transform.rotation = wrist.rotation;
        }
		
	}
}
