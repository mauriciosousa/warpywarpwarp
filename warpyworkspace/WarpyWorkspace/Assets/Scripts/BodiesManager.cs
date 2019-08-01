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
    //[Header("Arm Segmentation Settings:")]
    //[Range(0.01f, 0.5f)]
    //public float UpperArmDistance = 0.1f;
    //[Range(0.01f, 0.5f)]
    //public float ForearmDistance = 0.1f;
    //[Range(0.01f, 0.5f)]
    //public float HandDistance = 0.1f;

    [Space(20)]
    [Header("Body Warping Settings:")]
    public bool DISABLEWARP = false;
    public bool DISABLE_CALC_TARGET = false;
    public bool doArmWarping;
    public bool DebugBonesPC = false;
    public EvaluationProceadure evaluation;

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
    private OneEuroFilter<Vector3> _rightElbowFilter;
    public Transform rightWrist;
    private OneEuroFilter<Vector3> _rightWristFilter;
    public Transform rightHand;
    private OneEuroFilter<Vector3> _rightHandFilter;
    public Transform rightHandTip;
    private OneEuroFilter<Vector3> _rightHandTipFilter;
    public Transform rightThumb;
    [Space(5)]
    public Transform leftShoulder;
    public Transform leftElbow;
    private OneEuroFilter<Vector3> _leftElbowFilter;
    public Transform leftWrist;
    private OneEuroFilter<Vector3> _leftWristFilter;
    public Transform leftHand;
    private OneEuroFilter<Vector3> _leftHandFilter;
    public Transform leftHandTip;
    private OneEuroFilter<Vector3> _leftHandTipFilter;
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
    public Transform LEGBONE;
    [Space(5)]
    public Transform rightIKShoulder;
    public Transform rightIKElbow;
    public Transform rightIKWrist;
    public Transform rightIKHandTip;
    [Space(5)]
    public Transform leftIKShoulder;
    public Transform leftIKElbow;
    public Transform leftIKWrist;
    public Transform leftIKHandTip;

    private Vector3 _leftElbowLastPosition      = Vector3.zero;
    private Vector3 _leftWristLastPosition      = Vector3.zero;
    private Vector3 _leftHandTipLastPosition    = Vector3.zero;
    private Vector3 _rightElbowLastPosition     = Vector3.zero;
    private Vector3 _rightWristLastPosition     = Vector3.zero;
    private Vector3 _rightHandTipLastPosition   = Vector3.zero;

    [Space(5)]
    [Header("Inverse Kinematics Targets:")]
    public Transform leftHandTipTarget;
    public Transform rightHandTipTarget;
    [Header("Hands Inside Interaction Area?")]
    public bool leftInside;
    public bool rightInside;



    [Space(5)]
    [Header("Lerp Fractions to match IK Joints")]
    [Range(0, 1)]
    public float targetLerpFrac = 0.2f;
    [Range(0, 1)]
    public float elbowLerpFrac = 0.2f;
    [Range(0, 1)]
    public float wristLerpFrac = 0.2f;
    [Range(0, 1)]
    public float handTipLerpFrac = 0.2f;

    [Space(5)]
    [Header("Remove Head Settings:")]
    public bool removeHead = false;
    [Range(0.01f, 0.2f)]
    public float Y_HeadOffset = 0.1f;
    [Range(0.01f, 0.5f)]
    public float HeadSize = 0.25f;

    [Space(5)]
    [Header("OneEuroFilter Params:")]
    public bool filterActive = false;
    public float freq = 1.0f;
    public float mincutoff = 1.0f;
    public float beta = 0.001f;
    public float dcutoff = 1.0f;

    void Start()
    {
        _humans = new Dictionary<string, Human>();

        _assembleHierarchy();

        armsWarpInfo = new IKWarpInfo();

        armsWarpInfo.LeftElbow =    leftElbow;
        armsWarpInfo.LeftWrist =    leftWrist;
        armsWarpInfo.LeftHandTip =  leftHandTip;
        armsWarpInfo.RightElbow =   rightElbow;
        armsWarpInfo.RightWrist =   rightWrist;
        armsWarpInfo.RightHandTip = rightHandTip;

        _rightElbowFilter = new OneEuroFilter<Vector3>(freq, mincutoff, beta, dcutoff);
        _rightWristFilter = new OneEuroFilter<Vector3>(freq, mincutoff, beta, dcutoff);
        _rightHandFilter = new OneEuroFilter<Vector3>(freq, mincutoff, beta, dcutoff);
        _rightHandTipFilter = new OneEuroFilter<Vector3>(freq, mincutoff, beta, dcutoff);
        _leftElbowFilter = new OneEuroFilter<Vector3>(freq, mincutoff, beta, dcutoff);
        _leftWristFilter = new OneEuroFilter<Vector3>(freq, mincutoff, beta, dcutoff);
        _leftHandFilter = new OneEuroFilter<Vector3>(freq, mincutoff, beta, dcutoff);
        _leftHandTipFilter = new OneEuroFilter<Vector3>(freq, mincutoff, beta, dcutoff);
    }

    void Update()
    {

        if (_humans.Count > 0)
        {
            _updateFilterParams();

            //if (human != null && _humanLocked && _humans.ContainsKey(human.id))
            //{
            //    human = _humans[human.id];
            //}
            //else
            //{
            //    _humanLocked = false;
            //    Human newHuman = null;
            //    foreach (Human h in _humans.Values)
            //    {
            //        if (newHuman == null)
            //        {
            //            newHuman = h;
            //        }
            //        else
            //        {
            //            if (CenterObject.gameObject != null && Vector3.Distance(h.body.Joints[BodyJointType.head], CenterObject.position) < Vector3.Distance(newHuman.body.Joints[BodyJointType.head], CenterObject.position))
            //            {
            //                newHuman = h;
            //            }
            //        }
            //    }
            //    human = newHuman;
            //}


            if (human != null && _humans.ContainsKey(human.id))
            {
                human = _humans[human.id];
            }
            else
            {
                //Human newHuman = null;
                //foreach (Human h in _humans.Values)
                //{
                //    if (newHuman == null)
                //    {
                //        newHuman = h;
                //    }
                //    else
                //    {
                //        if (CenterObject.gameObject != null && Vector3.Distance(h.body.Joints[BodyJointType.head], CenterObject.position) < Vector3.Distance(newHuman.body.Joints[BodyJointType.head], CenterObject.position))
                //        {
                //            newHuman = h;
                //        }
                //    }
                //}
                //human = newHuman;

                Human newHuman = null;

                float distance = float.PositiveInfinity;
                foreach (Human h in _humans.Values)
                {
                    float d = Vector3.Distance(h.body.Joints[BodyJointType.head], CenterObject.position);

                    if (newHuman == null || d < distance)
                    {
                        newHuman = h;
                        distance = d;
                    }
                }
                human = newHuman;
            }



            _disassembleHierarchy();
            _updateHumanJoints(human.body.Joints);
            _assembleHierarchy();

            armsWarpInfo.VRHead = head.position;
            armsWarpInfo.headSize = HeadSize;
            armsWarpInfo.removeHead = removeHead;
            armsWarpInfo.Y_HeadOffset = Y_HeadOffset;
            
            if (!local && !DISABLEWARP && doArmWarping)// && evaluation.evalState == EvalState.SESSION)
            {
                LEGBONE.position = spineBase.position + (-transform.up);

                armsWarpInfo.debug = DebugBonesPC;

                _saveJointInfo(true);

                // LEFT ARM IK
                armsWarpInfo.leftWarping = interactionZone.isHandInside(leftHandTip.position);
                leftInside = armsWarpInfo.leftWarping;
                if (!DISABLE_CALC_TARGET && leftInside) interactionZone.CalcTargetPosition(leftHandTipTarget, leftHandTip);

                ikLeftArm.Solve(armsWarpInfo.leftWarping, leftHandTipTarget.position, targetLerpFrac);
                if (armsWarpInfo.leftWarping && leftHandTip.localPosition.x != float.NaN)
                {
                    try { _applyLerpToArm(true); } catch { /*lol*/ }
                }
                else
                {
                    _applyNonIKLerpToArm(true);

                    //_leftElbowLastPosition = Vector3.zero;
                    //_leftWristLastPosition = Vector3.zero;
                    //_leftHandTipLastPosition = Vector3.zero;
                }

                // RIGHT ARM IK
                armsWarpInfo.rightWarping = interactionZone.isHandInside(rightHandTip.position);
                rightInside = armsWarpInfo.rightWarping;
                if (!DISABLE_CALC_TARGET && rightInside) interactionZone.CalcTargetPosition(rightHandTipTarget, rightHandTip);

                ikRightArm.Solve(armsWarpInfo.rightWarping, rightHandTipTarget.position, targetLerpFrac);
                if (armsWarpInfo.rightWarping && rightHandTip.localPosition.x != float.NaN)
                {
                    try { _applyLerpToArm(false); } catch { /*lol*/ }
                }
                else
                {
                    _applyNonIKLerpToArm(false);

                    //_rightElbowLastPosition = Vector3.zero;
                    //_rightWristLastPosition = Vector3.zero;
                    //_rightHandTipLastPosition = Vector3.zero;
                }

                _saveJointInfo(false);

                armsWarpInfo.Solve();
            }
            else if (!local && doArmWarping)
            {
                armsWarpInfo.reset();
            }
            
            if (local)
            {
                armsWarpInfo.leftWarping = false;
                armsWarpInfo.rightWarping = false;
                armsWarpInfo.removeHead = true;
            }
        }
        _cleanDeadHumans();
    }

    private void _updateFilterParams()
    {
        _rightElbowFilter.UpdateParams(freq, mincutoff, beta, dcutoff);
        _rightWristFilter.UpdateParams(freq, mincutoff, beta, dcutoff);
        _rightHandFilter.UpdateParams(freq, mincutoff, beta, dcutoff);
        _rightHandTipFilter.UpdateParams(freq, mincutoff, beta, dcutoff);
        _leftElbowFilter.UpdateParams(freq, mincutoff, beta, dcutoff);
        _leftWristFilter.UpdateParams(freq, mincutoff, beta, dcutoff);
        _leftHandFilter.UpdateParams(freq, mincutoff, beta, dcutoff);
        _leftHandTipFilter.UpdateParams(freq, mincutoff, beta, dcutoff);
    }

    private void _applyLerpToArm(bool leftArm)
    {
        Transform human = head.parent;
        if (leftArm)
        {
            leftElbow.parent = human;
            leftWrist.parent = human;
            leftHand.parent = human;
            leftHandTip.parent = human;

            leftIKElbow.parent = human;
            leftIKWrist.parent = human;
            leftIKHandTip.parent = human;

            if (_leftElbowLastPosition == Vector3.zero)
            {
                _leftElbowLastPosition = leftElbow.localPosition;
                _leftWristLastPosition = leftWrist.localPosition;
                _leftHandTipLastPosition = leftHandTip.localPosition;
            }

            leftElbow.localPosition = Vector3.Lerp(_leftElbowLastPosition, leftIKElbow.localPosition, elbowLerpFrac);
            leftWrist.localPosition = Vector3.Lerp(_leftWristLastPosition, leftIKWrist.localPosition, wristLerpFrac);
            leftHandTip.localPosition = Vector3.Lerp(_leftHandTipLastPosition, leftIKHandTip.localPosition, handTipLerpFrac);

            _leftElbowLastPosition = leftElbow.localPosition;
            _leftWristLastPosition = leftWrist.localPosition;
            _leftHandTipLastPosition = leftHandTip.localPosition;

            leftHandTip.parent = leftHand;
            leftHand.parent = leftWrist;
            leftWrist.parent = leftElbow;
            leftElbow.parent = leftShoulder;

            leftIKHandTip.parent = leftIKWrist;
            leftIKWrist.parent = leftIKElbow;
            leftIKElbow.parent = leftIKShoulder;
        }
        else
        {
            rightElbow.parent = human;
            rightWrist.parent = human;
            rightHand.parent = human;
            rightHandTip.parent = human;

            rightIKElbow.parent = human;
            rightIKWrist.parent = human;
            rightIKHandTip.parent = human;

            if (_rightElbowLastPosition == Vector3.zero)
            {
                _rightElbowLastPosition = rightElbow.localPosition;
                _rightWristLastPosition = rightWrist.localPosition;
                _rightHandTipLastPosition = rightHandTip.localPosition;
            }

            rightElbow.localPosition = Vector3.Lerp(_rightElbowLastPosition, rightIKElbow.localPosition, elbowLerpFrac);
            rightWrist.localPosition = Vector3.Lerp(_rightWristLastPosition, rightIKWrist.localPosition, wristLerpFrac);
            rightHandTip.localPosition = Vector3.Lerp(_rightHandTipLastPosition, rightIKHandTip.localPosition, handTipLerpFrac);

            _rightElbowLastPosition = rightElbow.localPosition;
            _rightWristLastPosition = rightWrist.localPosition;
            _rightHandTipLastPosition = rightHandTip.localPosition;

            rightHandTip.parent = rightHand;
            rightHand.parent = rightWrist;
            rightWrist.parent = rightElbow;
            rightElbow.parent = rightShoulder;

            rightIKHandTip.parent = rightIKWrist;
            rightIKWrist.parent = rightIKElbow;
            rightIKElbow.parent = rightIKShoulder;
        }
    }

    private void _applyNonIKLerpToArm(bool leftArm)
    {
        Transform human = head.parent;
        if (leftArm)
        {
            leftElbow.parent = human;
            leftWrist.parent = human;
            leftHand.parent = human;
            leftHandTip.parent = human;

            leftElbow.localPosition = Vector3.Lerp(_leftElbowLastPosition, leftElbow.localPosition, elbowLerpFrac);
            leftWrist.localPosition = Vector3.Lerp(_leftWristLastPosition, leftWrist.localPosition, wristLerpFrac);
            leftHandTip.localPosition = Vector3.Lerp(_leftHandTipLastPosition, leftHandTip.localPosition, handTipLerpFrac);

            _leftElbowLastPosition = leftElbow.localPosition;
            _leftWristLastPosition = leftWrist.localPosition;
            _leftHandTipLastPosition = leftHandTip.localPosition;

            leftHandTip.parent = leftHand;
            leftHand.parent = leftWrist;
            leftWrist.parent = leftElbow;
            leftElbow.parent = leftShoulder;
        }
        else
        {
            rightElbow.parent = human;
            rightWrist.parent = human;
            rightHand.parent = human;
            rightHandTip.parent = human;

            rightElbow.localPosition = Vector3.Lerp(_rightElbowLastPosition, rightElbow.localPosition, elbowLerpFrac);
            rightWrist.localPosition = Vector3.Lerp(_rightWristLastPosition, rightWrist.localPosition, wristLerpFrac);
            rightHandTip.localPosition = Vector3.Lerp(_rightHandTipLastPosition, rightHandTip.localPosition, handTipLerpFrac);

            _rightElbowLastPosition = rightElbow.localPosition;
            _rightWristLastPosition = rightWrist.localPosition;
            _rightHandTipLastPosition = rightHandTip.localPosition;

            rightHandTip.parent = rightHand;
            rightHand.parent = rightWrist;
            rightWrist.parent = rightElbow;
            rightElbow.parent = rightShoulder;
        }
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

        if (!local)
        {
            rightIKShoulder.parent = human;
            rightIKElbow.parent = human;
            rightIKWrist.parent = human;
            rightIKHandTip.parent = human;

            leftIKShoulder.parent = human;
            leftIKElbow.parent = human;
            leftIKWrist.parent = human;
            leftIKHandTip.parent = human;
        }
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

        if (!local)
        {
            rightIKHandTip.parent = rightIKWrist;
            rightIKWrist.parent = rightIKElbow;
            rightIKElbow.parent = rightIKShoulder;

            leftIKHandTip.parent = leftIKWrist;
            leftIKWrist.parent = leftIKElbow;
            leftIKElbow.parent = leftIKShoulder;
        }
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

//            transform.rotation = Quaternion.Inverse(UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye));

            if (location == SetupLocation.RIGHT)
            {
                headPivot.transform.parent.forward = -headPivot.transform.parent.forward;
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
        rightElbow.localPosition =      filterActive ? _rightElbowFilter.Filter(human.body.Joints[BodyJointType.rightElbow]) : human.body.Joints[BodyJointType.rightElbow];
        rightWrist.localPosition =      filterActive ? _rightWristFilter.Filter(human.body.Joints[BodyJointType.rightWrist]) : human.body.Joints[BodyJointType.rightWrist];
        rightHand.localPosition =       filterActive ? _rightHandFilter.Filter(human.body.Joints[BodyJointType.rightHand]) : human.body.Joints[BodyJointType.rightHand];
        rightHandTip.localPosition =    filterActive ? _rightHandTipFilter.Filter(human.body.Joints[BodyJointType.rightHandTip]) : human.body.Joints[BodyJointType.rightHandTip];

        leftShoulder.localPosition = human.body.Joints[BodyJointType.leftShoulder];
        leftElbow.localPosition = filterActive ? _leftElbowFilter.Filter(human.body.Joints[BodyJointType.leftElbow]) : human.body.Joints[BodyJointType.leftElbow];
        leftWrist.localPosition = filterActive ? _leftWristFilter.Filter(human.body.Joints[BodyJointType.leftWrist]) : human.body.Joints[BodyJointType.leftWrist];
        leftHand.localPosition = filterActive ? _leftHandFilter.Filter(human.body.Joints[BodyJointType.leftHand]) : human.body.Joints[BodyJointType.leftHand];
        leftHandTip.localPosition = filterActive ? _leftHandTipFilter.Filter(human.body.Joints[BodyJointType.leftHandTip]) : human.body.Joints[BodyJointType.leftHandTip];

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

            rightIKShoulder.localPosition = rightShoulder.localPosition;
            rightIKElbow.localPosition = rightElbow.localPosition;
            rightIKWrist.localPosition = rightWrist.localPosition;
            rightIKHandTip.localPosition = rightHandTip.localPosition;

            leftIKShoulder.localPosition = leftShoulder.localPosition;
            leftIKElbow.localPosition = leftElbow.localPosition;
            leftIKWrist.localPosition = leftWrist.localPosition;
            leftIKHandTip.localPosition = leftHandTip.localPosition;
        }
    }

    private Vector3 _mirrorJoint(Vector3 p)
    {
        return new Vector3(-p.x, p.y, p.z);
    }
}
