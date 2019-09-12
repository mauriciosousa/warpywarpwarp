using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorFromCloud : MonoBehaviour {

    public Transform referencePoint;
    public Transform cursorPosition;
    public BoxCollider workspaceCollider;

    public bool contains = false;

    public MeshFilter cloud0;
    public MeshFilter cloud1;
    public MeshFilter cloud2;
    public MeshFilter cloud3;
    public MeshFilter cloud4;

    // Use this for initialization
    void Start () {
        cloud0 = null;
        cloud1 = null;
        cloud2 = null;
        cloud3 = null;
        cloud4 = null;
    }

    // Update is called once per frame
    public int length = 0;
	void Update () {

        

        if (cloud0 == null) cloud0 = _getCloud("cloud0");
        if (cloud1 == null) cloud1 = _getCloud("cloud1");
        if (cloud2 == null) cloud2 = _getCloud("cloud2");
        if (cloud3 == null) cloud3 = _getCloud("cloud3");
        if (cloud4 == null) cloud4 = _getCloud("cloud4");


        MeshFilter cloud = cloud0;


        if (cloud != null)
        {
            //length = cloud.mesh.vertices.Length;
            for (int i = 0; i < cloud.mesh.vertices.Length; i++)
            {
                Vector3 p = cloud.transform.localToWorldMatrix.MultiplyPoint3x4(cloud.mesh.vertices[i]);
                contains = workspaceCollider.bounds.Contains(p);
                if (contains)
                {
                    Debug.Log(p);
                    cursorPosition.position = p;
                }
            }
        }





        return;

        for(int i = 0; i < transform.childCount; i++)
        {

            Transform t = transform.GetChild(i);
            if (t.name != "LocalHuman")
            {
                for (int j = 0; j < t.childCount; j++)
                {
                    Transform u = t.GetChild(j);
                    if (u.name == t.name)
                    {
                        float smallestD = float.PositiveInfinity;
                        Vector3 closestP = Vector3.zero;

                        for (int k = 0; k < u.childCount; k++)
                        {
                            Transform v = u.GetChild(k);
                            Vector3[] vertices = v.GetComponent<MeshFilter>().mesh.vertices;

                            for (int l = 0; l < vertices.Length; l++)
                            {

                                if (vertices[l] != Vector3.zero)
                                {
                                    Vector3 p = v.localToWorldMatrix.MultiplyPoint3x4(vertices[l]);
                                    //if (workspaceCollider.bounds.Contains(p))
                                    {
                                        float d = Vector3.Distance(p, referencePoint.position);
                                        if (d < smallestD)
                                        {
                                            smallestD = d;
                                            closestP = p;
                                        }
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

    private MeshFilter _getCloud(string v)
    {
        //BFS
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(transform);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.name == v)
                return c.GetComponent<MeshFilter>();
            foreach (Transform t in c)
                queue.Enqueue(t);
        }
        return null;
    }
}
