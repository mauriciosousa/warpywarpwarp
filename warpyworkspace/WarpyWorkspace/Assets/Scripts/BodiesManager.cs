﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodiesManager : MonoBehaviour
{

    private Dictionary<string, Human> _humans;
    private bool _humanLocked = false;
    public Human human = null;

    public Transform CenterObject;

    public Transform humanGO;

    private List<Transform> _listOfChildren;
    private Dictionary<BodyJointType, Transform> _bodyTrans;


    void Start()
    {
        _humans = new Dictionary<string, Human>();
        _listOfChildren = new List<Transform>();
        _bodyTrans = new Dictionary<BodyJointType, Transform>();
        _populateBodyTrans(humanGO);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            _disassembleHierarchy(humanGO);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            _assembleHumanHierarchy(humanGO);
        }

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
            foreach (BodyJointType joint in (BodyJointType[])Enum.GetValues(typeof(BodyJointType)))
            {
                humanGO.Find(joint.ToString()).localPosition = human.body.Joints[joint];
            }
            _assembleHumanHierarchy(humanGO);
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
            foreach (Transform t in trans)
            {
                _getAllChildren(t);
            }
        }

        foreach (Transform g in _listOfChildren)
        {
            g.parent = trans;
        }

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
}
