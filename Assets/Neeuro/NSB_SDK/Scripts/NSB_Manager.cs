//#define USE_SBv2;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System;

#if (UNITY_STANDALONE_WIN || UNITY_EDITOR) && NET_4_6 && !UNITY_EDITOR_OSX
using NSB_SDK_WINDOWS;
#elif UNITY_ANDROID
using NSB_SDK_ANDROID;
#elif UNITY_IOS
using NSB_SDK_IOS;
#endif

/// <summary>
/// Receives EEG data via callbacks from Senzeband framework
/// </summary>
public class NSB_Manager : CallbackReceiver
{
#if !UNITY_EDITOR_OSX
	/*
		NSBM Init
			- false
			- true 	- Ready

		Scanning
			- false - Not scanning
			- true 	- Scanning

		Connection States
			- 0 	- Not connected
			- 1 	- Connecting
			- 2		- Connected, connectionStatus is updated in DeviceStatus callback
			- 3		- Connected, SB address is returned in ConnectionSucceed callback
			- 4		- Connected, MCUID is received
			Note:
			iOS devices 0->1->3->4
			Android devices 0->1->2->3->4

		EEG Started
			- false
			- true

		Authenticated
			- false	- Expired, not able to authenticate Developer code with Neeuro Server
			- true	- Valid, not expired

	*/


	//Developer info to update
	public string DEVELOPER_CODE = "1111222233334444";  //Replace this string with your developer code. This is used to authenticate with the NEEURO server.

	//Information stored for SB
	public List<string> listAvailableDevices = new List<string>();  //list of available SB addresses, from scanning

	private bool bIsInitCompleted = false;                          //state, if NSB systems are ready
	private bool bIsScanning = false;                               //state, if scanning
	private bool bEegStarted = false;                               //state, if EEG is being transmitted, received
	private int connectionState = 0;                                //state, on the connection
	private int prevConnectionState = 0;

	private string addressConnectingSB = string.Empty;                  //holds address of currently connecting SB
	private string addressConnectedSB = string.Empty;                   //holds address of currently connected SB
	private string mcuid = string.Empty;                                //holds the MCUID when available (of connected SB)
	private string connectionStatus = string.Empty;                 // "Not connected", "Connecting" or "Connected"  from NSB_BLE.getConnectingString(), getConnectedString(), getNotConnectedString
	private bool bluetoothStatus = false;                      //holds the OS's bluetooth status,  enabled or disabled
	private string batteryLevel = string.Empty;                     //holds the battery level

	private float[] mentalStateData = new float[4];
	private int[] accelerometerData = new int[3];
	private bool[] channelStatus = new bool[4];
	private bool goodBTConnection = false;
	private bool signalReady;
	private float[,] frequencyBandData = new float[4, 5];
	private int[] rawEEGData = new int[1000];
	private bool authenticationResult = false;
	private string authenticationStatus = "";                       //holds the string for authentication status: "200", "No Intenet COnnection" "Invalid"
	private float[] gammaReading = new float[4]; 
	private float[] meanReading = new float[4];
	private float[] fiftysixtyReading = new float[4];   //50 60 Strength

	public static NSB_Manager instance = null;

	#region Event Callbacks
	/// <summary>
	/// Triggered when SenzeBand connection is successful.
	/// </summary>
	public UnityEvent connectionSuccessfulCallback;

	/// <summary>
	/// Triggered when SenzeBand connection is broken or disconnected.
	/// </summary>
	public UnityEvent connectionBrokenCallback;

	/// <summary>
	/// Triggered when the app fails to connect successfully to the SenzeBand. 
	/// </summary>
	public UnityEvent connectionFailedCallback;

	/// <summary>
	/// Triggered when the app receives EEG data from the SenzeBand. 
	/// </summary>
	public UnityEvent rawdataGrabbed;   //will be used to announce if new set of data has been fetched
	#endregion

