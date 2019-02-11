using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Workspace))]
public class WorkspaceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Workspace myScript = (Workspace)target;
        if (GUILayout.Button("Show/Hide ABCDs"))
        {
            myScript.renderABCDs(!myScript.ShowABCDs);
        }
    }
}
