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

public enum Test
{
    A, B
}

public enum BallQuadrant
{
    I, II, III, IV
}

public class EvaluationProceadure : MonoBehaviour {

    public GameObject workspace;
    private AlteredTelepresenceNetwork _network;

    private SetupLocation _location;
    private Formation _formation; // = condition
    public Role role;
    private int _leftID;
    private int _rightID;
    private Test _test;

    public GUIStyle style;

    public AssemblerCursor cursor;
    public Transform arrow;

    public int T = 1;
    public EvalState evalState = EvalState.PAUSE;



    private DateTime _startTime;

    private string _resultsFolder;

    private bool _init = false;


    public Transform balls;
    public List<GameObject> ABalls;
    public List<GameObject> BBalls;

    public ProxemicsAnalysis proxemics;
    private float pc_whole = 0;
    private float pc_inside = 0;
    private MainResultsFile _resultsFile;

    private DateTime _lastTimestamp;


    private void Awake()
    {
        _lastTimestamp = DateTime.Now;
    }

    void Start () {
        _resultsFolder = null;
        _network = GetComponent<AlteredTelepresenceNetwork>();

        ABalls = new List<GameObject>();
        BBalls = new List<GameObject>();

        foreach (Transform child in balls)
        {
            if (child.name[0] == 'A')
            {
                ABalls.Add(child.gameObject);
            }
            else
            {
                BBalls.Add(child.gameObject);

            }
        }
    }

