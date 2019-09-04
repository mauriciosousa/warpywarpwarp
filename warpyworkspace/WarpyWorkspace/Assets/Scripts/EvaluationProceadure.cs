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
    A, B, C
}

public enum BallQuadrant
{
    I, II, III, IV
}

public class EvaluationProceadure : MonoBehaviour {

    public GameObject workspaceModel;
    public Transform localWorkspaceOrigin;
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
    public Transform startArrow;

    public int T = 1;
    public EvalState evalState = EvalState.PAUSE;



    private DateTime _startTime;

    private string _resultsFolder;

    private bool _init = false;


    public Transform balls;
    public List<GameObject> ABalls;
    public List<GameObject> BBalls;
    public List<GameObject> CBalls;

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
        CBalls = new List<GameObject>();

        foreach (Transform child in balls)
        {
            if (child.name[0] == 'A')
            {
                ABalls.Add(child.gameObject);
            }
            else if (child.name[0] == 'B')
            {
                BBalls.Add(child.gameObject);

            }
            else
            {
                CBalls.Add(child.gameObject);

            }
        }

        workspaceModel.SetActive(false);
    }

    public void Init(SetupLocation location, Formation formation, int leftID, int rightID, Test test)
    {
        _location = location;
        _formation = formation;
        _leftID = leftID;
        _rightID = rightID;
        _test = test;

        _resultsFolder = Application.dataPath + Path.DirectorySeparatorChar + "Results";
        if (!Directory.Exists(_resultsFolder))
        {
            Directory.CreateDirectory(_resultsFolder);
        }

        if (_location == SetupLocation.LEFT)
        {
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
        else if (!ACABOU && _evaluationStarted && role == Role.MANIPULATOR)//pause
        {
            startArrow.GetComponent<SlowRotation>().active = true;
        }
	}

    [Space(5)]
    [Header("Local and Remote Joints for logging:")]
    public Transform localHuman_leftHandTip;
    public Transform localHuman_rightHandTip;
    public Transform localHuman_spineBase;
    public Transform remoteHuman_leftHandTip;
    public Transform remoteHuman_rightHandTip;
    public Transform remoteHuman_spineBase;
    public float angle = 0f;
    void FixedUpdate()
    {
        //TimeSpan span = DateTime.Now - _lastTimestamp;
        //if (span.Milliseconds >= 200) // TODO: FILE NOT WRITE EVERY TIME LOZ
        {
        //    _lastTimestamp = DateTime.Now;

            if (evalState == EvalState.SESSION && _location == SetupLocation.LEFT)
            {            
                pc_whole += 1;
                if (proxemics.humansColliding) pc_inside += 1;

                if (_evalDataFile != null)
                {
                    _evalDataFile.writeLine(proxemics.distance, proxemics.distanceClassification, localHuman_leftHandTip.position, localHuman_rightHandTip.position, localHuman_spineBase.position, remoteHuman_leftHandTip.position, remoteHuman_rightHandTip.position, remoteHuman_spineBase.position);
                }
            }

            if (evalState == EvalState.SESSION && _angleData != null)
            {
                Vector3 headToWorkspace = (workspaceModel.transform.position - Camera.main.transform.position);
                headToWorkspace = _ipisiloneAZeroENormaliz(headToWorkspace);

                Vector3 headRotation = (Camera.main.transform.rotation.eulerAngles);
                headRotation = _ipisiloneAZeroENormaliz(headRotation);

                //Debug.DrawLine(workspace.transform.position, Camera.main.transform.position, Color.green);
                //Debug.DrawLine(Camera.main.transform.position, Camera.main.transform.position + Camera.main.transform.forward);

                angle = Vector3.Angle(headToWorkspace, headRotation);

                _angleData.writeLine(angle);
            }
        }
    }

    private Vector3 _ipisiloneAZeroENormaliz(Vector3 p)
    {
        return new Vector3(p.x, 0f, p.z).normalized;
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

        //WorkspaceModel.rotation = Quaternion.LookRotation(-localWorkspaceOrigin.transform.forward, localWorkspaceOrigin.transform.up);
        if (role == Role.MANIPULATOR && _formation == Formation.REAL_LIFE)
        {
            workspaceModel.transform.rotation = Quaternion.LookRotation(-localWorkspaceOrigin.transform.forward, localWorkspaceOrigin.transform.up);
        }

        workspaceModel.SetActive(true);
        

        evalState = EvalState.SESSION;

        _instructorBall = _getInstructorBall(_test, t);

        startArrow.GetComponent<SlowRotation>().active = false;

        cursor.transform.position = new Vector3(workspaceModel.transform.position.x, workspaceModel.transform.position.y - 1f, workspaceModel.transform.position.z);

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

            _evalDataFile = new EvaluationData(_resultsFolder + Path.DirectorySeparatorChar + "Task_" + T + "_" + _formation + ".txt");
        }

        _angleData = new WorkspaceAngleData(_resultsFolder + Path.DirectorySeparatorChar + "Angle_T" + T + "_participant_" + (_location == SetupLocation.LEFT ? _leftID : _rightID) + "-" + role + "-" + _formation + ".txt");
    }

    public bool ACABOU = false;
    public void EndTask()
    {
        evalState = EvalState.PAUSE;
        T += 1;
        cursor.canDo = false;
        _instructorBall.gameObject.GetComponent<Renderer>().enabled = false;
        arrow.GetComponent<SlowRotation>().active = false;

        workspaceModel.SetActive(false);


        if (_location == SetupLocation.LEFT)
        {
            TimeSpan timeSpan = DateTime.Now - _startTime;
            print("  TASK " + (T-1) + " ended.... with " + timeSpan.TotalMilliseconds.ToString() + "ms");

            float errorDistance = Vector3.Distance(_instructorBall.position, cursor.transform.position);
            print(" Error Distance: " + errorDistance);

            float insidePercentage = (pc_inside * 100) / pc_whole;
            if (float.IsNaN(insidePercentage)) insidePercentage = 0;
            print(" percentage inside: " + insidePercentage);

            //_resultsFile.writeLine(_leftID, _rightID, _getRole(_location), (T-1), _test, _getQuadrant(_instructorBall.localPosition), errorDistance, insidePercentage, _formation, workspace.transform.position);

            _resultsFile.writeLine(_leftID, _rightID, _getRole(_location), (T - 1), _test, _getQuadrant(_instructorBall.localPosition), timeSpan, errorDistance, insidePercentage, _formation, workspaceModel.transform.position);

            _evalDataFile.flush();
            _evalDataFile = null;
        }

        _angleData.flush();
        _angleData = null;
        _instructorBall = null;
        cursor.transform.localPosition = Vector3.zero;

        if (T > 16)
        {
            ACABOU = true;
            if (_location == SetupLocation.LEFT) _resultsFile.flush();
        }
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
        List<GameObject> list;
        if (test == Test.A)
            list = ABalls;
        else if (test == Test.B)
            list = BBalls;
        else
            list = CBalls;

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

    private List<string> _lines;

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
        header += "timespan" + _sep;
        header += "errorDistance" + _sep;
        header += "%inside" + _sep;
        header += "condition" + _sep;

        header += "workspacePosition_X" + _sep;
        header += "workspacePosition_Y" + _sep;
        header += "workspacePosition_Z" + _sep;


        _lines = new List<string>();
        //_writeLine(header);
        _lines.Add(header);
        Debug.Log("created: " + filename);
    }

    private void _writeLine(string line)
    {
        File.AppendAllText(_file, line + Environment.NewLine);
    }

    public void writeLine(int leftID, int rightID, Role leftRole, int task, Test test, BallQuadrant quadrant, TimeSpan timespan, float errorDistance, float percentage, Formation condition, Vector3 workspacePosition)
    {
        string line = "";

        line += DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss") + _sep;
        line += leftID + _sep;
        line += rightID + _sep;
        line += leftRole.ToString() + _sep;
        line += task + _sep;
        line += test + _sep;
        line += quadrant.ToString() + _sep;
        line += timespan.TotalMilliseconds.ToString() + _sep;
        line += errorDistance + _sep;
        line += percentage + _sep;
        line += condition.ToString() + _sep;
        line += workspacePosition.x + _sep;
        line += workspacePosition.y + _sep;
        line += workspacePosition.z + _sep;

        _lines.Add(line);
        //_writeLine(line);
    }

    public void flush()
    {
        File.WriteAllLines(_file, _lines.ToArray());
    }
}

