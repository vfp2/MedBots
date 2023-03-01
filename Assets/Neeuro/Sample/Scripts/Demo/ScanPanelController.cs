using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sample usage file. To control the demo Scan panel
/// </summary>
public class ScanPanelController : MonoBehaviour {
#if !UNITY_EDITOR_OSX && !UNITY_STANDALONE_OSX
	public NSB_Manager nsbm;
	public Text btStatusText;
	public Text scanStatusText;
	public Button scanButton;
	public GameObject availSBButton;

	private List<GameObject> availSBButtonList;
	private bool bIsReady;

	// Use this for initialization
	void Start () {
		bIsReady = false;
		availSBButtonList = new List<GameObject>();
		//hide this button reference for instantiating
		if(availSBButton!=null)
			availSBButton.SetActive (false);
	}
		

	// Update is called once per frame
	void Update () {

		//Performs one-time processes when NSB init is completed.
		if (!bIsReady) {
			if (nsbm.IsInitCompleted()) {
				//Just Ready!
				bIsReady = true;
				scanButton.onClick.AddListener (ToggleScan);
			} else {
				//NSB init is not ready yet, so skip all following processes
				return;
			}
		}

		if (nsbm.IsBluetoothEnabled ()) {

			if (nsbm.IsScanning())
				scanStatusText.text = "Scanning";
			else
				scanStatusText.text = "Not scanning";


			if (btStatusText.text != "BT is ON") {
				//BT is switched from OFF to ON
				btStatusText.text = "BT is ON";
			}
		} else {
			if (btStatusText.text != "BT is OFF") {
				//BT is switched from ON to OFF
				btStatusText.text = "BT is OFF";
				//update of scanning status
				if (nsbm.IsScanning ()) {
					ToggleScan ();
				}
			}
		}
		//Test
		//if(Input.GetKeyDown(KeyCode.A))
		//{
		//	nsbm.listAvailableDevices.Add("abcdefgh");
		//}

		//To manage, create and destroy list of buttons of available SenzeBands detected.
		if ( availSBButtonList.Count != nsbm.listAvailableDevices.Count ) 
		{
			//Different number of SB avail
			//Debug.Log("NSB  Button List has "+availSBButtonList.Count + " ; NSBm List has "+nsbm.listAvailableDevices.Count);

			if (nsbm.listAvailableDevices.Count > availSBButtonList.Count) 
			{
				//To add 1 SB button
				//Debug.Log("NSB  add 1 button to current "+availSBButtonList.Count);

				GameObject buttonObj;
				buttonObj = Instantiate (availSBButton, this.transform);
				Vector3 pos = buttonObj.transform.localPosition;
				pos.y = scanButton.transform.localPosition.y - 60 - availSBButtonList.Count * 50;
				buttonObj.transform.localPosition = pos;

				buttonObj.GetComponentInChildren<Text>().text = nsbm.listAvailableDevices [availSBButtonList.Count];
				buttonObj.GetComponent<Button>().onClick.AddListener ( ()=>{ nsbm.ConnectSB(buttonObj.GetComponentInChildren<Text>().text); } );
				buttonObj.SetActive (true);
				availSBButtonList.Add (buttonObj);

			}
			if (nsbm.listAvailableDevices.Count < availSBButtonList.Count && availSBButtonList.Count > 0) {
				//clear all availSBButtonList
				//Debug.Log("NSB  remove all "+availSBButtonList.Count+" buttons");
				for (int i = availSBButtonList.Count-1; i >= 0; --i) {
					Destroy (availSBButtonList [i]);
				}
				availSBButtonList.Clear ();
					
			}

		}
			
	}

	void ToggleScan()
	{
		if (nsbm.IsScanning ()) {
			nsbm.SetScanning (false);
			scanStatusText.text = "Not scanning";
		} else {
			if (nsbm.IsBluetoothEnabled ()) {
				nsbm.SetScanning (true);
				scanStatusText.text = "Scanning";
			}
		}
	}
#endif
}