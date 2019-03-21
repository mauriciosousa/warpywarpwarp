using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKWarpInit : MonoBehaviour {

    public BodiesManager _bodies;
    public UdpBodiesListener _bodiesListener;

    public TrackerMesh _ravatar;


    public int CT_trackerBroadcastPort;
    public int RAV_trackerPort;
    public int RAV_listenPort;


    void Awake () {
        Application.runInBackground = true;

        _bodiesListener.startListening(CT_trackerBroadcastPort);

        _ravatar.Init(RAV_trackerPort, RAV_listenPort, transform);
    }

    void Update () {
		
	}
}
