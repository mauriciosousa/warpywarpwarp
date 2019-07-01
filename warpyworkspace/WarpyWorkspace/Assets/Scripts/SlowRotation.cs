using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowRotation : MonoBehaviour {

    public bool active = false;
    public float rotationSpeed = 1; // degrees per second

	void Start () {
		
	}
	
	void Update () {
        GetComponentInChildren<Renderer>().enabled = active;
        if (active)
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0, Space.World);
        }
	}
}
