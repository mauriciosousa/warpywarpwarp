using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProxemicDistances
{
    Intimate,
    Personal,
    Social,
    Public
}

public class ProxemicsAnalysis : MonoBehaviour {

    public IsColliding localHumanCollider;
    public bool humansColliding;

    public Transform localHumanSpineJoint;
    public Transform remoteHumanSpineJoint;

    public float distance = 0f;

    public ProxemicDistances proxemics = ProxemicDistances.Public;

    void Start () {
		
	}
	
	
	void Update () {
        humansColliding = localHumanCollider.COLLIDING;

        Vector3 localPosition = new Vector3(localHumanSpineJoint.position.x, 0, localHumanSpineJoint.position.z);
        Vector3 remotePosition = new Vector3(remoteHumanSpineJoint.position.x, 0, remoteHumanSpineJoint.position.z);

        distance = Vector3.Distance(localPosition, remotePosition);

        if (distance <= 0.46f) proxemics = ProxemicDistances.Intimate;
        else if (distance > 0.46f && distance <= 1.2f) proxemics = ProxemicDistances.Personal;
        else if (distance > 1.2f && distance <= 3.7) proxemics = ProxemicDistances.Social;
        else proxemics = ProxemicDistances.Public;

    }
}
