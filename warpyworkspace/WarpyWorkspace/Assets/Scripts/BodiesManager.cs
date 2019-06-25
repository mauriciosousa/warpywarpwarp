using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodiesManager : MonoBehaviour
{

    private Dictionary<string, Human> _humans;
    private bool _humanLocked = false;
    public Human human = null;

    public Transform CenterObject;

    public Vector3 getHeadPosition(out bool canApplyHeadPosition)
    {
        canApplyHeadPosition = false;

        if (human != null)
        {
            canApplyHeadPosition = true;
            return transform.position = human.body.Joints[BodyJointType.head];
        }

        return Vector3.zero;
    }

    public Transform humanGO;


    public CreepyTrackerIKSolver ikLeftArm;
    public CreepyTrackerIKSolver ikRightArm;

    private List<Transform> _listOfChildren;
    private Dictionary<BodyJointType, Transform> _bodyTrans;


    [Space(20)]
    public Transform LeftShoulder;
    public Transform LeftElbow;
    public Transform LeftWrist;
    public Transform LeftHandTip;
    [Space(10)]
    public Transform RightShoulder;
    public Transform RightElbow;
    public Transform RightWrist;
    public Transform RightHandTip;

    //[Space(10)]
    //[Range(0.01f, 0.5f)]
    private float UpperArmDistance = 0.1f;
    //[Range(0.01f, 0.5f)]
    private float ForearmDistance = 0.1f;
    //[Range(0.01f, 0.5f)]
    private float HandDistance = 0.1f;

    [Space(20)]
    [Header("Body Warping Settings:")]
    public bool doArmWarping;
    public bool DebugBonesPC = false;
    [Range(0, 1)]
    public float lerpTime = 0.2f;


    public IKWarpInfo armsWarpInfo;

    public Transform leftTarget;
    public Transform rightTarget;

    [Space(20)]
    [Header("AT Evaluation Settings:")]
    public bool local = false;

    [Space(20)]
    public Transform spineBase;
    public Transform spineMid;
    public Transform spineShoulder;
    public Transform neck;
    public Transform head;

    private Transform rightShoulder;
    private Transform rightElbow;
    private Transform rightWrist;
    private Transform rightHand;
    private Transform rightHandTip;

    private Transform leftShoulder;
    private Transform leftElbow;
    private Transform leftWrist;
    private Transform leftHand;
    private Transform leftHandTip;

    void Start()
    {
        _humans = new Dictionary<string, Human>();
        _listOfChildren = new List<Transform>();
        _bodyTrans = new Dictionary<BodyJointType, Transform>();
        _populateBodyTrans(humanGO);
        _disassembleHierarchy(humanGO);
        _associateHumanToJoints();
        ikLeftArm.IsActive = doArmWarping;
        ikRightArm.IsActive = doArmWarping;
        armsWarpInfo = new IKWarpInfo();
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    _disassembleHierarchy(humanGO);
        //}

        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    _assembleHumanHierarchy(humanGO);
        //}

        if (_humans.Count > 0)
        {
            if (human != null && _humanLocked && _humans.ContainsKey(human.id))
            {
                human = _humans[human.id];
            }
            else
            {
                _humanLocked = false;
                Human newHuman = null;
                foreach (Human h in _humans.Values)
                {
                    if (newHuman == null)
                    {
                        newHuman = h;
                    }
                    else
                    {
                        if (CenterObject.gameObject != null && Vector3.Distance(h.body.Joints[BodyJointType.head], CenterObject.position) < Vector3.Distance(newHuman.body.Joints[BodyJointType.head], CenterObject.position))
                        {
                            newHuman = h;
                        }
                    }
                }
                human = newHuman;
            }


            _disassembleHierarchy(humanGO);
            _updateHumanJoints(human.body.Joints);
            _assembleHumanHierarchy(humanGO);

            //_disassembleHierarchy(humanGO);
            //foreach (BodyJointType joint in (BodyJointType[])Enum.GetValues(typeof(BodyJointType)))
            //{
            //    humanGO.Find(joint.ToString()).localPosition = human.body.Joints[joint];
            //}
            //_assembleHumanHierarchy(humanGO);







            //armsWarpInfo.warping = doArmWarping;
            //armsWarpInfo.UpperArmDistance = UpperArmDistance;
            //armsWarpInfo.ForearmDistance = ForearmDistance;
            //armsWarpInfo.HandDistance = HandDistance;
            //armsWarpInfo.debug = DebugBonesPC;

            //_saveJointInfo(true); --- uncomment
        
        //----ikLeftArm.Solve(doArmWarping, leftTarget.position, lerpTime);
            //----ikRightArm.Solve(doArmWarping, rightTarget.position, lerpTime);
        
        //_saveJointInfo(false); -- uncomment
            
        //armsWarpInfo.Solve();
        }
        _cleanDeadHumans();
    }



    public void setNewFrame(Body[] bodies)
    {
        foreach (Body b in bodies)
        {
            try
            {
                string bodyID = b.Properties[BodyPropertiesType.UID];

                if (!_humans.ContainsKey(bodyID))
                {
                    _humans.Add(bodyID, new Human());
                }
                _humans[bodyID].Update(b);
            }
            catch (Exception e) {
                //Debug.Log(e.Message);
            }
        }
    }

    void _cleanDeadHumans()
    {
        List<Human> deadhumans = new List<Human>();

        foreach (Human h in _humans.Values)
        {
            if (DateTime.Now > h.lastUpdated.AddMilliseconds(1000))
                deadhumans.Add(h);
        }

        foreach (Human h in deadhumans)
        {
            _humans.Remove(h.id);
        }

        deadhumans = new List<Human>();
    }

    private void _getAllChildren(Transform trans)
    {
        if (trans.childCount > 0)
        {
            foreach (Transform t in trans)
            {
                if (t.name != "Sphere")
                {
                    _listOfChildren.Add(t);
                    _getAllChildren(t);
                }
            }
        }
    }

    public void _disassembleHierarchy(Transform trans)
    {
        _listOfChildren = new List<Transform>();
        if (trans.childCount > 0)
        {
            for (int i = 0; i < trans.childCount; i++)
            {
                _getAllChildren(trans.GetChild(0));
            }

            //foreach (Transform t in trans)
            //{
            //    _getAllChildren(t);
            //}
        }

        for (int i = 0; i < _listOfChildren.Count; i++)
        {
            _listOfChildren[i].parent = trans;
        }

        //foreach (Transform g in _listOfChildren)
        //{
        //    g.parent = trans;
        //}

        _listOfChildren.Add(trans.Find(BodyJointType.spineBase.ToString()));
    }

    private void _assembleHumanHierarchy(Transform trans)
    {
        _bodyTrans[BodyJointType.spineMid].parent = _bodyTrans[BodyJointType.spineBase];
        _bodyTrans[BodyJointType.spineShoulder].parent = _bodyTrans[BodyJointType.spineMid];

        _bodyTrans[BodyJointType.neck].parent = _bodyTrans[BodyJointType.spineShoulder];
        _bodyTrans[BodyJointType.head].parent = _bodyTrans[BodyJointType.neck];

        _bodyTrans[BodyJointType.rightShoulder].parent = _bodyTrans[BodyJointType.spineShoulder];
        _bodyTrans[BodyJointType.rightElbow].parent = _bodyTrans[BodyJointType.rightShoulder];
        _bodyTrans[BodyJointType.rightWrist].parent = _bodyTrans[BodyJointType.rightElbow];
        _bodyTrans[BodyJointType.rightHand].parent = _bodyTrans[BodyJointType.rightWrist];
        _bodyTrans[BodyJointType.rightThumb].parent = _bodyTrans[BodyJointType.rightWrist];
        _bodyTrans[BodyJointType.rightHandTip].parent = _bodyTrans[BodyJointType.rightHand];

        _bodyTrans[BodyJointType.leftShoulder].parent = _bodyTrans[BodyJointType.spineShoulder];
        _bodyTrans[BodyJointType.leftElbow].parent = _bodyTrans[BodyJointType.leftShoulder];
        _bodyTrans[BodyJointType.leftWrist].parent = _bodyTrans[BodyJointType.leftElbow];
        _bodyTrans[BodyJointType.leftHand].parent = _bodyTrans[BodyJointType.leftWrist];
        _bodyTrans[BodyJointType.leftThumb].parent = _bodyTrans[BodyJointType.leftWrist];
        _bodyTrans[BodyJointType.leftHandTip].parent = _bodyTrans[BodyJointType.leftHand];

        _bodyTrans[BodyJointType.rightHip].parent = _bodyTrans[BodyJointType.spineBase];
        _bodyTrans[BodyJointType.rightKnee].parent = _bodyTrans[BodyJointType.rightHip];
        _bodyTrans[BodyJointType.rightAnkle].parent = _bodyTrans[BodyJointType.rightKnee];
        _bodyTrans[BodyJointType.rightFoot].parent = _bodyTrans[BodyJointType.rightAnkle];

        _bodyTrans[BodyJointType.leftHip].parent = _bodyTrans[BodyJointType.spineBase];
        _bodyTrans[BodyJointType.leftKnee].parent = _bodyTrans[BodyJointType.leftHip];
        _bodyTrans[BodyJointType.leftAnkle].parent = _bodyTrans[BodyJointType.leftKnee];
        _bodyTrans[BodyJointType.leftFoot].parent = _bodyTrans[BodyJointType.leftAnkle];
    }

    private void _populateBodyTrans(Transform humanGO)
    {
        _bodyTrans.Clear();
        _disassembleHierarchy(humanGO);
        foreach (BodyJointType joint in (BodyJointType[])Enum.GetValues(typeof(BodyJointType)))
        {
                _bodyTrans[joint] = _getTransformOfJoint(joint);
        }
        _assembleHumanHierarchy(humanGO);
    }

    private Transform _getTransformOfJoint(BodyJointType joint)
    {
        Transform ret = null;

        foreach (Transform t in _listOfChildren)
        {
            if (t.gameObject.name == joint.ToString())
            {
                ret = t; break;
            }
        }
        return ret;
    }

    private void _saveJointInfo(bool isBeforeIK)
    {
        if (local) return;

        try
        {
            if (isBeforeIK)
            {
                armsWarpInfo.LEFT_OriginalShoulder = LeftShoulder.position;
                armsWarpInfo.LEFT_OriginalElbow = LeftElbow.position;
                armsWarpInfo.LEFT_OriginalWrist = LeftWrist.position;
                armsWarpInfo.LEFT_OriginalHandTip = LeftHandTip.position;

                armsWarpInfo.RIGHT_OriginalShoulder = RightShoulder.position;
                armsWarpInfo.RIGHT_OriginalElbow = RightElbow.position;
                armsWarpInfo.RIGHT_OriginalWrist = RightWrist.position;
                armsWarpInfo.RIGHT_OriginalHandTip = RightHandTip.position;
            }
            else
            {
                armsWarpInfo.LEFT_IKShoulder = LeftShoulder.position;
                armsWarpInfo.LEFT_IKElbow = LeftElbow.position;
                armsWarpInfo.LEFT_IKWrist = LeftWrist.position;
                armsWarpInfo.LEFT_IKHandTip = LeftHandTip.position;

                armsWarpInfo.RIGHT_IKShoulder = RightShoulder.position;
                armsWarpInfo.RIGHT_IKElbow = RightElbow.position;
                armsWarpInfo.RIGHT_IKWrist = RightWrist.position;
                armsWarpInfo.RIGHT_IKHandTip = RightHandTip.position;
            }
        }
        catch (Exception)
        {
            // lol 
        }
    }

    public void calibrateHuman()
    {
        //Human h = getHumanWithHandUp();
        //if (h == null)
        //{
        //    throw new Exception("Cannot find that human!");
        //}
        //else
        //{
        //    UnityEngine.XR.InputTracking.Recenter();
        //    human = h;
        //}

        if (human != null)
        {
            UnityEngine.XR.InputTracking.Recenter();
            print("HUMAN RECENTER DONE");
        }
        else
        {
            Debug.LogError("No human to calibrate");
        }
    }

    public Human getHumanWithHandUp()
    {

        string id = _getHumanIdWithHandUp();
        foreach (KeyValuePair<String, Human> h in _humans)
        {
            if (h.Key == id) return h.Value;
        }
        return null;
    }

    private string _getHumanIdWithHandUp()
    {
        foreach (Human h in _humans.Values)
        {
            if (h.body.Joints[BodyJointType.leftHand].y > h.body.Joints[BodyJointType.head].y ||
                h.body.Joints[BodyJointType.rightHand].y > h.body.Joints[BodyJointType.head].y)
            {
                return h.id;
            }
        }
        return string.Empty;
    }
    


    private void _associateHumanToJoints()
    {
        spineBase = humanGO.Find(BodyJointType.spineBase.ToString());
        spineMid = humanGO.Find(BodyJointType.spineMid.ToString());
        spineShoulder = humanGO.Find(BodyJointType.spineShoulder.ToString());
        neck = humanGO.Find(BodyJointType.neck.ToString());
        head = humanGO.Find(BodyJointType.head.ToString());

        rightShoulder = humanGO.Find(BodyJointType.rightShoulder.ToString());
        rightElbow = humanGO.Find(BodyJointType.rightElbow.ToString());
        rightWrist = humanGO.Find(BodyJointType.rightWrist.ToString());
        rightHand = humanGO.Find(BodyJointType.rightHand.ToString());
        rightHandTip = humanGO.Find(BodyJointType.rightHandTip.ToString());

        leftShoulder = humanGO.Find(BodyJointType.leftShoulder.ToString());
        leftElbow = humanGO.Find(BodyJointType.leftElbow.ToString());
        leftWrist = humanGO.Find(BodyJointType.leftWrist.ToString());
        leftHand = humanGO.Find(BodyJointType.leftHand.ToString());
        leftHandTip = humanGO.Find(BodyJointType.leftHandTip.ToString());
    }

    private void _updateHumanJoints(Dictionary<BodyJointType, Vector3> joints)
    {
        spineBase.localPosition = human.body.Joints[BodyJointType.spineBase];
        spineMid.localPosition = human.body.Joints[BodyJointType.spineMid];
        spineShoulder.localPosition = human.body.Joints[BodyJointType.spineShoulder];
        neck.localPosition = human.body.Joints[BodyJointType.neck];
        head.localPosition = human.body.Joints[BodyJointType.head];

        rightShoulder.localPosition = human.body.Joints[BodyJointType.rightShoulder];
        rightElbow.localPosition = human.body.Joints[BodyJointType.rightElbow];
        rightWrist.localPosition = human.body.Joints[BodyJointType.rightWrist];
        rightHand.localPosition = human.body.Joints[BodyJointType.rightHand];
        rightHandTip.localPosition = human.body.Joints[BodyJointType.rightHandTip];

        leftShoulder.localPosition = human.body.Joints[BodyJointType.leftShoulder];
        leftElbow.localPosition = human.body.Joints[BodyJointType.leftElbow];
        leftWrist.localPosition = human.body.Joints[BodyJointType.leftWrist];
        leftHand.localPosition = human.body.Joints[BodyJointType.leftHand];
        leftHandTip.localPosition = human.body.Joints[BodyJointType.leftHandTip];
    }

}
