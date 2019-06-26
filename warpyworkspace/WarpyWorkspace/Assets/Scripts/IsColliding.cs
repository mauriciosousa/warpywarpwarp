using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsColliding : MonoBehaviour {

    public bool COLLIDING = false;
    private string _tag = "Player"; // Must be defined in the Editor

	void Start () {
        COLLIDING = false;
        gameObject.tag = _tag;
	}
	
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == _tag)
        {
            COLLIDING = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == _tag)
        {
            COLLIDING = false;
        }
    }
}
