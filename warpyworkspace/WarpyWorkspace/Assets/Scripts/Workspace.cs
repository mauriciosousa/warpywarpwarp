using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ParticipantsType
{
    ASSEMBLER, 
    REMOTE_HELPER,
    NONE
}

public class Workspace : MonoBehaviour {

    public int numberOfTasks = 5;
    private int _task = 0;

    void Start()
    {

    }
	
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _endPreviousTask();
            _startNextTask();
        }
	}

    private void _startNextTask()
    {
        if (_task == numberOfTasks) return;

        _task += 1;
        Debug.Log("Starting Task " + _task);        
    }

    private void _endPreviousTask()
    {
        if (_task == 0) return;

        if (_task == numberOfTasks)
        {
            Debug.Log("ACABOU");
            return;
        }
        else
        {
            Debug.Log("Ending Task " + _task);
        }
    }
}
