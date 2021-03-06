﻿//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System.IO;






//public class Workspace : MonoBehaviour {

//    public GameObject instructorSpherePrefab;
//    private GameObject ballsParent;
//    public Transform spawnBalls;
//    public GameObject assemblerBall;

//    private int numberOfTasks = 21;
//    public int TASK = 0;

//    private string instructors_balls_filenames;

//    public Role participantRole = Role.INSTRUCTOR;
//    public SetupLocation _location;
//    public Test _test;
//    public Formation _condition;

//    private GameObject[] balls;

//    private float time;

//    public GUIStyle InfoToModeratorStyle;

//    private int intermissionTask = 11;
//    private List<int> habituationTasks;
//    private List<int> leftIsInstructorTasks;

//    private MainResultsFile _resultsFile;
//    private string _resultsFolder = "Results";

//    private WarpyNetwork _network;
//    private NetworkView _networkView;
    

//    private string ConfigFile;


//    private float buttonTimer = 1.0f;
//    private float _lastButtonPress = 0.0f;

//    private IsColliding _localCapsuleCollider;
//    private List<bool> _listOfCollidingFrames;

//    private bool _duringTask = false;

//    void Awake()
//    {
//        TASK = 0;
//        _resultsFolder = Application.dataPath + "/" + _resultsFolder;
//        if (!Directory.Exists(_resultsFolder))
//        {
//            Directory.CreateDirectory(_resultsFolder);
//            Debug.Log("Results Folder created!");
//        }
//        ConfigFile = Application.dataPath + "/config.txt";


//        _network = GetComponent<WarpyNetwork>();
//        _networkView = GetComponent<NetworkView>();
//        _location = (SetupLocation)Enum.Parse(enumType: typeof(SetupLocation), value: ConfigProperties.load(ConfigFile, "setup.type"));
//        _test = (Test)Enum.Parse(enumType: typeof(Test), value: ConfigProperties.load(ConfigFile, "test"));
//        _condition = (Formation)Enum.Parse(enumType: typeof(Formation), value: ConfigProperties.load(ConfigFile, "start.formation"));

//        _listOfCollidingFrames = new List<bool>();
//        _localCapsuleCollider = GameObject.Find("localCapsule").GetComponent<IsColliding>();

//    }

//    void Start()
//    {
//        habituationTasks = new List<int>() { 1, 6, 12, 17};
//        leftIsInstructorTasks = new List<int>() { 1, 2, 3, 4, 5, 11, 12, 13, 14, 15, 16};

//        instructors_balls_filenames = Application.dataPath + "/instructor_balls.txt";
//        ballsParent = new GameObject("balls");
//        ballsParent.transform.parent = this.transform;
//        ballsParent.transform.localPosition = Vector3.zero;
//        ballsParent.transform.localRotation = Quaternion.identity;

//        if (File.Exists(instructors_balls_filenames))
//        {
//            List<GameObject> b = new List<GameObject>();
//            string[] readText = File.ReadAllLines(instructors_balls_filenames);
//            foreach (string line in readText)
//            {
//                string[] data = line.Split(':');
//                GameObject ball = Instantiate(instructorSpherePrefab);
//                ball.name = data[0];
//                ball.transform.parent = ballsParent.transform;
//                ball.transform.localPosition = new Vector3(float.Parse(data[1]), float.Parse(data[2]), float.Parse(data[3]));
//                b.Add(ball);
//                ball.SetActive(false);
//            }
//            balls = b.ToArray();
//        }
//        else Debug.LogError("There is no Ballz!");


//        assemblerBall.SetActive(false);

//        participantRole = _location == SetupLocation.LEFT ? Role.INSTRUCTOR : Role.MANIPULATOR;

//        _network = GetComponent<WarpyNetwork>();
//        _network.__init__();

//    }

//    void Update ()
//    {
//        bool button = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W);

//        float ellapsed = Time.time - _lastButtonPress;
//        if (button)
//        {
//            if (ellapsed < buttonTimer)
//            {
//                button = false;
//            }
//            _lastButtonPress = Time.time;
//        }

//        if (button && _network.Connected && participantRole == Role.MANIPULATOR)
//        {
//            _networkView.RPC("RPC_ButtonPressed", RPCMode.All);
//        }




