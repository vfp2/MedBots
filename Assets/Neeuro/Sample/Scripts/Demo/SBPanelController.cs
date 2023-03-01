using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sample usage file. To control the demo Senzeband UI
/// </summary>
public class SBPanelController : MonoBehaviour {
#if !UNITY_EDITOR_OSX && !UNITY_STANDALONE_OSX
	public NSB_Manager nsbm;

	public Text ConnectionStatusText;
	public Text SBAddressText;
	public Text SBMcuidText;
    public Text SBBattery;
    public Text ReceiveEEGText;
	public Text AuthenticationText;
	public Button DisconnectButton;
	public Button ToggleEEGButton;
	public Button CancelConnectButton;

	private bool bIsReady;
	private bool bIsConnected;

	const string defaultMCUIDtext = "MCUID";
	const string defaultSBAddresstext = "SB Address";
	const string defaultBatterytext = "-";

	// Use this for initialization
	void Start () {
		bIsReady = false;
		if (SBAddressText != null)
			SBAddressText.text = defaultSBAddresstext;
		
		if (DisconnectButton != null)
			DisconnectButton.gameObject.SetActive (false);
		
		if(SBMcuidText!=null)
			SBMcuidText.text = defaultMCUIDtext;

        if(SBBattery!=null)
            SBBattery.text = defaultBatterytext;

        if (ToggleEEGButton != null)
			ToggleEEGButton.gameObject.SetActive (false);
	
		if(ReceiveEEGText!=null)
			ReceiveEEGText.gameObject.SetActive (false);

		if (CancelConnectButton != null)
			CancelConnectButton.gameObject.SetActive (false);

		if (ConnectionStatusText != null)
			ConnectionStatusText.text = "-";

	}
	
	// Update is called once per frame
	void Update () {

		//Performs one-time processes when NSB init is completed.
		if (!bIsReady) {
			if (nsbm.IsInitCompleted ()) {
				//Just Ready!
				bIsReady = true;
			} else {
				//NSB init is not ready yet, so skip all following processes
				return;
			}
		}

		AuthenticationText.text = "Authentication: " + (nsbm.GetAuthenticationResult()? "True" : "False");

		/*
		Connection States
		- 0 	- Not connected
		- 1 	- Connecting
		- 2		- Connected, connectionStatus is updated in DeviceStatus callback
		- 3		- Connected, SB address is returned in ConnectionSucceed callback
		- 4		- Connected, MCUID is received
		*/
		//Track the state of connection
		if (!bIsConnected) {
			int subState = nsbm.GetConnectionState ();
			switch (subState) {
			case 0:
				ConnectionStatusText.text = "Not connected";
				if (CancelConnectButton.gameObject.activeSelf == true) {
					CancelConnectButton.gameObject.SetActive (false);
					CancelConnectButton.onClick.RemoveAllListeners ();
				}
				break;
			case 1:
				ConnectionStatusText.text = "Connecting...";
				if (CancelConnectButton.gameObject.activeSelf == false) {
					CancelConnectButton.gameObject.SetActive (true);
					CancelConnectButton.onClick.AddListener (() => {
						nsbm.DisconnectSB ();
					});		//TODO: for Android, to add a cancel connecting process function. For iOS, this disconnect function works
				}
				break;
			case 2:
				ConnectionStatusText.text = "Connected, awaiting info";
				break;
			case 3:
				ConnectionStatusText.text = "Connected, awaiting more info";
				break;
			case 4:
				ConnectionStatusText.text = "Connected. Ready!!";
				break;
			}
		}

		//Transitions from Not Connected to Connected; add in and remove buttons
		if (!bIsConnected && nsbm.GetConnectionState () 	== 4) {
			//Just connected
			if (CancelConnectButton.gameObject.activeSelf == true) {
				CancelConnectButton.gameObject.SetActive (false);
				CancelConnectButton.onClick.RemoveAllListeners ();
			}
			DisconnectButton.gameObject.SetActive (true);
			DisconnectButton.onClick.AddListener ( ()=>{nsbm.DisconnectSB();} );

			ToggleEEGButton.gameObject.SetActive (true);
			ToggleEEGButton.onClick.AddListener ( ToggleEEG );

			ReceiveEEGText.gameObject.SetActive (true);
			if (!nsbm.GetReceiveEEGState ())
				ReceiveEEGText.text = "Not receiving EEG";

			//Debug.Log ("NSB SB Panel Connected - "+nsbm.NSBm_GetConnectedSBAddress ());
			SBAddressText.text = nsbm.GetConnectedSBAddress ();
			SBMcuidText.text = nsbm.GetConnectedSBMCUID ();
            SBBattery.text = nsbm.GetConnectedSBBattery ();
            bIsConnected = true;


		} else if (bIsConnected && nsbm.GetConnectionState () == 0) {
			//Just disconnected
			DisconnectButton.gameObject.SetActive (false);
			DisconnectButton.onClick.RemoveAllListeners ();

			ToggleEEGButton.gameObject.SetActive (false);
			ToggleEEGButton.onClick.RemoveAllListeners ();

			ReceiveEEGText.gameObject.SetActive (false);

			//Debug.Log ("NSB SB Panel Disconnected");
			SBAddressText.text = defaultSBAddresstext;
			SBMcuidText.text = defaultMCUIDtext;
			SBBattery.text = defaultBatterytext;
			bIsConnected = false;
		}

        if(nsbm.GetReceiveEEGState())
        {
            SBBattery.text = nsbm.GetConnectedSBBattery() + "% battery";
        }
		else
			SBBattery.text = defaultBatterytext;
	}

	private void ToggleEEG()
	{
		//To toggle to receive EEG data
		if (nsbm.GetReceiveEEGState ()) {
			nsbm.SetReceiveEEG (false);
			ReceiveEEGText.text = "Not receiving EEG";
		} else {
			nsbm.SetReceiveEEG (true);
			ReceiveEEGText.text = "Receiving EEG";
		}
	}
#endif
}
