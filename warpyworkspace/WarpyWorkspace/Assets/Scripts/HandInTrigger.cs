using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandInTrigger : MonoBehaviour {


    public bool local;


	// Use this for initialization
	void Start () {
		
	}
	

    private void OnTriggerStay(Collider other)
    {
        if(other.name == "BallPark")
        {
            print("IN THE BALLPARK " + local);
        }
        if(other.name == "WarpingZone")
        {
            print("IN THE WARPING ZONE " + local);
        }

    }
}
