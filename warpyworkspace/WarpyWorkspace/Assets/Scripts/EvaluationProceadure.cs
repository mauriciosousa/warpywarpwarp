using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Role
{
    MANIPULATOR,
    INSTRUCTOR
}

public class EvaluationProceadure : MonoBehaviour {

    public GameObject workspace;
    private AlteredTelepresenceNetwork _network;

    private SetupLocation _location;
    private Formation _formation; // = condition
    public Role role;
    private int _leftID;
    private int _rightID;

    public GUIStyle style;

    public AssemblerCursor cursor;

    private int t;
    private int t_intermission = 9;
    private int t_lastOne = 16;

    public int task;
    public int Task
    {
        get
        {
            if (t > 0 && t < t_intermission) return t;
            else if (t == t_intermission || t > t_lastOne || t == 0) return -1;
            else return t - 1;
        }
    }

    private DateTime _startTime;

    private string _resultsFolder;

    private bool _init = false;

	void Start () {
        _resultsFolder = Application.dataPath + Path.DirectorySeparatorChar + "Results";
        if (!Directory.Exists(_resultsFolder))
        {
            Directory.CreateDirectory(_resultsFolder);
        }
        role = _location == SetupLocation.LEFT ? Role.INSTRUCTOR : Role.MANIPULATOR;
        _network = GetComponent<AlteredTelepresenceNetwork>();
	}

    public void Init(SetupLocation location, Formation formation, int leftID, int rightID)
    {
        _location = location;
        _formation = formation;
        _leftID = leftID;
        _rightID = rightID;
    }

    private bool _evaluationStarted = false;
    public void startEvaluation()
    {
        _evaluationStarted = true;
        t = 0;
        print("Starting Evaluation with " + _location + " " + _formation + " " + role);

        workspace.SetActive(true);
    }

    internal void ping()
    {
        Debug.Log("PING");
    }

    void Update () {

        if (Input.GetKeyDown(KeyCode.P))
        {
            _network.ping();
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            _network.buttonPressed(_location.ToString());
        }


        if (!_init) return;  

        task = Task;



        if (task >= 1 && task <= 16 && role == Role.MANIPULATOR)
        {
            _network.syncCursor(cursor.transform.localPosition);
        }
	}

    internal void buttonPressed(string location)
    {
        Debug.Log("BUTTON PRESSED in " + location);

        if (_evaluationStarted)
        {
            role = _getRole();
            if (role == Role.MANIPULATOR)
            {
                _network.moveOn();
            }
        }
    }

    internal void syncCursor(Vector3 p)
    {
        if (role == Role.INSTRUCTOR)
        {
            cursor.transform.localPosition = p;
        }
    }

    public void moveOn()
    {        
        if (t >= 1 && t <= t_lastOne && t != t_intermission)
        {
            _endTask();
        }


        t += 1;
        if (t == t_intermission)
        {
            print("INTERMISSION");
        }
        else if (t > 7)
        {
            print("END");
        }
        else
        {
            _startTask();
            
        }
    }

    private Role _getRole()
    {
        if (_location == SetupLocation.LEFT && t <= 8)
            return Role.INSTRUCTOR;

        return Role.MANIPULATOR;
    }

    private void _startTask()
    {
        print("T" + t);

        if (_getRole() == Role.MANIPULATOR) cursor.canDo = true;

        if (_location == SetupLocation.LEFT)
        {
            _startTime = DateTime.Now;
        }
    }

    private void _endTask()
    {
        cursor.canDo = false;
        if (_location == SetupLocation.LEFT)
        {
            TimeSpan timeSpan = DateTime.Now - _startTime;
            print(timeSpan.TotalMilliseconds.ToString());
        }
    }

    void OnGUI()
    {
        int top = 50;
        int left = 10;
        
        GUI.Label(new Rect(left, top, 100, 35), _getRole().ToString(), style);

        top += 40;
        GUI.Label(new Rect(left, top, 100, 35), "T = " + t, style);
        


    }

    internal void communicateStart()
    {
        _network.startEvaluation();
    }
}

public class ResultsFile
{
    private string _file;
    private string _sep = "$";

    public ResultsFile(string filename)
    {
        _file = filename;

        string header = "";

        header += "Timestamp" + _sep;
        header += "Task" + _sep;
        header += "Test" + _sep;
        header += "Condition" + _sep;
        header += "BallQuadrant" + _sep;
        header += "ErrorDistance" + _sep;
        header += "Time" + _sep;
        header += "SameSpacePercentage" + _sep;
        header += "InstructorBall.x" + _sep;
        header += "InstructorBall.y" + _sep;
        header += "InstructorBall.z" + _sep;
        header += "AssemblerBall.x" + _sep;
        header += "AssemblerBall.y" + _sep;
        header += "AssemblerBall.z";

        _writeLine(header);
        Debug.Log("created: " + filename);
    }

    private void _writeLine(string line)
    {
        File.AppendAllText(_file, line + Environment.NewLine);
    }

    public void writeLine(int task, Test test, Formation condition, BallQuadrant ballQuadrant, float errorDistance, float time, float sameSpacePercentage, Vector3 instructorBall, Vector3 assemblerBall)
    {
        string line = "";

        line += DateTime.Now.ToString("yyyyMMddHHmmss") + _sep;
        line += task + _sep;
        line += test + _sep;
        line += condition + _sep;
        line += ballQuadrant.ToString() + _sep;
        line += errorDistance + _sep;
        line += time + _sep;
        line += sameSpacePercentage + _sep;
        line += instructorBall.x + _sep;
        line += instructorBall.y + _sep;
        line += instructorBall.z + _sep;
        line += assemblerBall.x + _sep;
        line += assemblerBall.y + _sep;
        line += assemblerBall.z + _sep;

        _writeLine(line);
    }
}
