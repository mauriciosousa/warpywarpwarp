using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FILTER
{
    AdaptiveDoubleExponential,
    Kalman,
    OneEuroFilter
}

public class VRHeadController : MonoBehaviour {

    public BodiesManager _bodies;
    public Transform localHumanHead;

    public FILTER filter;

    private AdaptiveDoubleExponentialFilterVector3 headPosition_ADX;
    private KalmanFilterVector3 headPosition_KLM;

    private Vector3 oneeurofilteredValue;
    private OneEuroFilter<Vector3> headPosition_1EURO;

    [Space(5)]
    [Header("OneEuroFilter Params:")]
    public float freq = 100.0f;
    public float mincutoff = 1.0f;
    public float beta = 0.001f;
    public float dcutoff = 1.0f;

    void Start()
    {
        headPosition_ADX = new AdaptiveDoubleExponentialFilterVector3();
        headPosition_KLM = new KalmanFilterVector3();

        oneeurofilteredValue = Vector3.zero;
        headPosition_1EURO = new OneEuroFilter<Vector3>(freq, mincutoff, beta, dcutoff);
    }

    void Update()
    {

        if (_bodies.human != null)
        {
            if (filter == FILTER.AdaptiveDoubleExponential)
            {
                headPosition_ADX.Value = localHumanHead.position;
                this.transform.position = headPosition_ADX.Value;
            }
            else if (filter == FILTER.Kalman)
            {
                headPosition_KLM.Value = localHumanHead.position;
                this.transform.position = headPosition_KLM.Value;
            }
            else
            {
                headPosition_1EURO.UpdateParams(freq, mincutoff, beta, dcutoff);
                oneeurofilteredValue = headPosition_1EURO.Filter(localHumanHead.position);
                this.transform.position = oneeurofilteredValue;
            }
        }
        else
        {
            this.transform.position = new Vector3(0, 1.8f, 0);
        }
    }
}
