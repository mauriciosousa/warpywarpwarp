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

    //public Transform humanGO;

    public CreepyTrackerIKSolver ikLeftArm;
    public CreepyTrackerIKSolver ikRightArm;

    //private List<Transform> _listOfChildren;
    //private Dictionary<BodyJointType, Transform> _bodyTrans;

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

    public TableTriggerInteractionZone interactionZone;

    public IKWarpInfo armsWarpInfo;

    [Space(20)]
    [Header("AT Evaluation Settings:")]
    public bool local = false;

    [Space(20)]
    [Header("Human:")]
    public Transform head;
    public Transform neck;
    public Transform spineShoulder;
    public Transform spineMid;
    public Transform spineBase;
    [Space(5)]
    public Transform rightShoulder;
    public Transform rightElbow;
    public Transform rightWrist;
    public Transform rightHand;
    public Transform rightHandTip;
    public Transform rightThumb;
    [Space(5)]
    public Transform leftShoulder;
    public Transform leftElbow;
    public Transform leftWrist;
    public Transform leftHand;
    public Transform leftHandTip;
    public Transform leftThumb;
    [Space(5)]
    public Transform rightHip;
    public Transform rightKnee;
    public Transform rightAnkle;
    public Transform rightFoot;
    [Space(5)]
    public Transform leftHip;
    public Transform leftKnee;
    public Transform leftAnkle;
    public Transform leftFoot;

    [Space(5)]
    [Header("Inverse Kinematics Targets:")]
    public Transform leftHandTipTarget;
    public Transform rightHandTipTarget;
    [Header("Hands Inside Interaction Area?")]
    public bool leftInside;
    public bool rightInside;


    void Start()
    {
        _humans = new Dictionary<string, Human>();
        //_listOfChildren = new List<Transform>();
        //_bodyTrans = new Dictionary<BodyJointType, Transform>();
        //_populateBodyTrans(humanGO);
        //_disassembleHierarchy(humanGO);
        //_associateHumanToJoints();

        _assembleHierarchy();
        //ikLeftArm.IsActive = doArmWarping;
        //ikRightArm.IsActive = doArmWarping;
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


            //_disassembleHierarchy(humanGO);
            //_updateHumanJoints(human.body.Joints);
            //_assembleHumanHierarchy(humanGO);

            //_disassembleHierarchy(humanGO);
            //foreach (BodyJointType joint in (BodyJointType[])Enum.GetValues(typeof(BodyJointType)))
            //{
            //    humanGO.Find(joint.ToString()).localPosition = human.body.Joints[joint];
            //}
            //_assembleHumanHierarchy(humanGO);


            _disassembleHierarchy();
            _updateHumanJoints(human.body.Joints);
            _assembleHierarchy();

            if (!local && doArmWarping)
            {


                armsWarpInfo.UpperArmDistance = UpperArmDistance;
                armsWarpInfo.ForearmDistance = ForearmDistance;
                armsWarpInfo.HandDistance = HandDistance;
                armsWarpInfo.debug = DebugBonesPC;

                _saveJointInfo(true);

                armsWarpInfo.leftWarping = interactionZone.isHandInside(leftHandTip.position);
                leftInside = armsWarpInfo.leftWarping;
                if (armsWarpInfo.leftWarping)
                {
                    interactionZone.CalcTargetPosition(leftHandTipTarget, leftHandTip);
                    ikLeftArm.Solve(true, leftHandTipTarget.position, lerpTime);
                }
                else
                {
                    ikLeftArm.IsActive = false;
                }

                armsWarpInfo.rightWarping = interactionZone.isHandInside(rightHandTip.position);
                rightInside = armsWarpInfo.rightWarping;
                if (armsWarpInfo.rightWarping)
                {
                    interactionZone.CalcTargetPosition(rightHandTipTarget, rightHandTip);
                    ikRightArm.Solve(true, rightHandTipTarget.position, lerpTime);
                }
                else
                {
                    ikRightArm.IsActive = false;
                }

                _saveJointInfo(false);
                //armsWarpInfo.Solve();
            }
        }
        _cleanDeadHumans();
    }

    private void _disassembleHierarchy()
    {
        Transform human = head.parent;

        rightShoulder.parent = human;
        rightElbow.parent = human;
        rightWrist.parent = human;
        rightHand.parent = human;
        rightHandTip.parent = human;

        leftShoulder.parent = human;
        leftElbow.parent = human;
        leftWrist.parent = human;
        leftHand.parent = human;
        leftHandTip.parent = human;
    }

    private void _assembleHierarchy()
    {
        rightHandTip.parent = rightHand;
        rightHand.parent = rightWrist;
        rightWrist.parent = rightElbow;
        rightElbow.parent = rightShoulder;

        leftHandTip.parent = leftHand;
        leftHand.parent = leftWrist;
        leftWrist.parent = leftElbow;
        leftElbow.parent = leftShoulder;
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

    //private void _getAllChildren(Transform trans)
    //{
    //    if (trans.childCount > 0)
    //    {
    //        foreach (Transform t in trans)
    //        {
    //            if (t.name != "Sphere")
    //            {
    //                _listOfChildren.Add(t);
    //                _getAllChildren(t);
    //            }
    //        }
    //    }
    //}

    //public void _disassembleHierarchy(Transform trans)
    //{
    //    _listOfChildren = new List<Transform>();
    //    if (trans.childCount > 0)
    //    {
    //        for (int i = 0; i < trans.childCount; i++)
    //        {
    //            _getAllChildren(trans.GetChild(0));
    //        }

    //        //foreach (Transform t in trans)
    //        //{
    //        //    _getAllChildren(t);
    //        //}
    //    }

    //    for (int i = 0; i < _listOfChildren.Count; i++)
    //    {
    //        _listOfChildren[i].parent = trans;
    //    }

    //    //foreach (Transform g in _listOfChildren)
    //    //{
    //    //    g.parent = trans;
    //    //}

    //    _listOfChildren.Add(trans.Find(BodyJointType.spineBase.ToString()));
    //}

    //private void _assembleHumanHierarchy(Transform trans)
    //{
    //    _bodyTrans[BodyJointType.spineMid].parent = _bodyTrans[BodyJointType.spineBase];
    //    _bodyTrans[BodyJointType.spineShoulder].parent = _bodyTrans[BodyJointType.spineMid];

    //    _bodyTrans[BodyJointType.neck].parent = _bodyTrans[BodyJointType.spineShoulder];
    //    _bodyTrans[BodyJointType.head].parent = _bodyTrans[BodyJointType.neck];

    //    _bodyTrans[BodyJointType.rightShoulder].parent = _bodyTrans[BodyJointType.spineShoulder];
    //    _bodyTrans[BodyJointType.rightElbow].parent = _bodyTrans[BodyJointType.rightShoulder];
    //    _bodyTrans[BodyJointType.rightWrist].parent = _bodyTrans[BodyJointType.rightElbow];
    //    _bodyTrans[BodyJointType.rightHand].parent = _bodyTrans[BodyJointType.rightWrist];
    //    _bodyTrans[BodyJointType.rightThumb].parent = _bodyTrans[BodyJointType.rightWrist];
    //    _bodyTrans[BodyJointType.rightHandTip].parent = _bodyTrans[BodyJointType.rightHand];

    //    _bodyTrans[BodyJointType.leftShoulder].parent = _bodyTrans[BodyJointType.spineShoulder];
    //    _bodyTrans[BodyJointType.leftElbow].parent = _bodyTrans[BodyJointType.leftShoulder];
    //    _bodyTrans[BodyJointType.leftWrist].parent = _bodyTrans[BodyJointType.leftElbow];
    //    _bodyTrans[BodyJointType.leftHand].parent = _bodyTrans[BodyJointType.leftWrist];
    //    _bodyTrans[BodyJointType.leftThumb].parent = _bodyTrans[BodyJointType.leftWrist];
    //    _bodyTrans[BodyJointType.leftHandTip].parent = _bodyTrans[BodyJointType.leftHand];

    //    _bodyTrans[BodyJointType.rightHip].parent = _bodyTrans[BodyJointType.spineBase];
    //    _bodyTrans[BodyJointType.rightKnee].parent = _bodyTrans[BodyJointType.rightHip];
    //    _bodyTrans[BodyJointType.rightAnkle].parent = _bodyTrans[BodyJointType.rightKnee];
    //    _bodyTrans[BodyJointType.rightFoot].parent = _bodyTrans[BodyJointType.rightAnkle];

    //    _bodyTrans[BodyJointType.leftHip].parent = _bodyTrans[BodyJointType.spineBase];
    //    _bodyTrans[BodyJointType.leftKnee].parent = _bodyTrans[BodyJointType.leftHip];
    //    _bodyTrans[BodyJointType.leftAnkle].parent = _bodyTrans[BodyJointType.leftKnee];
    //    _bodyTrans[BodyJointType.leftFoot].parent = _bodyTrans[BodyJointType.leftAnkle];
    //}

    //private void _populateBodyTrans(Transform humanGO)
    //{
    //    _bodyTrans.Clear();
    //    _disassembleHierarchy(humanGO);
    //    foreach (BodyJointType joint in (BodyJointType[])Enum.GetValues(typeof(BodyJointType)))
    //    {
    //            _bodyTrans[joint] = _getTransformOfJoint(joint);
    //    }
    //    _assembleHumanHierarchy(humanGO);
    //}

    //private Transform _getTransformOfJoint(BodyJointType joint)
    //{
    //    Transform ret = null;

    //    foreach (Transform t in _listOfChildren)
    //    {
    //        if (t.gameObject.name == joint.ToString())
    //        {
    //            ret = t; break;
    //        }
    //    }
    //    return ret;
    //}

    private void _saveJointInfo(bool isBeforeIK)
    {
        if (local) return;

        try
        {
            if (isBeforeIK)
            {
                armsWarpInfo.LEFT_OriginalShoulder = leftShoulder.position;
                armsWarpInfo.LEFT_OriginalElbow = leftElbow.position;
                armsWarpInfo.LEFT_OriginalWrist = leftWrist.position;
                armsWarpInfo.LEFT_OriginalHandTip = leftHandTip.position;

                armsWarpInfo.RIGHT_OriginalShoulder = rightShoulder.position;
                armsWarpInfo.RIGHT_OriginalElbow = rightElbow.position;
                armsWarpInfo.RIGHT_OriginalWrist = rightWrist.position;
                armsWarpInfo.RIGHT_OriginalHandTip = rightHandTip.position;
            }
            else
            {
                armsWarpInfo.LEFT_IKShoulder = leftShoulder.position;
                armsWarpInfo.LEFT_IKElbow = leftElbow.position;
                armsWarpInfo.LEFT_IKWrist = leftWrist.position;
                armsWarpInfo.LEFT_IKHandTip = leftHandTip.position;

                armsWarpInfo.RIGHT_IKShoulder = rightShoulder.position;
                armsWarpInfo.RIGHT_IKElbow = rightElbow.position;
                armsWarpInfo.RIGHT_IKWrist = rightWrist.position;
                armsWarpInfo.RIGHT_IKHandTip = rightHandTip.position;
            }
        }
        catch (Exception)
        {
            // lol 
        }
    }

    [Space(20)]
    public Transform headPivot;
    public void calibrateHuman(SetupLocation location)
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
            if (location == SetupLocation.RIGHT)
            {
                headPivot.transform.forward = -headPivot.transform.forward;
            }


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
    


    //private void _associateHumanToJoints()
    //{
    //    spineBase = humanGO.Find(BodyJointType.spineBase.ToString());
    //    spineMid = humanGO.Find(BodyJointType.spineMid.ToString());
    //    spineShoulder = humanGO.Find(BodyJointType.spineShoulder.ToString());
    //    neck = humanGO.Find(BodyJointType.neck.ToString());
    //    head = humanGO.Find(BodyJointType.head.ToString());

    //    rightShoulder = humanGO.Find(BodyJointType.rightShoulder.ToString());
    //    rightElbow = humanGO.Find(BodyJointType.rightElbow.ToString());
    //    rightWrist = humanGO.Find(BodyJointType.rightWrist.ToString());
    //    rightHand = humanGO.Find(BodyJointType.rightHand.ToString());
    //    rightHandTip = humanGO.Find(BodyJointType.rightHandTip.ToString());

    //    leftShoulder = humanGO.Find(BodyJointType.leftShoulder.ToString());
    //    leftElbow = humanGO.Find(BodyJointType.leftElbow.ToString());
    //    leftWrist = humanGO.Find(BodyJointType.leftWrist.ToString());
    //    leftHand = humanGO.Find(BodyJointType.leftHand.ToString());
    //    leftHandTip = humanGO.Find(BodyJointType.leftHandTip.ToString());
    //}

    private void _updateHumanJoints(Dictionary<BodyJointType, Vector3> joints)
    {
        spineBase.localPosition = human.body.Joints[BodyJointType.spineBase];
        head.localPosition = human.body.Joints[BodyJointType.head];

        rightShoulder.localPosition =   human.body.Joints[BodyJointType.rightShoulder];
        rightElbow.localPosition =      human.body.Joints[BodyJointType.rightElbow];
        rightWrist.localPosition =      human.body.Joints[BodyJointType.rightWrist];
        rightHand.localPosition =       human.body.Joints[BodyJointType.rightHand];
        rightHandTip.localPosition =    human.body.Joints[BodyJointType.rightHandTip];

        leftShoulder.localPosition =    human.body.Joints[BodyJointType.leftShoulder];
        leftElbow.localPosition =       human.body.Joints[BodyJointType.leftElbow];
        leftWrist.localPosition =       human.body.Joints[BodyJointType.leftWrist];
        leftHand.localPosition =        human.body.Joints[BodyJointType.leftHand];
        leftHandTip.localPosition =     human.body.Joints[BodyJointType.leftHandTip];

        if (!local)
        {
            neck.localPosition = human.body.Joints[BodyJointType.neck];
            spineShoulder.localPosition = human.body.Joints[BodyJointType.spineShoulder];
            spineMid.localPosition = human.body.Joints[BodyJointType.spineMid];

            rightHip.localPosition = human.body.Joints[BodyJointType.rightHip];
            rightKnee.localPosition = human.body.Joints[BodyJointType.rightKnee];
            rightAnkle.localPosition = human.body.Joints[BodyJointType.rightKnee];
            rightFoot.localPosition = human.body.Joints[BodyJointType.rightFoot];
            rightThumb.localPosition = human.body.Joints[BodyJointType.rightThumb];

            leftHip.localPosition = human.body.Joints[BodyJointType.leftHip];
            leftKnee.localPosition = human.body.Joints[BodyJointType.leftKnee];
            leftAnkle.localPosition = human.body.Joints[BodyJointType.leftKnee];
            leftFoot.localPosition = human.body.Joints[BodyJointType.leftFoot];
            leftThumb.localPosition = human.body.Joints[BodyJointType.leftThumb];

        }
    }

    private Vector3 _mirrorJoint(Vector3 p)
    {
        return new Vector3(-p.x, p.y, p.z);
    }
}
