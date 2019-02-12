using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Workspace : MonoBehaviour {

    private bool __initialized__ = false;


    public void __init__ () {

        __initialized__ = true;
	}
	
	void Update () {
        if (!__initialized__) return;


		
	}
}
