using UnityEngine;

public class AdaptiveDoubleExponentialFilterVector3
{
	private AdaptiveDoubleExponentialFilterFloat x;
	private AdaptiveDoubleExponentialFilterFloat y;
	private AdaptiveDoubleExponentialFilterFloat z;

	public Vector3 Value
	{
		get { return new Vector3(x.Value, y.Value, z.Value); }
		set { Update(value); }
	}

	public AdaptiveDoubleExponentialFilterVector3()
	{
		x = new AdaptiveDoubleExponentialFilterFloat();
		y = new AdaptiveDoubleExponentialFilterFloat();
		z = new AdaptiveDoubleExponentialFilterFloat();
	}

	private void Update(Vector3 v)
	{
		x.Value = v.x;
		y.Value = v.y;
		z.Value = v.z;
	}


    private float deltaLow;
    private float deltaHigh;
    private float gainLow;
    private float gainHigh;
    private float trendGain;
    public float DeltaLow
    {
        get { return deltaLow; }
        set { deltaLow = value;
            x.DeltaLow = value;
            y.DeltaLow = value;
            z.DeltaLow = value;
        }
    }

    public float DeltaHigh
    {
        get { return deltaHigh; }
        set { deltaHigh = value;
            x.DeltaHigh = value;
            y.DeltaHigh = value;
            z.DeltaHigh = value;
        }
    }

    public float GainLow
    {
        get { return gainLow; }
        set { gainLow = value;
            x.GainLow = value;
            y.GainLow = value;
            z.GainLow = value;
        }
    }

    public float GainHigh
    {
        get { return gainHigh; }
        set { gainHigh = value;
            x.GainHigh = value;
            y.GainHigh = value;
            z.GainHigh = value;
        }
    }

    public float TrendGain
    {
        get { return trendGain; }
        set { trendGain = value;
            x.TrendGain = value;
            y.TrendGain = value;
            z.TrendGain = value;
        }
    }

}