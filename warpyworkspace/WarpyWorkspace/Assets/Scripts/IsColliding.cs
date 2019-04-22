using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsColliding : MonoBehaviour {

    public bool COLLIDING = false;

	void Start () {
        COLLIDING = false;
	}
	
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            //Debug.Log("ENTER");
            COLLIDING = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            //Debug.Log("EXIT");
            COLLIDING = false;
        }
    }
}
