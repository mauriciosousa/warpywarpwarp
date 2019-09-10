using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorFromCloud : MonoBehaviour {

    public Transform referencePoint;
    public Transform cursorPosition;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
        for(int i = 0; i < transform.childCount; i++)
        {
            Transform t = transform.GetChild(i);
            if(t.name != "LocalHuman")
            {
                for(int j = 0; j < t.childCount; j++)
                {
                    Transform u = t.GetChild(j);
                    if(u.name == t.name)
                    {
                        float smallestD = float.PositiveInfinity;
                        Vector3 closestP = Vector3.zero;

                        for (int k = 0; k < u.childCount; k++)
                        {
                            Transform v = u.GetChild(k);
                            Vector3[] vertices = v.GetComponent<MeshFilter>().mesh.vertices;

                            for(int l = 0; l < vertices.Length; l++)
                            {
                                if (vertices[l] != Vector3.zero)
                                {
                                    Vector3 p = v.localToWorldMatrix.MultiplyPoint3x4(vertices[l]);
                                    float d = Vector3.Distance(p, referencePoint.position);
                                    if (d < smallestD)
                                    {
                                        smallestD = d;
                                        closestP = p;
                                    }
                                }
                            }
                        }

                        cursorPosition.position = closestP;

                        break;
                    }
                }
                break;
            }
        }
	}
}
