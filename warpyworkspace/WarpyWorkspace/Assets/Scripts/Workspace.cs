using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Workspace : MonoBehaviour {

    public Transform[] ABCDs;

    public float workspaceLength;
    public float workspaceDepth;
    public float workspaceHeight;

    public bool ShowABCDs;

    public Material workspaceMaterial;

    void Start () {
        renderABCDs(ShowABCDs);
        _distributeABCDs();
        _createBottomMesh();
	}

    void Update () {
		
	}

    private void _distributeABCDs()
    {
        ABCDs[0].localPosition = new Vector3(-workspaceLength / 2, workspaceHeight, -workspaceDepth / 2);
        ABCDs[1].localPosition = new Vector3(workspaceLength / 2, workspaceHeight, -workspaceDepth / 2);
        ABCDs[2].localPosition = new Vector3(workspaceLength / 2, workspaceHeight, workspaceDepth / 2);
        ABCDs[3].localPosition = new Vector3(-workspaceLength / 2, workspaceHeight, workspaceDepth / 2);
        
        ABCDs[4].localPosition = new Vector3(-workspaceLength / 2, 0, -workspaceDepth / 2);
        ABCDs[5].localPosition = new Vector3(workspaceLength / 2, 0, -workspaceDepth / 2);
        ABCDs[6].localPosition = new Vector3(workspaceLength / 2, 0, workspaceDepth / 2);
        ABCDs[7].localPosition = new Vector3(-workspaceLength / 2, 0, workspaceDepth / 2);
    }

    public void renderABCDs(bool showABCDs)
    {
        foreach (Transform t in ABCDs)
        {
            t.GetComponent<Renderer>().enabled = showABCDs;
        }
        ShowABCDs = showABCDs;
    }

    private void _createBottomMesh()
    {
        MeshFilter meshFilter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        Mesh mesh = new Mesh()
        {
            name = "WorkspaceMesh",
            vertices = new Vector3[] { ABCDs[4].position, ABCDs[5].position, ABCDs[6].position, ABCDs[7].position },
            triangles = new int[]
            {
                0, 2, 1,
                0, 3, 2
            }
        };

        Vector2[] uv = new Vector2[mesh.vertices.Length];
        for (int i = 0; i < uv.Length; i++)
        {
            uv[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].z);
        }
        mesh.uv = uv;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;

        MeshRenderer renderer = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        renderer.material = workspaceMaterial;
        //MeshCollider collider = gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
    }
}