public class EvaluationData
{
    private string _file;
    private string _sep = "$";
    private List<string> _lines;

    public EvaluationData(string filename)
    {
        _file = filename;

        string header = "";

        header += "Timestamp" + _sep;
        header += "Distance" + _sep;
        header += "ProxemicClassification" + _sep;

        header += "LEFTHUMAN_LHandTip_X" + _sep;
        header += "LEFTHUMAN_LHandTip_Y" + _sep;
        header += "LEFTHUMAN_LHandTip_Z" + _sep;
        header += "LEFTHUMAN_RHandTip_X" + _sep;
        header += "LEFTHUMAN_RHandTip_Y" + _sep;
        header += "LEFTHUMAN_RHandTip_Z" + _sep;
        header += "LEFTHUMAN_SPINEBASE_X" + _sep;
        header += "LEFTHUMAN_SPINEBASE_Y" + _sep;
        header += "LEFTHUMAN_SPINEBASE_Z" + _sep;

        header += "RIGHTHUMAN_LHandTip_X" + _sep;
        header += "RIGHTHUMAN_LHandTip_Y" + _sep;
        header += "RIGHTHUMAN_LHandTip_Z" + _sep;
        header += "RIGHTHUMAN_RHandTip_X" + _sep;
        header += "RIGHTHUMAN_RHandTip_Y" + _sep;
        header += "RIGHTHUMAN_RHandTip_Z" + _sep;
        header += "RIGHTHUMAN_SPINEBASE_X" + _sep;
        header += "RIGHTHUMAN_SPINEBASE_Y" + _sep;
        header += "RIGHTHUMAN_SPINEBASE_Z" + _sep;

        _lines = new List<string>();
        _lines.Add(header);
//        _writeLine(header);
        Debug.Log("created: " + filename);
    }