//        /***********/
//        if (Input.GetKeyDown(KeyCode.F1)) // create balls
//        {
//            if (ballsParent.transform.childCount == 0)
//            {
//                int totalNumberOfTasks = 21;

//                string prefix = "A";
//                for (int i = 1; i <= totalNumberOfTasks; i++)
//                {
//                    if (i != intermissionTask) // because intermission
//                    {
//                        GameObject ball = Instantiate(instructorSpherePrefab);
//                        ball.name = prefix + i;
//                        ball.transform.parent = ballsParent.transform;
//                        ball.transform.localPosition = Vector3.zero;
//                        ball.transform.localRotation = Quaternion.identity;
//                    }
//                }
//                prefix = "B";
//                for (int i = 1; i <= totalNumberOfTasks; i++)
//                {
//                    if (i != intermissionTask) // because intermission
//                    {
//                        GameObject ball = Instantiate(instructorSpherePrefab);
//                        ball.name = prefix + i;
//                        ball.transform.parent = ballsParent.transform;
//                        ball.transform.localPosition = Vector3.zero;
//                        ball.transform.localRotation = Quaternion.identity;
//                    }
//                }
//            }
//        }
//        if (Input.GetKeyDown(KeyCode.F2)) // save to file
//        {
//            if (ballsParent.transform.childCount > 0)
//            {
//                using (System.IO.StreamWriter file = new System.IO.StreamWriter(instructors_balls_filenames))
//                {
//                    foreach (Transform t in ballsParent.transform)
//                    {
//                        file.WriteLine(t.name + ":" + t.localPosition.x + ":" + t.localPosition.y + ":" + t.localPosition.z);
//                    }
//                    Debug.Log("Balls saved");
//                }
//            }
//            else
//            {
//                Debug.LogError("no balls");
//            }
//        }


//        // sync ball position

//        if (_duringTask)
//        {
//            _listOfCollidingFrames.Add(_localCapsuleCollider.COLLIDING);

//            if (participantRole == Role.MANIPULATOR && _network.Connected)
//            {
//                _networkView.RPC("RPC_SyncAssemblersBall", RPCMode.Others, assemblerBall.transform.localPosition);
//            }

//        }
//    }

//    private void _startNextTask()
//    {
//        if (TASK > numberOfTasks) return;

//        _duringTask = true;
//        _listOfCollidingFrames.Clear();
//        TASK += 1;

//        if (TASK == intermissionTask)
//        {
//            resetTrialParameters();
//            Debug.Log("Welcome to intermission!!!!");
//        }
//        else
//        {
//            _setupTask();
//            Debug.Log("START Task " + TASK);
//        }
//    }

//    private void _endPreviousTask()
//    {
//        if (TASK == 0) return;

//        _duringTask = false;

//        if (TASK > numberOfTasks)
//        {
//            Debug.Log("ACABOU");
//            return;
//        }
//        else
//        {
//            if (TASK != intermissionTask)
//            {
//                /* do stuff here */
//                _cleanUpTask();

//                //Debug.Log("END Task " + _task);
//            }
//        }
//    }

//    private GameObject _getInstructorBall(Test t, int task)
//    {

//        foreach (GameObject b in balls)
//        {
//            if (b.name == t.ToString() + task)
//                return b;
//        }

//        return null;
//    }

//    private void _setupTask()
//    {
//        if (TASK > numberOfTasks) return;

//        if (TASK == intermissionTask)
//        {
//            // do intermission
//        }
//        else
//        {
//            if (TASK == 1) _resultsFile = new MainResultsFile(_resultsFolder + "/" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");

//            resetTrialParameters();
//            time = Time.time;
//            if (participantRole == Role.INSTRUCTOR)
//            {
//                _getInstructorBall(_test, TASK).SetActive(true);
//            }
//            else
//            {
//                assemblerBall.SetActive(true);
//            }
//            assemblerBall.transform.position = spawnBalls.position;

//        }
//    }

//    private void _cleanUpTask()
//    {
//        time = Time.time - time;
//        Debug.Log("        > demorou " + time + "s");
//        GameObject ball = _getInstructorBall(_test, TASK);

//        float errorDistance = Vector3.Distance(ball.transform.position, assemblerBall.transform.position);

        

