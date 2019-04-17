﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class IKWarpInfo {

    public Vector3 LEFT_OriginalShoulder;
    public Vector3 LEFT_OriginalElbow;
    public Vector3 LEFT_OriginalWrist;
    public Vector3 LEFT_OriginalHandTip;

    public Vector3 LEFT_IKShoulder;
    public Vector3 LEFT_IKElbow;
    public Vector3 LEFT_IKWrist;
    public Vector3 LEFT_IKHandTip;

    public Vector3 RIGHT_OriginalShoulder;
    public Vector3 RIGHT_OriginalElbow;
    public Vector3 RIGHT_OriginalWrist;
    public Vector3 RIGHT_OriginalHandTip;

    public Vector3 RIGHT_IKShoulder;
    public Vector3 RIGHT_IKElbow;
    public Vector3 RIGHT_IKWrist;
    public Vector3 RIGHT_IKHandTip;

    public float UpperArmDistance;
    public float ForearmDistance;
    public float HandDistance;

    public Matrix4x4 LEFT_UpperArmMatrix;
    public Matrix4x4 LEFT_ForearmMatrix;
    public Matrix4x4 LEFT_HandMatrix;

    public Matrix4x4 RIGHT_UpperArmMatrix;
    public Matrix4x4 RIGHT_ForearmMatrix;
    public Matrix4x4 RIGHT_HandMatrix;

    public bool warping;

    public bool debug;

    private GameObject LEFTELB;
    private GameObject LEFTWRIST;

    public IKWarpInfo()
    {
        warping = false;
        debug = false;

        LEFTELB = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        LEFTELB.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        LEFTWRIST = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        LEFTWRIST.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        _reset();
    }

    public void Solve()
    {
        if (!warping)
        {
            _reset();
            return;
        }

        // LEFT ARM
        Vector3 shoulder = LEFT_OriginalShoulder;
        Vector3 elbow = LEFT_OriginalElbow;
        Vector3 wrist = LEFT_OriginalWrist;
        Vector3 handTip = LEFT_OriginalHandTip;

        Vector3 oldVector_upper = LEFT_OriginalElbow - LEFT_OriginalShoulder;
        Vector3 newVector_upper = LEFT_IKElbow - LEFT_IKShoulder;
        // Shoulder -> Elbow
        LEFT_UpperArmMatrix = _calcM(shoulder, oldVector_upper, newVector_upper);

        elbow = LEFT_UpperArmMatrix.MultiplyPoint(elbow);
        wrist = LEFT_UpperArmMatrix.MultiplyPoint(wrist);
        Vector3 oldVector_forearm = wrist - elbow;
        Vector3 newVector_forearm = LEFT_IKWrist - LEFT_IKElbow;
        // Elbow -> Wrist

        LEFT_ForearmMatrix = _calcM(elbow, oldVector_forearm, newVector_forearm);

        wrist = LEFT_ForearmMatrix.MultiplyPoint(wrist);
        handTip = LEFT_UpperArmMatrix.MultiplyPoint(handTip);
        handTip = LEFT_ForearmMatrix.MultiplyPoint(handTip);

        Vector3 oldVector_hand = handTip - wrist;
        Vector3 newVector_hand = LEFT_IKHandTip - LEFT_IKWrist;
        // Wrist -> HandTip
        LEFT_HandMatrix = _calcM(wrist, oldVector_hand, newVector_hand);




        //  ARM
        shoulder = RIGHT_OriginalShoulder;
        elbow = RIGHT_OriginalElbow;
        wrist = RIGHT_OriginalWrist;
        handTip = RIGHT_OriginalHandTip;

        oldVector_upper = RIGHT_OriginalElbow - RIGHT_OriginalShoulder;
        newVector_upper = RIGHT_IKElbow - RIGHT_IKShoulder;
        // Shoulder -> Elbow
        RIGHT_UpperArmMatrix = _calcM(shoulder, oldVector_upper, newVector_upper);

        elbow = RIGHT_UpperArmMatrix.MultiplyPoint(elbow);
        wrist = RIGHT_UpperArmMatrix.MultiplyPoint(wrist);
        oldVector_forearm = wrist - elbow;
        newVector_forearm = RIGHT_IKWrist - RIGHT_IKElbow;
        // Elbow -> Wrist

        RIGHT_ForearmMatrix = _calcM(elbow, oldVector_forearm, newVector_forearm);

        wrist = RIGHT_ForearmMatrix.MultiplyPoint(wrist);
        handTip = RIGHT_UpperArmMatrix.MultiplyPoint(handTip);
        handTip = RIGHT_ForearmMatrix.MultiplyPoint(handTip);

        oldVector_hand = handTip - wrist;
        newVector_hand = RIGHT_IKHandTip - RIGHT_IKWrist;
        // Wrist -> HandTip
        RIGHT_HandMatrix = _calcM(wrist, oldVector_hand, newVector_hand);
    }

    private Matrix4x4 _calcM(Vector3 pivot, Vector3 oldVector, Vector3 newVector)
    {
        Vector3 axis = Vector3.Cross(oldVector, newVector);   
        float angle = Vector3.Angle(oldVector, newVector);

        return Matrix4x4.Translate(pivot) * Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(angle, axis), Vector3.one) * Matrix4x4.Translate(-pivot);
    }

    private void _reset()
    {
       LEFT_UpperArmMatrix = Matrix4x4.identity;
       LEFT_ForearmMatrix = Matrix4x4.identity;
       LEFT_HandMatrix = Matrix4x4.identity;
       
       RIGHT_UpperArmMatrix = Matrix4x4.identity;
       RIGHT_ForearmMatrix = Matrix4x4.identity;
       RIGHT_HandMatrix = Matrix4x4.identity;
    }

}