    private void _writeLine(string line)
    {
        File.AppendAllText(_file, line + Environment.NewLine);
    }

    public void writeLine(float distance, ProxemicDistances proxemicClassification, Vector3 localHuman_leftHandTip, Vector3 localHuman_rightHandTip, Vector3 localHuman_spineBase, Vector3 remoteHuman_leftHandTip, Vector3 remoteHuman_rightHandTip, Vector3 remoteHuman_spineBase)
    {
        string line = "";

        line += DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss") + _sep;
        line += distance + _sep;
        line += proxemicClassification.ToString() + _sep;


        line += localHuman_leftHandTip.x + _sep;
        line += localHuman_leftHandTip.y + _sep;
        line += localHuman_leftHandTip.z + _sep;

        line += localHuman_rightHandTip.x + _sep;
        line += localHuman_rightHandTip.y + _sep;
        line += localHuman_rightHandTip.z + _sep;

        line += localHuman_spineBase.x + _sep;
        line += localHuman_spineBase.y + _sep;
        line += localHuman_spineBase.z + _sep;

        line += remoteHuman_leftHandTip.x + _sep;
        line += remoteHuman_leftHandTip.y + _sep;
        line += remoteHuman_leftHandTip.z + _sep;

        line += remoteHuman_rightHandTip.x + _sep;
        line += remoteHuman_rightHandTip.y + _sep;
        line += remoteHuman_rightHandTip.z + _sep;

        line += remoteHuman_spineBase.x + _sep;
        line += remoteHuman_spineBase.y + _sep;
        line += remoteHuman_spineBase.z + _sep;

        _lines.Add(line);
        //_writeLine(line);
    }

    public void flush()
    {
        File.WriteAllLines(_file, _lines.ToArray());
    }
}

public class WorkspaceAngleData
{
    private string _file;
    private string _sep = "$";
    private List<string> _lines;


    public WorkspaceAngleData(string filename)
    {
        _file = filename;

        string header = "";

        header += "Timestamp" + _sep;
        header += "Angle" + _sep;

        _lines = new List<string>();
        _lines.Add(header);
        //_writeLine(header);
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

        _lines.Add(line);
        //_writeLine(line);
    }

    public void flush()
    {
        File.WriteAllLines(_file, _lines.ToArray());
    }
}
