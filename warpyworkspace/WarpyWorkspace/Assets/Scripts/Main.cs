using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum SetupLocation
{
    LEFT,
    RIGHT
}

public enum Formation
{
    SIDE_TO_SIDE,
    FACE_TO_FACE
}


public class Main : MonoBehaviour {

    public string ConfigFile;
    public SetupLocation setupLocation;
    public Formation formation;
    private string _localPrefix;
    private string _remotePrefix;

    public BodiesManager localBodiesManager;
    public UdpBodiesListener localUdpListener;
    public BodiesManager remoteBodiesManager;
    public UdpBodiesListener remoteUdpListener;

    private SurfaceRectangle _localSurface;
    private SurfaceRectangle _remoteSurface;
    private bool _everythingIsConfigured = false;

    public Transform localOrigin;
    public Transform remoteOrigin;

    public Workspace workspace;

	void Awake () {

        Application.runInBackground = true;

        ConfigFile = Application.dataPath + "/config.txt";
        ConfigProperties.save(ConfigFile, "last.run", DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));

        setupLocation = (SetupLocation) Enum.Parse(enumType: typeof(SetupLocation), value: ConfigProperties.load(ConfigFile, "setup.type"));
        if (setupLocation == SetupLocation.LEFT)
        {
            _localPrefix = "left"; _remotePrefix = "right";
        }
        else
        {
            _localPrefix = "right"; _remotePrefix = "left";
        }

        formation = (Formation)Enum.Parse(enumType: typeof(Formation), value: ConfigProperties.load(ConfigFile, "start.formation"));

        string localTrackerAddress = ConfigProperties.load(ConfigFile, _localPrefix + ".setup.address");
        int localTrackerBroadcastPort = int.Parse(ConfigProperties.load(ConfigFile, _localPrefix + ".tracker.broadcast.port")); 
        int localTrackerSurfaceRequestPort = int.Parse(ConfigProperties.load(ConfigFile, _localPrefix + ".tracker.surface.request.port"));
        int localTrackerSurfaceListenerPort = int.Parse(ConfigProperties.load(ConfigFile, _localPrefix + ".tracker.surface.listener.port"));

        string remoteTrackerAddress = ConfigProperties.load(ConfigFile, _remotePrefix + ".setup.address");
        int remoteTrackerBroadcastPort = int.Parse(ConfigProperties.load(ConfigFile, _remotePrefix + ".tracker.broadcast.port"));
        int remoteTrackerSurfaceRequestPort = int.Parse(ConfigProperties.load(ConfigFile, _remotePrefix + ".tracker.surface.request.port"));
        int remoteTrackerSurfaceListenerPort = int.Parse(ConfigProperties.load(ConfigFile, _remotePrefix + ".tracker.surface.listener.port"));


        localUdpListener.startListening(localTrackerBroadcastPort);
        remoteUdpListener.startListening(remoteTrackerBroadcastPort);
        GetComponent<CreepyTrackerSurfaceRequestListener>().StartReceive(localTrackerSurfaceListenerPort, remoteTrackerSurfaceListenerPort);
        GetComponent<CreepyTrackerSurfaceRequest>().Request(localTrackerAddress, localTrackerSurfaceRequestPort, localTrackerSurfaceListenerPort, 
                                                            remoteTrackerAddress, remoteTrackerSurfaceRequestPort, remoteTrackerSurfaceListenerPort);

        


    }
	
	void Update () {

        if (!_everythingIsConfigured && _localSurface != null && _remoteSurface != null)
        {
            _configWorkspaces();
            _everythingIsConfigured = true;
        }



        _debugWorkspaces(_localSurface, Color.red);
        _debugWorkspaces(_remoteSurface, Color.green);
    }

    internal void setLocalSurface(SurfaceRectangle s)
    {
        Debug.Log("LOCAL SURFACE " + s.ToString());
        _localSurface = s;
    }

    internal void setRemoteSurface(SurfaceRectangle s)
    {
        Debug.Log("REMOTE SURFACE " + s.ToString());
        _remoteSurface = s;
    }

    private void _configWorkspaces()
    {
        Debug.Log("NOW I CAN DO ALL THE STUFFS!!");

        //ShowSomeCubes(localOrigin.gameObject, _localSurface);
        //ShowSomeCubes(remoteOrigin.gameObject, _remoteSurface);

        _deploySensors(_localSurface.sensors, localOrigin);
        _deploySensors(_remoteSurface.sensors, remoteOrigin);

        GameObject localWorkspaceCenter = _calculateCenter(localOrigin, _localSurface);
        localWorkspaceCenter.name = "localWorkspaceCenter";
        GameObject remoteWorkspaceCenter = _calculateCenter(remoteOrigin, _remoteSurface);
        remoteWorkspaceCenter.name = "remoteWorkspaceCenter";


        remoteWorkspaceCenter.transform.position = localWorkspaceCenter.transform.position;
        remoteWorkspaceCenter.transform.rotation = localWorkspaceCenter.transform.rotation;
        if (formation == Formation.FACE_TO_FACE)
        {
            remoteWorkspaceCenter.transform.forward = -localWorkspaceCenter.transform.forward;
        }

        workspace.transform.position = localWorkspaceCenter.transform.position;
        workspace.transform.rotation = localWorkspaceCenter.transform.rotation;

        
        //Debug.Log("[" + this.ToString() + "] Workspaces configured");
    }

    private GameObject _calculateCenter(Transform worldCenter, SurfaceRectangle surface)
    {
        GameObject workspaceCenter = new GameObject();
        workspaceCenter.transform.position = (surface.SurfaceBottomLeft + surface.SurfaceTopRight) / 2;

        worldCenter.parent = workspaceCenter.transform;
        return workspaceCenter;
    }

    private void ShowSomeCubes(GameObject centerGO, SurfaceRectangle surface)
    {
        GameObject center = LittleCube(centerGO.transform, centerGO.name);
        GameObject BL = LittleCube(localOrigin, "BL");
        BL.transform.localPosition = surface.SurfaceBottomLeft;
        GameObject BR = LittleCube(localOrigin, "BR");
        BR.transform.localPosition = surface.SurfaceBottomRight;
        GameObject TL = LittleCube(localOrigin, "TL");
        TL.transform.localPosition = surface.SurfaceTopLeft;
        GameObject TR = LittleCube(localOrigin, "TR");
        TR.transform.localPosition = surface.SurfaceTopRight;
    }

    private void _deploySensors(Sensor[] sensors, Transform parent)
    {
        foreach (Sensor s in sensors)
        {
            GameObject sensor = new GameObject
            {
                name = s.id
            };
            sensor.transform.parent = parent;
            sensor.transform.localPosition = s.position;
            sensor.transform.localRotation = s.rotation;
            GameObject cube = LittleCube(sensor.transform, sensor.name);
        }

    }

    public GameObject LittleCube(Transform parent, string name)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        cube.transform.parent = parent;
        cube.transform.localPosition = Vector3.zero;
        cube.transform.rotation = Quaternion.identity;
        cube.name = name;
        return cube;
    }

    private void _debugWorkspaces(SurfaceRectangle surface, Color color)
    {
        if (surface != null)
        {
            Debug.DrawLine(surface.SurfaceBottomLeft, surface.SurfaceBottomRight, color);
            Debug.DrawLine(surface.SurfaceBottomRight, surface.SurfaceTopRight, color);
            Debug.DrawLine(surface.SurfaceTopRight, surface.SurfaceTopLeft, color);
            Debug.DrawLine(surface.SurfaceTopLeft, surface.SurfaceBottomLeft, color);
        }
    }
}
