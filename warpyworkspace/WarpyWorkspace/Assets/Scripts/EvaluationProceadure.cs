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

    public AssemblerCursor cursor;

    private int t;
    private int t_intermission = 4;
    private int t_lastOne = 7;

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
        _network = GetComponent<AlteredTelepresenceNetwork>();
	}

    public void Init(SetupLocation location, Formation formation, int leftID, int rightID)
    {
        _location = location;
        _formation = formation;
        role = _location == SetupLocation.LEFT ? Role.INSTRUCTOR : Role.MANIPULATOR;
        _leftID = leftID;
        _rightID = rightID;
    }

    public void startEvaluation()
    {
        t = 0;
        print("Starting Evaluation with " + _location + " " + _formation + " " + role);

        workspace.SetActive(true);

        if (_location == SetupLocation.LEFT) _network.rightStartEvaluation();
    }

    internal void ping()
    {
        Debug.Log("PING");
    }

    internal void rightStartEvaluation()
    {
        if (_location == SetupLocation.RIGHT)
        { startEvaluation(); }
    }

    void Update () {

        if (Input.GetKeyDown(KeyCode.P))
        {
            _network.ping();
        }

        if (!_init) return;  

        task = Task;

        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            _network.buttonPressed(_location.ToString());
        }

        if (task >= 1 && task <= 16 && role == Role.MANIPULATOR)
        {
            _network.syncCursor(cursor.transform.localPosition);
        }
	}

    internal void buttonPressed(string location)
    {
        Debug.Log("BUTTON PRESSED in " + location);
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

        // review who can do this!!!

        if (t == 0) role = _location == SetupLocation.LEFT ? Role.INSTRUCTOR : Role.MANIPULATOR;
        else role = _getRole();


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
        if (_location == SetupLocation.LEFT && t >= 1 && t <= 8)
        {
            return Role.INSTRUCTOR;
        }
        else return Role.MANIPULATOR;
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
        if (t >= 1 && t <= 16)
        {
            GUI.Label(new Rect(Screen.width - 100, 10, 100, 35), _getRole().ToString());
        }
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
