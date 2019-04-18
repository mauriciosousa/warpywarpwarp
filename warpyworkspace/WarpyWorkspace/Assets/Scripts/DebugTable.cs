using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTable : MonoBehaviour {

    public Transform TL;
    public Transform TR;
    public Transform BL;
    public Transform BR;
	
	void Update () {


        Debug.DrawLine(TL.position, TR.position, Color.red);
        Debug.DrawLine(BL.position, BR.position, Color.red);
        Debug.DrawLine(TL.position, BL.position, Color.red);
        Debug.DrawLine(TR.position, BR.position, Color.red);


    }

}
