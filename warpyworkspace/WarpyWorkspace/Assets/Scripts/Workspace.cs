﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public enum Role
{
    ASSEMBLER, 
    INSTRUCTOR
}

public enum Test
{
    A, B
}

public enum BallQuadrant
{
    I, II, III, IV
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

    internal void writeDebugLine(int task, Test test, Formation condition, BallQuadrant ballQuadrant, float time)
    {
        string line = "";

        line += DateTime.Now.ToString("yyyyMMddHHmmss") + _sep;
        line += task + _sep;
        line += test.ToString() + _sep;
        line += condition.ToString() + _sep;
        line += ballQuadrant.ToString() + _sep;
        line += time;

        _writeLine(line);
    }
}

public class Workspace : MonoBehaviour {

    public GameObject instructorSpherePrefab;
    public GameObject assemblerSpherePrefab;
    private GameObject ballsParent;
    public Transform spawnBalls;

    private int numberOfTasks = 21;
    private int _task = 0;

    private string instructors_balls_filenames;

    private Role _participant = Role.INSTRUCTOR;
    private SetupLocation _location = SetupLocation.LEFT;
    private Test _test = Test.A;
    public Formation _condition = Formation.FACE_TO_FACE;

    private GameObject[] balls;

    private float time;

    public GUIStyle InfoToModeratorStyle;

    private int intermissionTask = 11;
    private List<int> habituationTasks;
    private List<int> leftIsInstructorTasks;

    private ResultsFile _resultsFile;
    private string _resultsFolder = "Results";

    private WarpyNetwork _network;

    void Awake()
    {
        _resultsFolder = Application.dataPath + "/" + _resultsFolder;
        if (!Directory.Exists(_resultsFolder))
        {
            Directory.CreateDirectory(_resultsFolder);
            Debug.Log("Results Folder created!");
        }


        _network = GetComponent<WarpyNetwork>();
         
    }

