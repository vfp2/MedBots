using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sample usage file. Displays data collected from the Senzeband
/// </summary>
public class EEGPanelController : MonoBehaviour {
#if !UNITY_EDITOR_OSX
	public NSB_Manager nsbm;
	public Text AttentionValue;
	public Text RelaxationValue;
	public Text MentalWLValue;
	public Text AccXValue;
	public Text AccYValue;
	public Text AccZValue;
	public Text GoodConnValue;
	public Text Ch1Value;
	public Text Ch2Value;
	public Text Ch3Value;
	public Text Ch4Value;
	public Text DeltaValue;
	public Text ThetaValue;
	public Text AlphaValue;
	public Text BetaValue;
	public Text GammaValue;
	public Text SignalReadyValue;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (nsbm.GetReceiveEEGState ()) {
			AttentionValue.text = nsbm.GetAttention ().ToString();
			RelaxationValue.text = nsbm.GetRelaxation ().ToString();
			MentalWLValue.text = nsbm.GetMentalWL ().ToString();

			AccXValue.text = nsbm.GetAccel (0).ToString ();
			AccYValue.text = nsbm.GetAccel (1).ToString ();
			AccZValue.text = nsbm.GetAccel (2).ToString ();

			GoodConnValue.text = nsbm.GetGoodBTConnection ().ToString ();
			Ch1Value.text = nsbm.GetChannelStatus (0).ToString ();
			Ch2Value.text = nsbm.GetChannelStatus (1).ToString ();
			Ch3Value.text = nsbm.GetChannelStatus (2).ToString ();
			Ch4Value.text = nsbm.GetChannelStatus (3).ToString ();
			SignalReadyValue.text = nsbm.GetSignalReady ().ToString ();

			DeltaValue.text = Mathf.Round (nsbm.GetFrequencyBand (0,0) * 100).ToString ();
			ThetaValue.text = Mathf.Round (nsbm.GetFrequencyBand (0,1) * 100).ToString ();
			AlphaValue.text = Mathf.Round (nsbm.GetFrequencyBand (0,2) * 100).ToString ();
			BetaValue.text = Mathf.Round (nsbm.GetFrequencyBand (0,3) * 100).ToString ();
			GammaValue.text = Mathf.Round (nsbm.GetFrequencyBand (0,4) * 100).ToString ();

		} else {
			AttentionValue.text = "-";
			RelaxationValue.text = "-";
			MentalWLValue.text = "-";

			AccXValue.text = "-";
			AccYValue.text = "-";
			AccZValue.text = "-";

			GoodConnValue.text = "-";
			Ch1Value.text = "-";
			Ch2Value.text = "-";
			Ch3Value.text = "-";
			Ch4Value.text = "-";
			SignalReadyValue.text = "-";

			DeltaValue.text = "-";
			ThetaValue.text = "-";
			AlphaValue.text = "-";
			BetaValue.text = "-";
			GammaValue.text = "-";
		}
	}
#endif
}