    public void Init(SetupLocation location, Formation formation, int leftID, int rightID, Test test)
    {
        _location = location;
        _formation = formation;
        _leftID = leftID;
        _rightID = rightID;
        _test = test;

        if (_location == SetupLocation.LEFT)
        {
            _resultsFolder = Application.dataPath + Path.DirectorySeparatorChar + "Results";
            if (!Directory.Exists(_resultsFolder))
            {
                Directory.CreateDirectory(_resultsFolder);
            }
            _resultsFile = new MainResultsFile(_resultsFolder + Path.DirectorySeparatorChar + "MainResults-" + _formation + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");
        }
    }

    private bool _evaluationStarted = false;
    public bool EvaluationStarted
    {
        get
        {
            return _evaluationStarted;
        }
    }

    public void startEvaluation()
    {
        _evaluationStarted = true;
        T = 1;
        role = _location == SetupLocation.LEFT ? Role.INSTRUCTOR : Role.MANIPULATOR;
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

        role = _getRole(_location);
        if (evalState == EvalState.SESSION)
        {
            if (role == Role.MANIPULATOR)
            {
                _network.syncCursor(cursor.transform.localPosition);
            }
            else
            {
                pc_whole = 0;
                pc_inside = 0;
            }
        }
	}

    public float angle = 0f;
    void FixedUpdate()
    {
        TimeSpan span = DateTime.Now - _lastTimestamp;
        if (span.Milliseconds >= 200)
        {
            _lastTimestamp = DateTime.Now;

            if (evalState == EvalState.SESSION && _location == SetupLocation.LEFT)
            {            
                pc_whole += 1;
                if (proxemics.humansColliding) pc_inside += 1;

                if (_evalDataFile != null)
                {
                    _evalDataFile.writeLine(proxemics.distance, proxemics.distanceClassification, proxemics.isFocusedOnWorkspace);
                }
            }

            if (evalState == EvalState.SESSION && _angleData != null)
            {
                Vector3 headToWorkspace = (workspace.transform.position - Camera.main.transform.position);
                Vector3 headRotation = (Camera.main.transform.rotation.eulerAngles);

                //Debug.DrawLine(workspace.transform.position, Camera.main.transform.position, Color.green);
                //Debug.DrawLine(Camera.main.transform.position, Camera.main.transform.position + Camera.main.transform.forward);

                angle = Vector3.Angle(headToWorkspace, headRotation);

                _angleData.writeLine(angle);
            }
        }
    }

    internal void buttonPressed(string location)
    {
        //SomeEnum enum = (SomeEnum)Enum.Parse(typeof(SomeEnum), "EnumValue");
        SetupLocation whoPressed = (SetupLocation)Enum.Parse(typeof(SetupLocation), location);
        

        if (_evaluationStarted)
        {

            //print(whoPressed.ToString() + " is " + _getRole(whoPressed).ToString());

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
        if (T <= 8)
        {
            return location == SetupLocation.LEFT ? Role.INSTRUCTOR : Role.MANIPULATOR;
        }
        else
        {
            return location == SetupLocation.LEFT ? Role.MANIPULATOR : Role.INSTRUCTOR;
        }
    }

    private void _startTask()
    {
        if (T <= 16)
        {
            _network.StartTask(T);
        }
    }

    private Transform _instructorBall;
    private EvaluationData _evalDataFile;
    private WorkspaceAngleData _angleData;
    public void StartTask(int t)
    {
        print("" + role + " " + _location);

        evalState = EvalState.SESSION;

        _instructorBall = _getInstructorBall(_test, t);

        if (role == Role.MANIPULATOR)
        {
            cursor.canDo = true;
        }
        else
        {
            _instructorBall.gameObject.GetComponent<Renderer>().enabled = true;
            arrow.localPosition = new Vector3(_instructorBall.localPosition.x, 0.5f, _instructorBall.localPosition.z);
            arrow.GetComponent<SlowRotation>().active = true;
        }

        if (_location == SetupLocation.LEFT)
        {
            _startTime = DateTime.Now;
            print("  TASK " + T + " started!!!!");
            pc_inside = 0;
            pc_whole = 0;

            _evalDataFile = new EvaluationData(_resultsFolder + Path.DirectorySeparatorChar + "Task_" + T + ".txt");
        }
        _angleData = new WorkspaceAngleData(_resultsFolder + Path.DirectorySeparatorChar + "Angle_T" + T + "_participant_" + (_location == SetupLocation.LEFT ? _leftID : _rightID) + "-" + role +".txt");
    }

    public bool ACABOU = false;
    public void EndTask()
    {
        evalState = EvalState.PAUSE;
        T += 1;
        cursor.canDo = false;
        _instructorBall.gameObject.GetComponent<Renderer>().enabled = false;
        arrow.GetComponent<SlowRotation>().active = false;


        if (_location == SetupLocation.LEFT)
        {
            TimeSpan timeSpan = DateTime.Now - _startTime;
            print("  TASK " + (T-1) + " ended.... with " + timeSpan.TotalMilliseconds.ToString() + "ms");

            float errorDistance = Vector3.Distance(_instructorBall.position, cursor.transform.position);
            print(" Error Distance: " + errorDistance);

            float insidePercentage = (pc_inside * 100) / pc_whole;
            if (float.IsNaN(insidePercentage)) insidePercentage = 0;
            print(" percentage inside: " + insidePercentage);

            _resultsFile.writeLine(_leftID, _rightID, _getRole(_location), (T-1), _test, _getQuadrant(_instructorBall.localPosition), errorDistance, insidePercentage, _formation);
            _evalDataFile = null;
        }

        _angleData = null;
        _instructorBall = null;
        cursor.transform.localPosition = Vector3.zero;

        if (T > 16) ACABOU = true;
    }

    private void _endTask()
    {
        _network.EndTask();
    }

    private BallQuadrant _getQuadrant(Vector3 lp)
    {
        if (lp.x > 0)
        {
            return lp.z > 0 ? BallQuadrant.I : BallQuadrant.IV;
        }
        else
        {
            return lp.z > 0 ? BallQuadrant.II : BallQuadrant.III;
        }

    }

    private Transform _getInstructorBall(Test test, int t)
    {
        List<GameObject> list = test == Test.A ? ABalls : BBalls;
        return list[t - 1].transform;
    }

    void OnGUI()
    {
        int top = 50;
        int left = 10;

        if (!ACABOU)
        {
            GUI.Label(new Rect(left, top, 100, 35), "" + _network.networkPeerType + ", Connected = " + _network.Connected + ", " + (_network.Peers == 1 ? "1 Client" : "" + _network.Peers + " Clients"), style);

            if (_evaluationStarted)
            {
                top += 40;
                GUI.Label(new Rect(left, top, 100, 35), _getRole(_location).ToString(), style);

                top += 40;
                GUI.Label(new Rect(left, top, 100, 35), evalState.ToString(), style);

                top += 40;
                if (evalState == EvalState.SESSION)
                {
                    GUI.Label(new Rect(left, top, 100, 35), "T = " + T, style);
                }
                else
                {
                    GUI.Label(new Rect(left, top, 100, 35), "Next T = " + T, style);
                }
            }
        }
        else
        {
            style.fontSize = 150;
            GUI.Label(new Rect(100, 100, Screen.width, Screen.height), "ACABOU", style);
        }
    }

    internal void communicateStart()
    {
        if (_location == SetupLocation.LEFT && _network.Peers != 0)
        {
            _network.startEvaluation();
        }
    }
}



public class MainResultsFile
{
    private string _file;
    private string _sep = "$";

    public MainResultsFile(string filename)
    {
        _file = filename;

        string header = "";

        header += "Timestamp" + _sep; //
        header += "leftID" + _sep;//
        header += "rightID" + _sep;//
        header += "leftRole" + _sep;//
        header += "task" + _sep;//
        header += "test" + _sep;//)
        header += "ballQuadrant" + _sep;
        header += "errorDistance" + _sep;
        header += "%inside" + _sep;
        header += "condition" + _sep;

        _writeLine(header);
        Debug.Log("created: " + filename);
    }

    private void _writeLine(string line)
    {
        File.AppendAllText(_file, line + Environment.NewLine);
    }

    public void writeLine(int leftID, int rightID, Role leftRole, int task, Test test, BallQuadrant quadrant, float errorDistance, float percentage, Formation condition)
    {
        string line = "";

        line += DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss") + _sep;
        line += leftID + _sep;
        line += rightID + _sep;
        line += leftRole.ToString() + _sep;
        line += task + _sep;
        line += test + _sep;
        line += quadrant.ToString() + _sep;
        line += errorDistance + _sep;
        line += percentage + _sep;
        line += condition.ToString() + _sep;


        _writeLine(line);
    }
}

public class EvaluationData
{
    private string _file;
    private string _sep = "$";

    public EvaluationData(string filename)
    {
        _file = filename;

        string header = "";

        header += "Timestamp" + _sep;
        header += "Distance" + _sep;
        header += "ProxemicClassification" + _sep;
        header += "FocusOnWorkspace" + _sep;

        _writeLine(header);
        Debug.Log("created: " + filename);
    }

    private void _writeLine(string line)
    {
        File.AppendAllText(_file, line + Environment.NewLine);
    }

    public void writeLine(float distance, ProxemicDistances proxemicClassification, bool focusOnWorkspace)
    {
        string line = "";

        line += DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss") + _sep;
        line += distance + _sep;
        line += proxemicClassification.ToString() + _sep;
        line += focusOnWorkspace + _sep;

        _writeLine(line);
    }
}

public class WorkspaceAngleData
{
    private string _file;
    private string _sep = "$";

    public WorkspaceAngleData(string filename)
    {
        _file = filename;

        string header = "";

        header += "Timestamp" + _sep;
        header += "Angle" + _sep;

        _writeLine(header);
        Debug.Log("created: " + filename);
    }

    private void _writeLine(string line)
    {
        File.AppendAllText(_file, line + Environment.NewLine);
    }

    public void writeLine(float angle)
    {
        string line = "";

        line += DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss") + _sep;
        line += angle + _sep;

        _writeLine(line);
    }
}