    void Start()
    {
        habituationTasks = new List<int>() { 1, 6, 12, 17};
        leftIsInstructorTasks = new List<int>() { 1, 2, 3, 4, 5, 12, 13, 14, 15, 16};

        instructors_balls_filenames = Application.dataPath + "/instructor_balls.txt";
        ballsParent = new GameObject("balls");
        ballsParent.transform.parent = this.transform;
        ballsParent.transform.localPosition = Vector3.zero;
        ballsParent.transform.localRotation = Quaternion.identity;

        if (File.Exists(instructors_balls_filenames))
        {
            List<GameObject> b = new List<GameObject>();
            string[] readText = File.ReadAllLines(instructors_balls_filenames);
            foreach (string line in readText)
            {
                string[] data = line.Split(':');
                GameObject ball = Instantiate(instructorSpherePrefab);
                ball.name = data[0];
                ball.transform.parent = ballsParent.transform;
                ball.transform.localPosition = new Vector3(float.Parse(data[1]), float.Parse(data[2]), float.Parse(data[3]));
                b.Add(ball);
            }
            balls = b.ToArray();
        }
        else Debug.LogError("There is no Ballz!");

        _participant = _location == SetupLocation.LEFT ? Role.INSTRUCTOR : Role.ASSEMBLER;

        _network = GetComponent<WarpyNetwork>();
        _network.__init__();

    }

    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _endPreviousTask();
            _startNextTask();
        }




        /***********/
        if (Input.GetKeyDown(KeyCode.F1)) // create balls
        {
            if (ballsParent.transform.childCount == 0)
            {
                int totalNumberOfTasks = 21;

                string prefix = "A";
                for (int i = 1; i <= totalNumberOfTasks; i++)
                {
                    if (i != intermissionTask) // because intermission
                    {
                        GameObject ball = Instantiate(instructorSpherePrefab);
                        ball.name = prefix + i;
                        ball.transform.parent = ballsParent.transform;
                        ball.transform.localPosition = Vector3.zero;
                        ball.transform.localRotation = Quaternion.identity;
                    }
                }
                prefix = "B";
                for (int i = 1; i <= totalNumberOfTasks; i++)
                {
                    if (i != intermissionTask) // because intermission
                    {
                        GameObject ball = Instantiate(instructorSpherePrefab);
                        ball.name = prefix + i;
                        ball.transform.parent = ballsParent.transform;
                        ball.transform.localPosition = Vector3.zero;
                        ball.transform.localRotation = Quaternion.identity;
                    }
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.F2)) // save to file
        {
            if (ballsParent.transform.childCount > 0)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(instructors_balls_filenames))
                {
                    foreach (Transform t in ballsParent.transform)
                    {
                        file.WriteLine(t.name + ":" + t.localPosition.x + ":" + t.localPosition.y + ":" + t.localPosition.z);
                    }
                    Debug.Log("Balls saved");
                }
            }
            else
            {
                Debug.LogError("no balls");
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (_task < 1)
            {
                if (_test == Test.A) _test = Test.B; else _test = Test.A;
            }
        }

    }

    private void _startNextTask()
    {
        if (_task > numberOfTasks) return;

        _task += 1;

        if (_task == intermissionTask)
        {
            Debug.Log("Welcome to intermission!!!!");
        }
        else
        {
            _setupTask();
            Debug.Log("START Task " + _task);
        }
    }

    private void _endPreviousTask()
    {
        if (_task == 0) return;

        if (_task > numberOfTasks)
        {
            Debug.Log("ACABOU");
            return;
        }
        else
        {
            if (_task != intermissionTask)
            {
                /* do stuff here */
                _cleanUpTask();

                //Debug.Log("END Task " + _task);
            }
        }
    }

    private GameObject _getInstructorBall(Test t, int task)
    {

        foreach (GameObject b in balls)
        {
            if (b.name == t.ToString() + task)
                return b;
        }

        return null;
    }

    private void _setupTask()
    {
        if (_task == intermissionTask)
        {
            // do intermission
        }
        else
        {
            if (_task == 1) _resultsFile = new ResultsFile(_resultsFolder + "/" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");

            resetTrialParameters();
            time = Time.time;
            if (_participant == Role.INSTRUCTOR)
            {
                _getInstructorBall(_test, _task).SetActive(true);
            }
            else
            {

            }
        }
    }

    private void _cleanUpTask()
    {
        time = Time.time - time;
        Debug.Log("        > demorou " + time + "s");
        _getInstructorBall(_test, _task).SetActive(false);

        if (!habituationTasks.Contains(_task))
        {
            BallQuadrant ballQuadrant = _getBallQuadrant();


            /*  x task
             *  x test
             *  x condition
             *  x ballQuadrant
             *  - errorDistance
             *  x time
             *  - sameSpacePercentage
             *  - instructorBall
             *  - assemblerBall
             */

            Debug.Log("writing results");
            _resultsFile.writeDebugLine(_task, _test, _condition, ballQuadrant, time);
        }
    }

    private BallQuadrant _getBallQuadrant()
    {
        if (new List<int>() { 5, 7, 14, 20 }.Contains(_task))
            return BallQuadrant.I;

        else if (new List<int>() {2, 8, 15, 21}.Contains(_task))
            return BallQuadrant.II;

        if (new List<int>() {3, 9, 16, 18}.Contains(_task))
            return BallQuadrant.III;

        else
            return BallQuadrant.IV;
    }

    public GameObject spawnAssemblerBall()
    {
        GameObject assemblerBall = Instantiate(assemblerSpherePrefab);
        assemblerBall = new GameObject("Grab_" + _test + _task);
        assemblerBall.transform.parent = this.transform;
        assemblerBall.transform.position = spawnBalls.position;
        return assemblerBall;
    }

    public void resetTrialParameters()
    {
        if (leftIsInstructorTasks.Contains(_task))
        {
            _participant = _location == SetupLocation.LEFT ? Role.INSTRUCTOR : Role.ASSEMBLER;
        }
        else
        {
            _participant = _location == SetupLocation.LEFT ? Role.ASSEMBLER : Role.INSTRUCTOR;

        }

        if (_test == Test.A)
        {
            if (_task >= 1 && _task <= 6)
                _condition = Formation.FACE_TO_FACE;
            else
                _condition = Formation.SIDE_TO_SIDE;

        }
        else // B
        {
            if (_task >= 1 && _task <= 6)
                _condition = Formation.SIDE_TO_SIDE;
            else
                _condition = Formation.FACE_TO_FACE;
        }

    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 500, 50), _test.ToString(), InfoToModeratorStyle);

        if (_task > numberOfTasks)
        {
            GUI.Label(new Rect(10, 40, 500, 50), "ACABOU", InfoToModeratorStyle);
        }
        else if(_task > 0 && _task <= numberOfTasks)
        {
            int top = 40;
            int step = 35;
            if (_task == intermissionTask)
            {
                GUI.Label(new Rect(10, top, 500, 50), "INTERMISSION", InfoToModeratorStyle);
            }
            else
            {
                if (habituationTasks.Contains(_task))
                {
                    GUI.Label(new Rect(10, top, 500, 50), "[[[  HABITUATION TASK ]]]", InfoToModeratorStyle);
                    top += step; top += step;
                }

                GUI.Label(new Rect(10, top, 500, 50), "CONDITION: " + _condition, InfoToModeratorStyle); top += step; top += step;
                GUI.Label(new Rect(10, top, 500, 50), "ROLE: " + _participant, InfoToModeratorStyle); top += step; top += step;
                GUI.Label(new Rect(10, top, 500, 50), "TASK: " + _task, InfoToModeratorStyle); top += step; top += step;
                GUI.Label(new Rect(10, top, 500, 50), "TIME: " + Math.Round((Time.time - time), 1) + "s", InfoToModeratorStyle);
            }
        } 
    }
}
