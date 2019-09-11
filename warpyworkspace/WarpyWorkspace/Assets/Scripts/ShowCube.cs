using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowCube : MonoBehaviour {

    public AlteredTelepresenceMain main;

	void Start () {

        gameObject.active = main.calibrationMode;
		
	}	

	void Update () {
		
	}
}
