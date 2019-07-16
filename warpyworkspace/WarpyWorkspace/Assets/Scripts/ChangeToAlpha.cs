using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeToAlpha : MonoBehaviour {

    public Material original;
    public Material transparent;

    private MeshCollider _collider;
    private Renderer _renderer;

    public List<Transform> AlphaTriggerObjects;

    void Start () {
        _collider = GetComponent<MeshCollider>();
        _renderer = GetComponent<Renderer>();
	}
	
	void LateUpdate () {

        _renderer.material = isSomeoneInside() ? transparent : original;

	}

    bool isSomeoneInside()
    {
        for (int i = 0; i < AlphaTriggerObjects.Count; i++)
        {
            if (_collider.bounds.Contains(AlphaTriggerObjects[i].position))
                return true;
        }
        return false;
    }
}
