using UnityEngine;

public class AdaptiveDoubleExponentialFilterFloat
{
	// as described in: msdn.microsoft.com/en-us/library/jj131429.aspx

	private float deltaLow = 0.02f;
	private float deltaHigh = 0.05f;
	private float gainLow = 0.1f;
	private float gainHigh = 0.15f;
	private float trendGain = 0.1f;

	public bool filtering = true;

	public float DeltaLow {
		get { return deltaLow; }
		set { deltaLow = value; }
	}

	public float DeltaHigh {
		get { return deltaHigh; }
		set { deltaHigh = value; }
	}

	public float GainLow {
		get { return gainLow; }
		set { gainLow = Mathf.Clamp (value, 0.0f, 1.0f); }
	}

	public float GainHigh {
		get { return gainHigh; }
		set { gainHigh = Mathf.Clamp (value, 0.0f, 1.0f); }
	}

	public float TrendGain {
		get { return trendGain; }
		set { trendGain = Mathf.Clamp (value, 0.0f, 1.0f); }
	}

	private float filteredValue;

	public float Value {
		get { return filteredValue; }
		set { filteredValue = Update (value); }
	}

	private float lastValue;
	private float trend;

	public AdaptiveDoubleExponentialFilterFloat ()
	{
		filteredValue = float.NaN;
		lastValue = float.NaN;
		trend = 0.0f;
	}

	private float Update (float newValue)
	{
		if (float.IsNaN (filteredValue) || !filtering) {
			lastValue = newValue;
			return newValue;
		} else {
			float gain;
			float delta = Mathf.Abs (newValue - lastValue);

			if (delta <= deltaLow) { // Low velocity
				gain = gainLow;
			} else if (delta >= deltaHigh) { // High velocity
				gain = gainHigh;
			} else { // Medium velocity
				gain = gainHigh + ((delta - deltaHigh) / (deltaLow - deltaHigh)) * (gainLow - gainHigh);
			}

			float newFilteredValue = gain * newValue + (1 - gain) * (filteredValue + trend);
			trend = trendGain * (newFilteredValue - filteredValue) + (1 - trendGain) * trend;

			lastValue = newValue;
			return newFilteredValue;
		}
	}
}