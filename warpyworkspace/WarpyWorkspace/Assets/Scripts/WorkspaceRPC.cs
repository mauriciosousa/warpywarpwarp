using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkspaceRPC : MonoBehaviour {

    private int _port;
    private string _address;
    private ParticipantsType _participant;

    private NetworkView _networkView;
    private NetworkPeerType _networkPeerType;

    public int Task = 0;

    private string serverLabel = "";

    public bool Connected
    {
        get
        {
            return Network.peerType != NetworkPeerType.Disconnected;
        }
    }

    void Start () {
        __init__(88888, "127.0.0.1", ParticipantsType.ASSEMBLER);
	}

    public void __init__(int port, string address, ParticipantsType participant)
    {
        _networkPeerType = participant == ParticipantsType.NONE ? NetworkPeerType.Server : NetworkPeerType.Client;
        _port = port;
        _address = address;
        _participant = participant;

        _networkView = GetComponent<NetworkView>();

        if (_networkPeerType == NetworkPeerType.Server)
        {
            Network.InitializeServer(3, _port, false);
            serverLabel = "Server";
        }
        else
        {
            Network.Connect(_address, _port);
        }
    }
	
	void Update () {
        
		
	}

    [RPC]
    void RPC_start(int task)
    {
        if (_networkPeerType == NetworkPeerType.Server)
        {
            Task = task;
            serverLabel = "Init Task " + Task;
        }
    }

    [RPC]
    void RPC_end(int task)
    {
        if (_networkPeerType == NetworkPeerType.Server)
        {
            serverLabel = "End Task " + Task;
        }
    }

    void OnPlayerConnected(NetworkPlayer player)
    {
        Debug.Log("New Player @ " + player.ipAddress);
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
       Debug.Log("Player Disconnected @ " + player.ipAddress);
    }

    void OnGUI()
    {
        if (_networkPeerType == NetworkPeerType.Server)
        {
            GUI.Label(new Rect(10, 10, 500, 50), serverLabel);
        }
    }
}
