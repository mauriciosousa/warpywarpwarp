using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTable : MonoBehaviour {

    public Transform TL;
    public Transform TR;
    public Transform BL;
    public Transform BR;

    private Color color;

    private void Start()
    {
        color = transform.parent.name.Contains("local") ? Color.red : Color.cyan;
    }

    void Update () {

        Vector3 tl = TL.position + TL.forward * 0.1f;
        Vector3 tr = TR.position + TR.forward * 0.1f;
        Vector3 bl = BL.position + BL.forward * 0.1f;
        Vector3 br = BR.position + BR.forward * 0.1f;


        Debug.DrawLine(tl, tr, color);
        Debug.DrawLine(bl, br, color);
        Debug.DrawLine(tl, bl, color);
        Debug.DrawLine(tr, br, color);

        Debug.DrawLine(0.5f * (tl + bl), 0.5f * (tr + br), color);
        Debug.DrawLine(0.5f * (tl + tr), 0.5f * (bl + br), color);
    }

}
