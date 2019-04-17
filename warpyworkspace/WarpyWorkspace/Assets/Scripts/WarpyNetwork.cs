using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpyNetwork : MonoBehaviour {

    private int _port;
    private string _address;
    private SetupLocation _location;
    private string ConfigFile;

    public NetworkPeerType networkPeerType;
    private NetworkView _networkView;

    private Workspace workspace;

    public bool Connected
    {
        get
        {
            return Network.peerType != NetworkPeerType.Disconnected;
        }
    }

    void Awake()
    {
       ConfigFile = Application.dataPath + "/config.txt";
        _networkView = GetComponent<NetworkView>();
        workspace = GetComponent<Workspace>();
    }

    internal void __init__()
    {
        _location = (SetupLocation)Enum.Parse(enumType: typeof(SetupLocation), value: ConfigProperties.load(ConfigFile, "setup.type"));
        _port = int.Parse(ConfigProperties.load(ConfigFile, "rpc.port"));
        _address = ConfigProperties.load(ConfigFile, "left.setup.address");

        if (_location == SetupLocation.LEFT)
        {
            Network.InitializeServer(4, _port, false);
            Debug.Log("!!! starting server");
        }
        else
        {
            Network.Connect(_address, _port);
        }
    }

    private void Update()
    {
        networkPeerType = Network.peerType;
    }

    void OnPlayerConnected(NetworkPlayer player)
    {
        Debug.Log("New Player @ " + player.ipAddress);
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Debug.Log("Player Disconnected @ " + player.ipAddress);
    }

    [RPC]
    void RPC_ButtonPressed()
    {
        workspace.buttonPressed();
    }


}
