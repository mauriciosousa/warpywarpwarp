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

    public TrackerMesh ravatarManagerTracker;

    public Table LocalTable;
    public Table RemoteTable;

    private SurfaceRectangle _localSurface;
    private SurfaceRectangle _remoteSurface;
    private bool _everythingIsConfigured = false;

    public Transform localOrigin;
    public Transform remoteOrigin;

    public GameObject localWorkspaceCenter;
    public GameObject remoteWorkspaceCenter;

    public Workspace workspace;

    public Dictionary<string, GameObject> _sensors;

    private string localTrackerAddress;
    private string remoteTrackerAddress;
    private int localTrackerListenPort;
    private int remoteTrackerListenPort;
    private int localTrackerSurfaceRequestPort;
    private int remoteTrackerSurfaceRequestPort;
    private int localTrackerSurfaceListenerPort;
    private int remoteTrackerSurfaceListenerPort;


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


        localTrackerListenPort = int.Parse(ConfigProperties.load(ConfigFile, _localPrefix + ".tracker.listen.port"));
        remoteTrackerListenPort = int.Parse(ConfigProperties.load(ConfigFile, _remotePrefix + ".tracker.listen.port"));



        localTrackerAddress = ConfigProperties.load(ConfigFile, _localPrefix + ".setup.address");
        int localTrackerBroadcastPort = int.Parse(ConfigProperties.load(ConfigFile, _localPrefix + ".tracker.broadcast.port")); 
        localTrackerSurfaceRequestPort = int.Parse(ConfigProperties.load(ConfigFile, _localPrefix + ".tracker.surface.request.port"));
        localTrackerSurfaceListenerPort = int.Parse(ConfigProperties.load(ConfigFile, _localPrefix + ".tracker.surface.listener.port"));
        int localAvatarListenPort = int.Parse(ConfigProperties.load(ConfigFile, _localPrefix + ".client.avatar.listen.port"));

        remoteTrackerAddress = ConfigProperties.load(ConfigFile, _remotePrefix + ".setup.address");
        int remoteTrackerBroadcastPort = int.Parse(ConfigProperties.load(ConfigFile, _remotePrefix + ".tracker.broadcast.port"));
        remoteTrackerSurfaceRequestPort = int.Parse(ConfigProperties.load(ConfigFile, _remotePrefix + ".tracker.surface.request.port"));
        remoteTrackerSurfaceListenerPort = int.Parse(ConfigProperties.load(ConfigFile, _remotePrefix + ".tracker.surface.listener.port"));
        int remoteAvatarListenPort = int.Parse(ConfigProperties.load(ConfigFile, _remotePrefix + ".client.avatar.listen.port"));

        GetComponent<CreepyTrackerSurfaceRequestListener>().StartReceive(localTrackerSurfaceListenerPort, remoteTrackerSurfaceListenerPort);
        localUdpListener.startListening(localTrackerBroadcastPort);
        remoteUdpListener.startListening(remoteTrackerBroadcastPort);

        _sensors = new Dictionary<string, GameObject>();
        _surfaceRequest();

    }

    internal void setupSensors(GameObject[] sensors)
    {
        if (sensors.Length > 0 && _sensors.Values.Count > 0)
        {
            foreach (GameObject g in sensors)
            {
                g.transform.parent = _sensors[g.name].transform;
                g.transform.localPosition = Vector3.zero;
                g.transform.localRotation = Quaternion.identity;
            }
        }
        else
        {
            Debug.LogError("lokl");
        }
    }

    private void _surfaceRequest()
    {
        if (_localSurface == null || _remoteSurface == null)
        {
            Debug.Log("[" + this.ToString() + "] Surface Request Sent...");
            GetComponent<CreepyTrackerSurfaceRequest>().Request(localTrackerListenPort, localTrackerSurfaceListenerPort, remoteTrackerListenPort, remoteTrackerSurfaceListenerPort);
        }
        else
        {
            Debug.Log("[" + this.ToString() + "] WE ALREADY HAVE ALL SURFACES");

        }
    }

    void Update () {

        if (!_everythingIsConfigured && _localSurface != null && _remoteSurface != null)
        {
            ravatarManagerTracker.Init(
            remoteTrackerListenPort,
            int.Parse(ConfigProperties.load(ConfigFile, _localPrefix + ".client.avatar.listen.port"))
            );

            _configWorkspaces();
            _everythingIsConfigured = true;
        }


        if (Input.GetKeyDown(KeyCode.R))
        {
            _surfaceRequest();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            try
            {
                Camera.main.transform.position = localBodiesManager.human.body.Joints[BodyJointType.head];
                Camera.main.transform.LookAt(localWorkspaceCenter.transform.position);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

    }

    internal void setLocalSurface(SurfaceRectangle s)
    {
        Debug.Log("] LOCAL SURFACE " + s.ToString());
        _localSurface = s;
    }

    internal void setRemoteSurface(SurfaceRectangle s)
    {
        Debug.Log("] REMOTE SURFACE " + s.ToString());
        _remoteSurface = s;
    }

    private void _configWorkspaces()
    {
        Debug.Log("NOW I CAN DO ALL THE STUFFS!!");

        LocalTable.set(_localSurface);
        RemoteTable.set(_remoteSurface);

        _deploySensors(_localSurface.sensors, localOrigin);
        _deploySensors(_remoteSurface.sensors, remoteOrigin);

        localWorkspaceCenter = new GameObject();
        localWorkspaceCenter.name = "localWorkspaceCenter";
        localWorkspaceCenter.transform.position = LocalTable.transform.position;
        localWorkspaceCenter.transform.rotation = LocalTable.transform.rotation;
        localOrigin.parent = localWorkspaceCenter.transform;


        remoteWorkspaceCenter = new GameObject();
        remoteWorkspaceCenter.name = "remoteWorkspaceCenter";
        remoteWorkspaceCenter.transform.position = RemoteTable.transform.position;
        remoteWorkspaceCenter.transform.rotation = RemoteTable.transform.rotation;
        remoteOrigin.parent = remoteWorkspaceCenter.transform;

        remoteWorkspaceCenter.transform.position = localWorkspaceCenter.transform.position;
        remoteWorkspaceCenter.transform.rotation = localWorkspaceCenter.transform.rotation;

        if (formation == Formation.FACE_TO_FACE)
        {

            remoteWorkspaceCenter.transform.rotation = Quaternion.LookRotation(-localWorkspaceCenter.transform.forward, localWorkspaceCenter.transform.up);
        }


        workspace.gameObject.SetActive(true);
        workspace.transform.position = localWorkspaceCenter.transform.position;
        workspace.transform.rotation = localWorkspaceCenter.transform.rotation;
        workspace.__init__();

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
            GameObject cube = LittleCube(sensor.transform, sensor.name + "cube");
            _sensors[s.id] = sensor;
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
}