	private void Awake()
	{
		if (instance != null)
			Destroy(this.gameObject);

		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(this.gameObject); //We need this object to persist throughout the lifetime of the program
		}
	}
	//UNITY mono functions
	// Use this for initialization
	new void Start()
	{
		base.Start();

		bIsInitCompleted = false;
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		Init();
	}

	// Update is called once per frame
	System.DateTime lastClear = System.DateTime.Now;
	System.TimeSpan period = new System.TimeSpan(0, 0, 6);
	new void Update()
	{
		base.Update();

		if (bIsScanning)
		{
			if (System.DateTime.Now - lastClear > period)
			{
				lastClear = System.DateTime.Now;
				ClearList();
			}
		}

		//Tracking connection state;
		if (connectionState != prevConnectionState)
		{
			Debug.Log("NSB Connection state, " + prevConnectionState + " -> " + connectionState);
			prevConnectionState = connectionState;

			if (connectionState == 4)
			{
				connectionSuccessfulCallback.Invoke();
			}
		}
	}

	private void OnDestroy()
	{
		Shutdown();
	}


	/// <summary>
	/// Initialises Bluetooth, NSB library system, and also sets the callback functions when data is received or calculated.
	/// </summary>
	public void Init()
	{
		//Initialises Bluetooth, NSB libraries, and also sets the callback functions when hardwares' actions are completed.
		if (bIsInitCompleted == false)
		{
			//BLE - Bluetooth system controls
			NSB_BLE.instance.assignErrorLogDelegate(Log);
			NSB_BLE.instance.initializeBT(InitComplete, GetDeviceStatus, GetBTStatus, DEVELOPER_CODE);			

			NSB_BLE.instance.assignAuthenticationStatusDelegate(GetAuthenticationStatus);
			NSB_BLE.instance.assignAuthenticationResultDelegate(GetAuthenticationResult);

			NSB_EEG.instance.assignBatteryStatus(GetBattery);
			NSB_BLE.instance.assignScanCallBack(FoundAvailableDevice);

			//EEG - EEG signal processing controls 
			NSB_EEG.instance.assignAttentionDelegate(grabAttention);
			NSB_EEG.instance.assignRelaxationDelegate(grabRelaxation);
			NSB_EEG.instance.assignMentalWorkloadDelegate(grabMentalWorkload);
			NSB_EEG.instance.assignAccDelegate(grabAccelerometer);
			NSB_EEG.instance.assignChannelDelegate(grabChannelStatus);
			NSB_EEG.instance.assignGoodConnectionCheckDelegate(grabGoodConnection);
			NSB_EEG.instance.assignSignalReadyStatusDelegate(grabSignalReady);
			NSB_EEG.instance.assignMCUIDDelegate(grabMCUID);
			NSB_EEG.instance.assignABDTDelegate(grabFrequencyBand);
			NSB_EEG.instance.assignRawDataDelegate(grabRawEEG);
			NSB_EEG.instance.assignEnvironmentDataDelegate(grabEnvironmentData);

			SetScanning(true);
		}
	}

	/// <summary>
	/// Releases resources under NSB library system
	/// </summary>
	public void Shutdown()
	{
		NSB_BLE.instance.shutdownBT();
	}

	/// <summary>
	///  
	/// </summary>
	/// <returns>Boolean value stating whether the initialisation of NSB library system is completed</returns>
	public bool IsInitCompleted()
	{

		return bIsInitCompleted;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns>Boolean value stating whether Bluetooth is enabled or not</returns>
	public bool IsBluetoothEnabled()
	{
		return bluetoothStatus;
	}

	/// <summary>
	///  
	/// </summary>
	/// <returns>Boolean value stating whether the app is scanning for available SB devices or not</returns>
	public bool IsScanning()
	{
		return bIsScanning;
	}

	/// <summary>
	/// Turns on or off scanning for SB device.
	/// </summary>
	/// <param name="state"></param>
	public void SetScanning(bool state)
	{
		if (bIsScanning)
		{
			ClearList();
		}
		bIsScanning = state;

		Debug.Log("NSB SetScanning " + state);
		NSB_BLE.instance.startStopScanning(state);
	}

	/// <summary>
	///  
	/// </summary>
	/// <returns>List of available SB devices found from scanning </returns>
	public List<string> GetScannedSenzeBandList()
	{
		return listAvailableDevices.Distinct().ToList();
	}

	/// <summary>
	/// Starts connection process to SB device
	/// </summary>
	/// <param name="address">Address of device to connect to</param>
	/// <returns>Success or failure of connection attempt</returns>
	public bool ConnectSB(string address)
	{
		//Starts connection process to SB device
		Debug.Log("NSB ConnectSB " + address);
		if (NSB_BLE.instance.connectBT(address, ConnectionSucceed, ConnectionBroken, ConnectionFailed) == false)
		{
			//Unable to process this function
			Debug.Log("NSB Can't start connect");
			return false;
		}

		addressConnectingSB = address;  //TEMP
		ClearList();
		return true;
	}

	/// <summary>
	/// Disconnects a connected SB device
	/// </summary>
	public void DisconnectSB()
	{
		//Disconnects a connected SB device
		Debug.Log("NSB DisconnectSB");
		if (addressConnectedSB != string.Empty)
		{
			NSB_BLE.instance.disconnectBT(addressConnectedSB);
			//connectionBrokenCallback.Invoke();

		}
		else if (addressConnectingSB != string.Empty)
		{
			NSB_BLE.instance.disconnectBT(addressConnectingSB);
			connectionFailedCallback.Invoke();
		}
	}

	/// <summary>
	/// Returns the state of the NSB connection handling.
	///
	/// Connection States
	/// - 0 	- Not connected
	/// - 1 	- Connecting
	/// - 2		- Connected, connectionStatus is updated in DeviceStatus callback
	/// - 3		- Connected, SB address is returned in ConnectionSucceed callback
	/// - 4		- Connected, MCUID is received	
	/// </summary>
	/// <returns>Connection state</returns>
	public int GetConnectionState()
	{
		connectionState = 0;

		if (connectionStatus == NSB_BLE.instance.getNotConnectedString())
			connectionState = 0;
		else if (connectionStatus == NSB_BLE.instance.getConnectingString())
			connectionState = 1;
		else if (connectionStatus == NSB_BLE.instance.getConnectedString())
		{
			if (addressConnectedSB == string.Empty)
				connectionState = 2;
			else
			{
				if (mcuid == string.Empty)
					connectionState = 3;
				else
					connectionState = 4;
			}
		}
		return connectionState;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns>Bluetooth identifier address of the connected SB device</returns>
	public string GetConnectedSBAddress()
	{
		return addressConnectedSB;
	}

	/// <summary>
	/// Returns the MCUID of the SB device
	/// MCUID is a 32 character string unique to every SB device. 
	/// This can be retrieved only after connection
	/// </summary>
	/// <returns>MCUID of the SB device</returns>
	public string GetConnectedSBMCUID()
	{
		return mcuid;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns>Boolean value stating whether the NSB library system is receiving EEG and other data from the SB device</returns>
	public bool GetReceiveEEGState()
	{
		return bEegStarted;
	}

	/// <summary>
	/// Sets the NSB library system to enable or disable receiving EEG and other data from the SB device
	/// </summary>
	/// <param name="send">Sending state</param>
	public void SetReceiveEEG(bool send)
	{
		Debug.Log("NSB Set receive EEG " + send);
		if (send)
		{
			NSB_EEG.instance.EEG_Start();
			bEegStarted = true;
		}
		else
		{
			NSB_EEG.instance.EEG_Stop();
			bEegStarted = false;
		}
	}

	float GT = 0.1f, FST = 0.08f;
	/// <summary>
	/// Sets gamma threshold for useful data
	/// </summary>
	/// <param name="threshold"></param>
	public void SetGammaThreshold(float threshold)
	{
		GT = threshold;
		EEGController.getInstance().SetEnvironmentThreshold(GT.ToString() + "," + FST.ToString());
	}

	/// <summary>
	/// Sets 5060hz threshold for useful data
	/// </summary>
	/// <param name="threshold"></param>
	public void Set5060Threshold(float threshold)
	{
		FST = threshold;
		EEGController.getInstance().SetEnvironmentThreshold(GT.ToString() + "," + FST.ToString());
	}


	/// <summary>
	/// Returns the accelerometer values.
	/// </summary>
	/// <param name="dimension"></param>
	/// <returns>Parameter: dimension: 0 - X axis, 1 - Y axis, 2 - Z axis</returns>
	public int GetAccel(int dimension)
	{
		if (dimension < 3)
			return accelerometerData[dimension];
		else
			return 999; //Error; dimension cover X, Y, Z only.
	}

	/// <summary>
	/// Gets current 5060hz power
	/// </summary>
	/// <param name="channel">The channel to read 5060hz power from</param>
	/// <returns></returns>
	public float GetFiftySixtyReading(int channel)
	{
		if ((channel >= 0 && channel < 4))
			return fiftysixtyReading[channel];
		else
			return 0f;
	}

	/// <summary>
	/// Gets current gamma power
	/// </summary>
	/// <param name="channel">The channel to read gamma power from</param>
	/// <returns></returns>
	public float GetGammaReading(int channel)
	{
		if ((channel >= 0 && channel < 4))
			return gammaReading[channel];
		else
			return 0f;
	}

	/// <summary>
	/// Gets current mean power
	/// </summary>
	/// <param name="channel">The channel to read mean power from</param>
	/// <returns></returns>
	public float GetMeanReading(int channel)
	{
		if ((channel >= 0 && channel < 4))
			return meanReading[channel];
		else
			return 0f;
	}

	/// <summary>
	/// Returns the attention level value, range of 0 - 1.
	/// This is calculated from the latest set of EEG data received from the SB device.
	/// </summary>
	/// <returns></returns>
	public float GetAttention()
	{
		return mentalStateData[1];
	}

	/// <summary>
	/// Returns the relaxation level value, range of 0 - 1.
	/// This is calculated from the latest set of EEG data received from the SB device.
	/// </summary>
	/// <returns></returns>
	public float GetRelaxation()
	{
		return mentalStateData[0];
	}

	/// <summary>
	/// Returns the mental workload level value, range of 0 - 1.
	/// This is calculated from the latest set of EEG data received from the SB device.
	/// </summary>
	/// <returns></returns>
	public float GetMentalWL()
	{
		return Mathf.Clamp(mentalStateData[2], 0f, 1f);
	}

	/// <summary>
	/// Returns if the signal from each sensor on SB device is receiving EEG signal
	/// Parameter: channel: 0 to 3 represents sensor from right to left.
	/// </summary>
	/// <param name="channel"></param>
	/// <returns></returns>
	public bool GetChannelStatus(int channel)
	{
		if (channel < 4)
			return channelStatus[channel];
		else
			return false;
	}

	/// <summary>
	/// Returns if the Bluetooth connection is good.
	/// Interference from other Bluetooth signal emitters can lead to data loss 
	/// </summary>
	/// <returns></returns>
	public bool GetGoodBTConnection()
	{
		return goodBTConnection;
	}

	/// <summary>
	/// Returns if the EEG signal received is acceptable. 
	/// Signal noise from body movement, or insufficient skin contact at the sensor electrode give poor quality signal. 
	/// Under this condition, the results from signal processing may not be accurate.
	/// </summary>
	/// <returns></returns>
	public bool GetSignalReady()
	{
		return signalReady;
	}

	/// <summary>
	/// Returns the battery kevel, range 0 - 1
	/// </summary>
	/// <returns></returns>
	public string GetConnectedSBBattery()
	{
		return batteryLevel;
	}

	/// <summary>
	/// Returns received frequency band data
	/// </summary>
	/// <returns></returns>
	public float GetFrequencyBand(int channel, int band)
	{
		//returns a float of the power spectral density(PSD). ie. 0.3.
		//the sum of the PSD for all the 5 bands should be 1.
		if ((band >= 0 && band < 5) &&
		   (channel >= 0 && channel < 4))
			return frequencyBandData[channel, band];
		else
			return 0f;
	}

	/// <summary>
	/// Returns received EEG data
	/// </summary>
	/// <returns></returns>
	public int[] GetRawEEG()
	{
		return rawEEGData;
	}

	/// <summary>
	/// Returns validity of authentication. If true, EEG data can be received from the Senzeband
	/// </summary>
	/// <returns>Authentication validity</returns>
	public bool GetAuthenticationResult()
	{
		return authenticationResult;
	}

	/// <summary>
	/// Returns human readable string describing auth status. i.e No Internet or Devcode not specified
	/// </summary>
	/// <returns>Authentication status description</returns>
	public string GetAuthenticationStatus()
	{
		return authenticationStatus;
	}

	//CALLBACKS to be sent to NSB_BLE and NSB_EEG
	void Log(string error)
	{
		Debug.Log("NSB SDK LOG: " + error);
	}

	/// <summary>
	/// </summary>
	/// <param name="authenStatus">Authentication status from Senzeband plugin</param>
	void GetAuthenticationStatus(string authenStatus)
	{
		//"200" or "Successful" - authentication is valid
		//"No internet connection	- no connection to server, authentication status will fail
		//"Unsuccessful" or others - authentication has expired 
		Debug.Log("NSB Authentication status is " + authenStatus);
		authenticationStatus = authenStatus;
	}

	/// <summary>
	/// </summary>
	/// <param name="authenStatus">Authentication result from Senzeband plugin</param>
	void GetAuthenticationResult(bool authenResult)
	{
		//TRUE - Within valid authentication period
		//FALSE - Not within valid authentication period
		Debug.Log("NSB Authentication result is " + authenResult);
		authenticationResult = authenResult;
	}

	/// <summary>
	/// Callback when NSB SDK Library initialisation is complete.
	/// </summary>
	void InitComplete()
	{
		//reset all data
		bIsScanning = true;
		addressConnectingSB = string.Empty;
		addressConnectedSB = string.Empty;
		mcuid = string.Empty;
		connectionStatus = string.Empty;
		bEegStarted = false;
		connectionState = 0;

		ClearList();
		bIsInitCompleted = true;

	}

	/// <summary>
	/// Updates the status of the connection of the SenzeBand device
	/// </summary>
	/// <param name="status"></param>
	void GetDeviceStatus(string status)
	{
		//Updates the status of the connection of the SenzeBand device
		Debug.Log("NSB SB connection status changed: " + status + " for address: " + addressConnectingSB);
		connectionStatus = status;
		if (status == NSB_BLE.instance.getConnectedString())
		{
			
			ConnectionSucceed(addressConnectingSB);
			//connected!		also see ConnectionSucceed()
		}
		else if (status == NSB_BLE.instance.getNotConnectedString())
		{
			//disconnected!		also see ConnectionFailed()
		}
		else if (status == NSB_BLE.instance.getConnectingString())
		{
			//connecting!
		}
	}

	/// <summary>
	/// Updates the status of Bluetooth settings
	/// </summary>
	/// <param name="status"></param>
	void GetBTStatus(bool status)
	{
		//Updates the status of Bluetooth settings
		Debug.Log("NSB BT setting changed: " + status);
		bluetoothStatus = status;

		//To reset the available device list when BT state is changed
		ClearList();
		//If BT is turned off, disconnect existing connections - reset.
		if (!status)
		{
			DisconnectSB();
		}

	}

	/// <summary>
	/// Updates the battery level
	/// </summary>
	/// <param name="battery"></param>
	void GetBattery(string battery)
	{
		//Updates the battery level
		batteryLevel = battery;

		Debug.Log("Battery level: " + battery);
	}

	/// <summary>
	/// During scanning, an available SenzeBand is found
	/// </summary>
	/// <param name="address"></param>
	void FoundAvailableDevice(string address)
	{
		//During scanning, an available SenzeBand is found
		Debug.Log("NSB found available device : " + address + " ; list of " + listAvailableDevices.Count);
		string tempString = string.Copy(address);
		foreach (string t in listAvailableDevices)
		{
			if (string.Equals(t, tempString))
			{
				//It is a repeated SB identity
				Debug.Log("NSB repeated device ID.  Not adding to list");
				return;
			}
		}
		listAvailableDevices.Add(tempString);

	}

	/// <summary>
	/// Gets from NSB to store the MCU ID of the SB device
	/// </summary>
	/// <returns></returns>
	IEnumerator co_PullMCUID()
    {
        yield return new WaitForSeconds(1.0f);
        if (connectionStatus == NSB_BLE.instance.getConnectedString() && addressConnectedSB != string.Empty)
        {

            while (mcuid == string.Empty)
            {
                yield return new WaitForSeconds(0.5f);

                //Gets from NSB to store the MCU ID of the SB device
                mcuid = NSB_BLE.instance.getMCUID(addressConnectedSB);
                if (mcuid.Length < 8)
                    mcuid = string.Empty;
                Debug.Log("NSB Pull mcuid: " + mcuid);

                if (connectionStatus != NSB_BLE.instance.getConnectedString() || addressConnectedSB == string.Empty)
                    break;
            }
        }
    }

	/// <summary>
	/// Callback for when the connection process is successful - SenzeBand is connected.
	/// </summary>
	/// <param name="address"></param>
	void ConnectionSucceed(string address)
    {
		if (SeparateThread)
		{
			System.Action<string> func = ConnectionSucceed;
			QueueInvoke(func, address);
			return;
		}

		//Callback for when the connection process is successful - SenzeBand is connected.
		Debug.Log("NSB connection succeed : " + address);
        addressConnectedSB = address;
        addressConnectingSB = string.Empty;
        bEegStarted = false;
        StartCoroutine(co_PullMCUID());
        //NSB_EEG.instance.EEG_Start();

        connectionSuccessfulCallback.Invoke();
    }

	/// <summary>
	/// Callback after a successful connection, when the connection is broken
	/// </summary>
	/// <param name="address"></param>
	void ConnectionBroken(string address)
    {
		if (SeparateThread)
		{
			System.Action<string> func = ConnectionBroken;
			QueueInvoke(func, address);
			return;
		}

		//Callback for when the connection process fails - SenzeBand is NOT connected.
		//OR after a successful connection, when the connection is broken
		Debug.Log("NSB connection broken : " + address);
        addressConnectedSB = string.Empty;
        addressConnectingSB = string.Empty;
        mcuid = string.Empty;
        bEegStarted = false;
        ClearList();

        connectionBrokenCallback.Invoke();
    }

	/// <summary>
	/// Callback for when the connection process fails - SenzeBand is NOT connected
	/// </summary>
	/// <param name="address"></param>
	void ConnectionFailed(string address)
    {
		if (SeparateThread)
		{
			System.Action<string> func = ConnectionFailed;
			QueueInvoke(func, address);
			return;
		}

		//Callback for when the connection process fails - SenzeBand is NOT connected.
		//OR after a successful connection, when the connection is broken
		Debug.Log("NSB connection failed : " + address);
        addressConnectedSB = string.Empty;
        addressConnectingSB = string.Empty;
        mcuid = string.Empty;
        bEegStarted = false;
        ClearList();
        
        connectionFailedCallback.Invoke();
    }

	/// <summary>
	/// Receives attention data from plugin
	/// </summary>
	/// <param name="attention"></param>
	void grabAttention(float attention)
	{
		mentalStateData[1] = attention;
	}

	/// <summary>
	/// Receives relaxation data from plugin
	/// </summary>
	/// <param name="relaxation"></param>
	void grabRelaxation(float relaxation)
	{
		mentalStateData[0] = relaxation;
	}

	/// <summary>
	/// Receives mental workload data from plugin
	/// </summary>
	/// <param name="mentalWorkload"></param>
	void grabMentalWorkload(float mentalWorkload)
	{
		mentalStateData[2] = mentalWorkload;
	}

	/// <summary>
	/// Receives accelerometer data from plugin
	/// </summary>
	/// <param name="acc"></param>
	void grabAccelerometer(float[] acc)
	{
		if (acc.Length == 3)
		{
			accelerometerData[0] = (int)acc[0];
			accelerometerData[1] = (int)acc[1];
			accelerometerData[2] = (int)acc[2];
		}
		else
			Debug.Log("NSB accelerometer has incorrect data");
	}

	/// <summary>
	/// Receives channel status data from plugin
	/// </summary>
	/// <param name="chnStatus"></param>
	void grabChannelStatus(bool[] chnStatus)
	{
		if (chnStatus.Length == 4)
		{
			channelStatus[0] = chnStatus[0];
			channelStatus[1] = chnStatus[1];
			channelStatus[2] = chnStatus[2];
			channelStatus[3] = chnStatus[3];

			Debug.LogFormat("Received channel status: {0} {1} {2} {3}", channelStatus[0], channelStatus[1], channelStatus[2], channelStatus[3]);
		}
		else
			Debug.Log("NSB channelStatus has incorrect data");

		var data = NSB_EEG.instance.GetEnvironmentData();

		var parameters = data.Split(',');
		if (parameters.Length >= 15)
		{
			float[] gamma = new float[4];
			float[] mean = new float[4];
			float[] fiftysixty = new float[4];

			gamma[0] = gamma[1] = gamma[2] = gamma[3] = 0;
			try
			{
				gamma[0] = float.Parse(parameters[1]);
				gamma[1] = float.Parse(parameters[2]);
				gamma[2] = float.Parse(parameters[3]);
				gamma[3] = float.Parse(parameters[4]);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}

			mean[0] = mean[1] = mean[2] = mean[3] = 0;
			try
			{
				mean[0] = float.Parse(parameters[6]);
				mean[1] = float.Parse(parameters[7]);
				mean[2] = float.Parse(parameters[8]);
				mean[3] = float.Parse(parameters[9]);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}

			fiftysixty[0] = fiftysixty[1] = fiftysixty[2] = fiftysixty[3] = 0;
			try
			{
				fiftysixty[0] = float.Parse(parameters[11]);
				fiftysixty[1] = float.Parse(parameters[12]);
				fiftysixty[2] = float.Parse(parameters[13]);
				fiftysixty[3] = float.Parse(parameters[14]);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}

			gammaReading = gamma;
			meanReading = mean;
			fiftysixtyReading = fiftysixty;

			//Debug.LogFormat("Received environment data\n gamma: {0} {1} {2} {3}\n mean: {4} {5} {6} {7}\n 5060: {8} {9} {10} {11}",
			//gamma[0], gamma[1], gamma[2], gamma[3],
			//mean[0], mean[1], mean[2], mean[3],
			//fiftysixty[0], fiftysixty[1], fiftysixty[2], fiftysixty[3]);
		}
	}

	/// <summary>
	/// Returns if the Bluetooth connection is good.
	/// Interference from other Bluetooth signal emitters can lead to data loss 
	/// </summary>
	/// <returns></returns>
	public void grabGoodConnection(bool goodConnection)
	{
		goodBTConnection = goodConnection;
	}

	/// <summary>
	/// Receives signal ready data from plugin
	/// </summary>
	/// <param name="_signalReady"></param>
	void grabSignalReady(bool _signalReady)
	{
		signalReady = _signalReady;
	}

	/// <summary>
	/// Receives MCUID data from plugin
	/// </summary>
	/// <param name="data"></param>
	void grabMCUID(string data)
	{
		mcuid = data;
	}

	/// <summary>
	/// Receives Frequency band data from plugin
	/// </summary>
	/// <param name="frequencyBand"></param>
	void grabFrequencyBand(float[,] frequencyBand)
	{
		//check if returned data is correct length
		if (frequencyBand.Length != frequencyBandData.Length)
			return;

		for (int j = 0; j < frequencyBandData.GetLength(0); ++j)
		{
			for (int i = 0; i < frequencyBandData.GetLength(1); ++i)
				frequencyBandData[j, i] = frequencyBand[j, i];
		}
	}

	/// <summary>
	/// Receives EEG data from plugin
	/// </summary>
	/// <param name="rawEEG"></param>
	void grabRawEEG(int[] rawEEG)
	{
		if (SeparateThread)
		{
			System.Action<int[]> func = grabRawEEG;
			QueueInvoke(func, rawEEG);
			return;
		}

		Debug.Log("Received EEG data");

		//Raw EEG received is integer values where 1 unit = 1 * 0.61 microVolt
		if (rawEEG.Length == rawEEGData.Length)
			rawEEGData = (int[])rawEEG.Clone();

		if (bEegStarted && rawdataGrabbed != null) //only trigger event if eeg receiving is ON
			rawdataGrabbed.Invoke();    //announce that new set of data has been fetched			 

		if (rawdataGrabbed == null)
			Debug.Log("rawdataGrabbed not assigned");
	}

	/// <summary>
	/// Receives grabEnvironmentData from plugin
	/// </summary>
	/// <param name="data"></param>
	void grabEnvironmentData(string data)
	{
		var parameters = data.Split(',');
		if (parameters.Length >= 15)
		{
			float[] gamma = new float[4];
			float[] mean = new float[4];
			float[] fiftysixty = new float[4];

			gamma[0] = float.Parse(parameters[1]);
			gamma[1] = float.Parse(parameters[2]);
			gamma[2] = float.Parse(parameters[3]);
			gamma[3] = float.Parse(parameters[4]);

			mean[0] = float.Parse(parameters[6]);
			mean[1] = float.Parse(parameters[7]);
			mean[2] = float.Parse(parameters[8]);
			mean[3] = float.Parse(parameters[9]);

			fiftysixty[0] = float.Parse(parameters[11]);
			fiftysixty[1] = float.Parse(parameters[12]);
			fiftysixty[2] = float.Parse(parameters[13]);
			fiftysixty[3] = float.Parse(parameters[14]);

			gammaReading = gamma;
			meanReading = mean;
			fiftysixtyReading = fiftysixty;

			Debug.LogFormat("Received environment data\n gamma: {0} {1} {2} {3}\n mean: {4} {5} {6} {7}\n 5060: {8} {9} {10} {11}",
			gamma[0], gamma[1], gamma[2], gamma[3],
			mean[0], mean[1], mean[2], mean[3],
			fiftysixty[0], fiftysixty[1], fiftysixty[2], fiftysixty[3]);
		}
	}

	//Other Internal Support functions

	/// <summary>
	/// Clearing the list of SenzeBands available for connection. Not if it is in Connecting state.
	/// </summary>
	public void ClearList()
	{
		//Clearing the list of SenzeBands available for connection. Not if it is in Connecting state.
		if (connectionState != 1)
		{
			Debug.Log("NSB Clearlist ");
			listAvailableDevices.Clear();
			NSB_BLE.instance.clearList();
		}
	}
#endif
}
