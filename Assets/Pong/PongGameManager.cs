using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;

public class PongGameManager : MonoBehaviour {

	public static int PlayerScore1 = 0;
	public static int PlayerScore2 = 0;

	public GUISkin layout;

	GameObject theBall;

	// Use this for initialization
	void Start () {
		theBall = GameObject.FindGameObjectWithTag ("Ball");

		InitMed();
	}

	public static void Score(string wallID) {
		if (wallID == "RightWall") {
			PlayerScore1++;
		} else {
			PlayerScore2++;
		}
	}

	void OnGUI() {
		GUI.skin = layout;
		GUI.Label (new Rect (Screen.width / 2 - 150 - 12, 20, 100, 100), "" + PlayerScore1);
		GUI.Label (new Rect (Screen.width / 2 + 150 + 12, 20, 100, 100), "" + PlayerScore2);

		if (GUI.Button (new Rect (Screen.width / 2 - 60, 35, 120, 53), "RESTART")) {
			PlayerScore1 = 0;
			PlayerScore2 = 0;
			theBall.SendMessage ("RestartGame", 0.5f, SendMessageOptions.RequireReceiver);
		}

		if (PlayerScore1 == 10) {
			GUI.Label (new Rect (Screen.width / 2 - 150, 200, 2000, 1000), "PLAYER ONE WINS");
			theBall.SendMessage ("ResetBall", null, SendMessageOptions.RequireReceiver);
		} else if (PlayerScore2 == 10) {
			GUI.Label (new Rect (Screen.width / 2 - 150, 200, 2000, 1000), "PLAYER TWO WINS");
			theBall.SendMessage ("ResetBall", null, SendMessageOptions.RequireReceiver);
		}
	}

	private static readonly int MF_ERROR_STR_MAX_LEN = 256;
    private static StringBuilder sMFErrorReason = new StringBuilder(MF_ERROR_STR_MAX_LEN);

    [DllImport("meterfeeder", CallingConvention = CallingConvention.Cdecl)]
    private static extern int MF_Initialize(StringBuilder pErrorReason);
    [DllImport("meterfeeder", CallingConvention = CallingConvention.Cdecl)]
    private static extern int MF_Shutdown();

	void InitMed()
    {
        int medRes = -1;
        sMFErrorReason.Clear();
        medRes = MF_Initialize(sMFErrorReason);
        Debug.Log($"MeterFeeder MF_Initialize: result:{medRes}, errorReason:{sMFErrorReason}");
    }

    void OnApplicationQuit()
    {
        Debug.Log("MeterFeeder MF_Shutdown");
        MF_Shutdown();
    }
}