//        if (!habituationTasks.Contains(TASK))
//        {
//            BallQuadrant ballQuadrant = _getBallQuadrant();

//            float percentage = _calcCollidingPercentage();
//            _listOfCollidingFrames.Clear();

//            /*  x task
//             *  x test
//             *  x condition
//             *  x ballQuadrant
//             *  - errorDistance
//             *  x time
//             *  - sameSpacePercentage
//             *  - instructorBall
//             *  - assemblerBall
//             */

//            Debug.Log("writing results");
//            _resultsFile.writeLine(TASK, _test, _condition, ballQuadrant, errorDistance, time, percentage, ball.transform.position, assemblerBall.transform.position);
//        }

//        ball.SetActive(false);
//        assemblerBall.SetActive(false);
//    }

//    private float _calcCollidingPercentage()
//    {
//        int collidingFrames = 0;
//        foreach (bool b in _listOfCollidingFrames)
//        {
//            if (b) collidingFrames += 1;
//        }

//        if (collidingFrames == 0) return 0;
//        else
//            return collidingFrames / _listOfCollidingFrames.Count;
//    }

//    private BallQuadrant _getBallQuadrant()
//    {
//        if (new List<int>() { 5, 7, 14, 20 }.Contains(TASK))
//            return BallQuadrant.I;

//        else if (new List<int>() {2, 8, 15, 21}.Contains(TASK))
//            return BallQuadrant.II;

//        if (new List<int>() {3, 9, 16, 18}.Contains(TASK))
//            return BallQuadrant.III;

//        else
//            return BallQuadrant.IV;
//    }

//    public void resetTrialParameters()
//    {
//        if (leftIsInstructorTasks.Contains(TASK))
//        {
//            participantRole = _location == SetupLocation.LEFT ? Role.INSTRUCTOR : Role.MANIPULATOR;
//        }
//        else
//        {
//            participantRole = _location == SetupLocation.LEFT ? Role.MANIPULATOR : Role.INSTRUCTOR;

//        }

//        if (_test == Test.A)
//        {
//            if (TASK >= 1 && TASK <= 6)
//                _condition = Formation.FACE_TO_FACE;
//            else
//                _condition = Formation.SIDE_TO_SIDE;

//        }
//        else // B
//        {
//            if (TASK >= 1 && TASK <= 6)
//                _condition = Formation.SIDE_TO_SIDE;
//            else
//                _condition = Formation.FACE_TO_FACE;
//        }

//    }

//    private void OnGUI()
//    {
//        GUI.Label(new Rect(10, 10, 500, 50), "" + _network.Connected, InfoToModeratorStyle);


//        if (TASK > numberOfTasks)
//        {
//            GUI.Label(new Rect(10, 40, 500, 50), "ACABOU", InfoToModeratorStyle);
//        }
//        else if(TASK > 0 && TASK <= numberOfTasks)
//        {
//            int top = 40;
//            int step = 35;
//            if (TASK == intermissionTask)
//            {
//                GUI.Label(new Rect(10, top, 500, 50), "INTERMISSION", InfoToModeratorStyle);
//            }
//            else
//            {
//                if (habituationTasks.Contains(TASK))
//                {
//                    GUI.Label(new Rect(10, top, 500, 50), "[[[  HABITUATION TASK ]]]", InfoToModeratorStyle);
//                    top += step; top += step;
//                }

//                GUI.Label(new Rect(10, top, 500, 50), "CONDITION: " + _condition, InfoToModeratorStyle); top += step; top += step;
//                GUI.Label(new Rect(10, top, 500, 50), "ROLE: " + participantRole, InfoToModeratorStyle); top += step; top += step;
//                GUI.Label(new Rect(10, top, 500, 50), "TASK: " + TASK, InfoToModeratorStyle); top += step; top += step;
//                GUI.Label(new Rect(10, top, 500, 50), "TIME: " + Math.Round((Time.time - time), 1) + "s", InfoToModeratorStyle);
//            }
//        } 
//    }

//    internal void buttonPressed()
//    {
//        _endPreviousTask();
//        _startNextTask();
//    }

//    internal void syncAssemblersBall(Vector3 position)
//    {
//        if (participantRole == Role.INSTRUCTOR)
//        {
//            assemblerBall.transform.localPosition = position;
//        }
//    }
//}
