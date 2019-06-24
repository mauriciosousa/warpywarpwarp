using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonFeedback : MonoBehaviour {

    public Material pressedMaterial;
    private Material _normalMaterial;

    private Renderer _renderer;

	void Start () {

        _renderer = GetComponent<Renderer>();
        _normalMaterial = _renderer.material;

	}

	void Update () {

        _renderer.material = _normalMaterial;
        if (Input.GetKey(KeyCode.KeypadEnter))
        {
            _renderer.material = pressedMaterial;
        }

	}
}
