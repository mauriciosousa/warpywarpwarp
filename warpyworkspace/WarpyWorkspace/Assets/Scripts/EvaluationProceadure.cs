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

public enum EvalState
{
    SESSION, PAUSE
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

    public int T = 1;
    public EvalState evalState = EvalState.PAUSE;



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
        T = 1;
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

        if (evalState == EvalState.SESSION && _getRole(_location) == Role.MANIPULATOR)
        {
            _network.syncCursor(cursor.transform.localPosition);
        }
	}

    internal void buttonPressed(string location)
    {
        //SomeEnum enum = (SomeEnum)Enum.Parse(typeof(SomeEnum), "EnumValue");
        SetupLocation whoPressed = (SetupLocation)Enum.Parse(typeof(SetupLocation), location);
        
        if (_evaluationStarted)
        {
            if (_getRole(whoPressed) == Role.MANIPULATOR && _location == SetupLocation.LEFT)
            {

                if (evalState == EvalState.PAUSE)
                {
                    _startTask();
                }
                else
                {
                    _endTask();
                }
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

    private Role _getRole(SetupLocation location)
    {
        if (location == SetupLocation.LEFT && T <= 8)
            return Role.INSTRUCTOR;

        return Role.MANIPULATOR;
    }

    private void _startTask()
    {
        if (T <= 16)
        {
            _network.StartTask(T);
        }

        //print("T" + t);

        //if (_getRole(_location) == Role.MANIPULATOR) cursor.canDo = true;

        //if (_location == SetupLocation.LEFT)
        //{
        //    _startTime = DateTime.Now;
        //}
    }

    public void StartTask(int t)
    {
        evalState = EvalState.SESSION;
    }

    public void EndTask()
    {
        evalState = EvalState.PAUSE;
        T += 1;
        role = _getRole(_location);
    }

    private void _endTask()
    {
        _network.EndTask();

        //cursor.canDo = false;
        //if (_location == SetupLocation.LEFT)
        //{
        //    TimeSpan timeSpan = DateTime.Now - _startTime;
        //    print(timeSpan.TotalMilliseconds.ToString());
        //}
    }

    void OnGUI()
    {
        int top = 50;
        int left = 10;
        
        GUI.Label(new Rect(left, top, 100, 35), _getRole(_location).ToString(), style);

        top += 40;
        GUI.Label(new Rect(left, top, 100, 35), "T = " + T, style);

        top += 40;
        GUI.Label(new Rect(left, top, 100, 35), evalState.ToString(), style);
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
