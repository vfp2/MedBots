using UnityEngine;
using System;

#if (UNITY_STANDALONE_WIN || UNITY_EDITOR) && NET_4_6 && !UNITY_EDITOR_OSX
using NSB_SDK_WINDOWS;
#elif UNITY_ANDROID
using NSB_SDK_ANDROID;
#elif UNITY_IOS
using NSB_SDK_IOS;
#endif

/// <summary>
/// This file consists of calls to the NSB_BLE as well as functions that the plugins will do callBacks from
/// </summary>
public class NSB_BLE : MonoBehaviour
{
	//Action class to store the actions, in order to queue them to run in Update() thread
	private class ActionClass<T>
	{
		public ActionClass(Action<T> funcIn, T paramIn)
		{
			func = funcIn;
			param = paramIn;
		}

		public Action<T> func;
		public T param;
	}

	public static NSB_BLE instance;

	//BLE callback delegates
	public delegate void voidReturnVoid();
	public delegate void boolReturnVoid(bool boolIn);
	public delegate void stringReturnVoid(string stringIn);
	public delegate void stringStringReturnVoid(string stringIn1, string stringIn2);


	//BLE connection and callback
	private boolReturnVoid BLEBluetoothStatusUpdate;      //Callback that will be invoked when bluetooth has been connected or disconnected
	private stringReturnVoid BLEDeviceStatusUpdate;         //Callback that will be invoked when there is any change in device connection.

#if UNITY_IOS
	private stringReturnVoid BLEBatteryUpdate; 				//Callback that will be invoked when the battery level is updated. This is called frequently.
#endif

	private voidReturnVoid BLEReady;

	//Callback that will be invoked when the bluetooth has finished setting up. Will be changed to null once its called
	private stringReturnVoid BLEFinishConnecting;           //Callback that will be invoked when the device has been connected. Will be changed to null once its called
	private stringReturnVoid BLEBreakConnection;            //Callback that will be invoked when the device has broken connection. Will be changed to null once its called
	private stringReturnVoid BLEFailConnection;             //Callback that will be invoked when the device has failed to connect. Will be changed to null once its called



	//Functions that we need to force to the UI Thread. If we don't there may be a JNI stale object error when threading is used
	private ActionClass<string> disconnectAction = null;
	private ActionClass<string> connectAction = null;

	// Use this for initialization
	void Awake()
	{
		if (instance != null)
			Destroy(this.gameObject);

		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(this.gameObject); //We need this object to persist throughout the lifetime of the program
		}
	}

	void Update()
	{
		if (disconnectAction != null)
		{
			disconnectAction.func(disconnectAction.param);
			disconnectAction = null;
		}

		if (connectAction != null)
		{
			connectAction.func(connectAction.param);
			connectAction = null;
		}
	}

	#region initialize BT
	/******************************************************************************/
	/*!
	\fn  void initializeBT()
	\brief Initializes the Bluetooth. Call this only once.
	*/
	/******************************************************************************/
	public void initializeBT()
	{
#if (UNITY_IOS && !UNITY_EDITOR)
		Debug.Log("initializeBT: UNITY_IOS");
#elif (UNITY_ANDROID && !UNITY_EDITOR)
		Debug.Log("initializeBT: UNITY_ANDROID");
#elif (UNITY_STANDALONE_WIN)
		Debug.Log("initializeBT: UNITY_WINDOWS");
#endif

		BLEController.getInstance().putBLE_SetDeviceConnectStatus(connectedSucceed);
		BLEController.getInstance().putBLE_SetDeviceConnectFailStatus(connectionFail);
		BLEController.getInstance().putBLE_SetDeviceNotConnectStatus(connectionBroken);
		BLEController.getInstance().addBTDeviceConnectionDelegate(updateBTConnectionStatus);
		BLEController.getInstance().putCompletedBTInit(completedBTInit);
		BLEController.getInstance().assignBTStatusCallBack(getBTStatus);
		BLEController.getInstance().initializeBT(this.gameObject.name);
	}

	/******************************************************************************/
	/*!
	\fn  void initializeBT(voidReturnVoid BTInitFinishCallBack, stringReturnVoid BLE_DeviceStatusUpdate, stringReturnVoid BLE_BluetoothStatusCallBack, string devcode)
	\brief Initializes the Bluetooth with callback functions as well as assign the developer code. Call this only once.
	\param BTInitFinishCallBack
	A delegate that will be called once the initialization process has been done. Will be set to null once its called.
	\param BLE_DeviceStatusUpdate
	A delegate that will be called if there is any change in the NEEURO device connection
	\param BLE_BluetoothStatusCallBack
	A delegate that will be called if the bluetooth status has changed. 
	\param devcode
	Developer code used for authentication
	*/
	/******************************************************************************/
	public void initializeBT(voidReturnVoid BTInitFinishCallBack, stringReturnVoid BLE_DeviceStatusUpdate, boolReturnVoid BLE_BluetoothStatusCallBack, string devcode)
	{
#if (UNITY_IOS && !UNITY_EDITOR)
		Debug.Log("initializeBT: UNITY_IOS");
#elif (UNITY_ANDROID && !UNITY_EDITOR)
		Debug.Log("initializeBT: UNITY_ANDROID");
#elif (UNITY_STANDALONE_WIN)
		Debug.Log("initializeBT: UNITY_WINDOWS");
#endif

		BLEReady += BTInitFinishCallBack;
		BLEDeviceStatusUpdate += BLE_DeviceStatusUpdate;
		BLEBluetoothStatusUpdate += BLE_BluetoothStatusCallBack;

		BLEController.getInstance().putBLE_SetDeviceConnectStatus(connectedSucceed);
		BLEController.getInstance().putBLE_SetDeviceConnectFailStatus(connectionFail);
		BLEController.getInstance().putBLE_SetDeviceNotConnectStatus(connectionBroken);
		BLEController.getInstance().addBTDeviceConnectionDelegate(updateBTConnectionStatus);
		BLEController.getInstance().putCompletedBTInit(completedBTInit);
		BLEController.getInstance().assignBTStatusCallBack(getBTStatus);
		BLEController.getInstance().initializeBT(this.gameObject.name, devcode);
	}

    /******************************************************************************/
    /*!
	\fn  void getBTStatus
	\brief A delegate that will be called if the bluetooth status has changed. 
	\param b
	New Bluetooth state
	*/
    /******************************************************************************/
    public void getBTStatus(bool b)
	{
		BLEBluetoothStatusUpdate(b);
	}

    /******************************************************************************/
    /*!
	\fn  void shutdownBT
	\brief
    Tells the SDK to release all resources and shut down
	*/
    /******************************************************************************/
    public void shutdownBT()
	{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
		Debug.Log("Shutting down: UNITY_WINDOWS");
		BLEController.getInstance().shutdownBT();
#endif
	}

	/******************************************************************************/
	/*!
	\fn  void populateInitFinishCallBack(BLECallBack BTInitFinishCallBack, bool unpopulate = false)
	\brief Adds in delegates that will be called once the initialization process has been done. Will be set to null once its called.
	To be used together with initializeBT()
	\param BTInitFinishCallBack
	A delegate that will be called once the initialization process has been done. Will be set to null once its called.
	\param unpopulate
	If this is set to true, then the delegate will be removed instead of being added instead.
	*/
	/******************************************************************************/
	public void populateInitFinishCallBack(voidReturnVoid BTInitFinishCallBack, bool unpopulate = false)
	{
		if (unpopulate)
			BLEReady -= BTInitFinishCallBack;
		else
			BLEReady += BTInitFinishCallBack;
	}

	/******************************************************************************/
	/*!
	\fn  void populateBLE_DeviceStatusUpdateCallBack(BLEConnectedCallBack BLE_DeviceStatusUpdate, bool unpopulate = false)
	\brief Adds in delegates that will be called if there is any change in the NEEURO device connection
	To be used together with initializeBT()
	\param BLE_DeviceStatusUpdate
	A delegate that will be called if there is any change in the NEEURO device connection. 
	\param unpopulate
	If this is set to true, then the delegate will be removed instead of being added instead.
	*/
	/******************************************************************************/
	public void populateBLE_DeviceStatusUpdateCallBack(stringReturnVoid BLE_DeviceStatusUpdate, bool unpopulate = false) //Changed from BLEConnectedCallBack, but the usage is the same
	{
		//BLEController.MyBLEStringCallBack temp = new BLEController.MyBLEStringCallBack(BLE_DeviceStatusUpdate);
		//BLEController.getInstance().addBTDeviceConnectionCallBack(BLE_DeviceStatusUpdate, unpopulate);

		if (unpopulate)
			BLEDeviceStatusUpdate -= BLE_DeviceStatusUpdate;
		else
			BLEDeviceStatusUpdate += BLE_DeviceStatusUpdate;
	}

	/******************************************************************************/
	/*!
	\fn  void populateBLE_BluetoothStatusUpdateCallBack(BLEConnectedCallBack BLE_BluetoothStatusCallBack, bool unpopulate = false)
	\brief Adds in delegates that will be called if there is any change in the bluetooth status.
	To be used together with initializeBT()		
	\param BLE_BluetoothStatusCallBack
	A delegate that will be called if there is any change in the bluetooth connection.
	\param unpopulate
	If this is set to true, then the delegate will be removed instead of being added instead.
	*/
	/******************************************************************************/
	public void populateBLE_BluetoothStatusUpdateCallBack(boolReturnVoid BLE_BluetoothStatusCallBack, bool unpopulate = false)
	{
		if (unpopulate)
			BLEBluetoothStatusUpdate -= BLE_BluetoothStatusCallBack;
		else
			BLEBluetoothStatusUpdate += BLE_BluetoothStatusCallBack;
	}

	/******************************************************************************/
	/*!
	\fn  assignAuthenticationStatusDelegate(BLEController.MyBLEStringCallBack callBackFunc)
	\brief  Passes in a delegate to grab the authentication status value. Returns the response code or if there's no internet connection
	\param callBackFunc
	CallBack to return the authentication status once its ready.
	*/
	/******************************************************************************/
	public void assignAuthenticationStatusDelegate(stringReturnVoid authenticationCallBackFunc)
	{
		BLEController.getInstance().assignAuthenticateStatusDelegate(new EEGController.EEGStringCallBack(authenticationCallBackFunc));
	}

	/******************************************************************************/
	/*!
	\fn  assignAuthenticationResultDelegate(BLEController.MyBLEBoolCallBack callBackFunc)
	\brief  Passes in a delegate to grab the authentication result. Returns only if there is a change in the authentication. 
	\param callBackFunc
	CallBack to return the authentication result once its ready.
	*/
	/******************************************************************************/
	boolReturnVoid auth_res_stat = null;
	public void assignAuthenticationResultDelegate(boolReturnVoid authenticationResultCallBackFunc)
	{
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
		BLEController.getInstance().assignAuthenticateResultDelegate(new EEGController.btBoolCallback(authenticationResultCallBackFunc));
#else
		auth_res_stat += authenticationResultCallBackFunc;
		BLEController.getInstance().assignAuthenticateResultDelegate(new EEGController.btBoolCallback(authenticationResultCallBackFunc));
#endif
	}

	/******************************************************************************/
	/*!
	\fn  public void assignScanCallBack(stringReturnVoid scanCallBackIn)
	\brief Adds in delegates that will be called during scanning, when an available (unconnected) SenzeBand is found 
	\param scanCallBackIn
	A delegate that will be called to pass the address of the SenzeBand found
	*/
	/******************************************************************************/
	public void assignScanCallBack(stringReturnVoid scanCallBackIn)
	{
		BLEController.getInstance().assignScanCallBack(new EEGController.EEGStringCallBack(scanCallBackIn));
	}

	/******************************************************************************/
	/*!
	\fn  authenticateUser()
	\brief  Authenticates the user via the developer code that is passed in from initialize. Only call this after initializeBT. Upon connection with a senzeBand, this will automatically get called as well
	*/
	/******************************************************************************/
	public void authenticateUser()
	{
		BLEController.getInstance().authenticateUser();
	}


	/******************************************************************************/
	/*!
	\fn  public void assignErrorLogDelegate(stringReturnVoid errorLogCallBack)
	\brief   
	\param errorLogCallBack
	*/
	/******************************************************************************/
	public void assignErrorLogDelegate(stringReturnVoid errorLogCallBack)
	{
		BLEController.getInstance().assignErrorLogCallBack(new EEGController.EEGStringCallBack(errorLogCallBack));
	}

	#endregion

	#region BluetoothConnection
	/******************************************************************************/
	/*!
	\fn  bool connectBT (string blueToothID, BLEConnectedCallBack callBackFunction = null, BLEConnectedCallBack connectionBroken = null)	
	\brief 	Connects the NEEURO device with the blueToothID to the app. We only allow one device to connect at any point of time.
	\param blueToothID
	The blueToothID of the device you want to connect to. This can be gotten in the GetDeviceList function.
	\param callBackFunction
	A callback that is executed upon a successful connection. Will be set to null after its called.
	\param connectionBroken
	A callback that is executed when the connection has been disconnected or if it fails to connect. Will be set to null after its called.
	\return
	bool
	*/
	/******************************************************************************/
	public bool connectBT(string blueToothID, stringReturnVoid connectionSuccessCallBack = null, stringReturnVoid connectionBrokenCallBack = null, stringReturnVoid connectionFailCallBack = null)
	{
		//Only allow one active connecting connection at any point of time
		if (getDeviceConnectionStatus() == BLEController.BLEConstantsExternal.connecting)
		{
			return false;
		}

		if (connectionSuccessCallBack != null)
			BLEFinishConnecting = connectionSuccessCallBack;

		if (connectionBrokenCallBack != null)
			BLEBreakConnection = connectionBrokenCallBack;

		if (connectionFailCallBack != null)
			BLEFailConnection = connectionFailCallBack;

		//connectAction = new ActionClass<string>(BLEController.getInstance().connectBT, blueToothID);
		BLEController.getInstance().connectBT(blueToothID);
		//BLEController.getInstance().setBluetoothConnection(blueToothID);


		return true;
	}

	/******************************************************************************/
	/*!
	\fn  void populateBLE_FinishConnecting(BLEConnectedCallBack callBackFunction, bool unpopulate = false)
	\brief 	Adds in delegates that will be called if the connection to the NEEURO device is successful.
	\param callBackFunction
	The delegate you want to add.
	\param unpopulate
	If its set to true, the callBackFunction will be taken out instead of adding in. Default parameter is false.
	*/
	/******************************************************************************/
	public void populateBLE_FinishConnecting(stringReturnVoid callBackFunction, bool unpopulate = false)
	{
		if (unpopulate)
			BLEFinishConnecting = null;//-= callBackFunction;
		else
			BLEFinishConnecting = callBackFunction;
	}

	/******************************************************************************/
	/*!
	\fn  void populateBLE_BreakConnection(BLEConnectedCallBack callBackFunction, bool unpopulate = false)
	\brief 	Adds in delegates that will be called if the connection to the NEEURO device has been disconnected.
	\param callBackFunction
	The delegate you want to add.
	\param unpopulate
	If its set to true, the callBackFunction will be taken out instead of adding in. Default parameter is false.
	*/
	/******************************************************************************/
	public void populateBLE_BreakConnection(stringReturnVoid callBackFunction, bool unpopulate = false)
	{
		if (unpopulate)
			BLEBreakConnection = null;//-= callBackFunction;
		else
			BLEBreakConnection = callBackFunction;
	}

	/******************************************************************************/
	/*!
	\fn  void populateBLE_FailConnection(BLEConnectedCallBack callBackFunction, bool unpopulate = false)
	\brief 	Adds in delegates that will be called if the connection to the NEEURO device fails to connect.
	\param callBackFunction
	The delegate you want to add.
	\param unpopulate
	If its set to true, the callBackFunction will be taken out instead of adding in. Default parameter is false.
	*/
	/******************************************************************************/
	public void populateBLE_FailConnection(stringReturnVoid callBackFunction, bool unpopulate = false)
	{
		if (unpopulate)
			BLEFailConnection = null;//-= callBackFunction;
		else
			BLEFailConnection = callBackFunction;
	}

	/******************************************************************************/
	/*!
	\fn  string getSDKVersion()
	\brief Returns the library and the plugin version.
	*/
	/******************************************************************************/
	public string getSDKVersion()
	{
		return BLEController.getInstance().getSDKVersion();
	}

	/******************************************************************************/
	/*!
	\fn  void disconnectBT(string deviceName)
	\brief Disconnects the NEEURO device with the same bluetoothID as the deviceName
	*/
	/******************************************************************************/
	public void disconnectBT(string deviceName)
	{
		disconnectAction = new ActionClass<string>(BLEController.getInstance().disconnectBT, deviceName);
	}

		#endregion

		#region getStatus


	/******************************************************************************/
	/*!
	\fn string getBTStatus()
	\brief 	Get the status of the Bluetooth connection. If bluetooth is disabled on the device, then this will be Device_Off.
	\return
	*/
	/******************************************************************************/
	public bool getBTStatus()
	{
		return BLEController.getInstance().getBTStatus();
	}

	/******************************************************************************/
	/*!
	\fn string getDeviceConnectionStatus()
	\brief 	Get the status of the NEEURO device connection.
	\return
	*/
	/******************************************************************************/
	public string getDeviceConnectionStatus()
	{
		return BLEController.getInstance().getBTConnectionStatus();
	}

	/******************************************************************************/
	/*!
	\fn string getCurrentConnectedDevice()
	\brief 	Get the ID of the current connected NEEURO device
	\return
	string
	*/
	/******************************************************************************/
	public string getCurrentConnectedDevice()
	{
		return BLEController.getInstance().getCurrentConnectedDevice();
	}

	/******************************************************************************/
	/*!
	\fn void clearList()
	\brief  Clears the list of discovered NEEURO devices. 
	*/
	/******************************************************************************/
	public void clearList()
	{
		BLEController.getInstance().clearList();
	}

	/******************************************************************************/
	/*!
	\fn void getMCUID(BLEConnectedCallBack mcuIDCallBack)
	\brief  Gets the MCUID of the NEEURO device. This will only work if there is a device connected.
	\param address
	The address of the connected device.
	*/
	/******************************************************************************/
	public string getMCUID(string address)
	{
		Debug.Log("Got MCUID: " + address);
		return BLEController.getInstance().grabMCUID(address);
	}

	/******************************************************************************/
	/*!
	\fn void startStopScanning(bool toStart)
	\brief  Toggles the scanning of bluetooth devices
	\param toStart
	Flag to start or stop the scanning.
	*/
	/******************************************************************************/
	public void startStopScanning(bool toStart)
	{
		BLEController.getInstance().startStopScanning(toStart);
	}

	/******************************************************************************/
	/*!
	\fn string getConnectedString()
	\brief  Gets the string that is returned when the NEEURO Senzeband is connected
	\return
	string
	*/
	/******************************************************************************/
	public string getConnectedString()
	{
		return BLEController.BLEConstantsExternal.connected;
	}

	/******************************************************************************/
	/*!
	\fn string getConnectingString()
	\brief  Gets the string that is returned when the NEEURO Senzeband is connecting
	\return
	string
	*/
	/******************************************************************************/
	public string getConnectingString()
	{
		return BLEController.BLEConstantsExternal.connecting;
	}

	/******************************************************************************/
	/*!
	\fn string getNotConnectedString()
	\brief  Gets the string that is returned when the NEEURO Senzeband is not connected
	\return
	string
	*/
	/******************************************************************************/
	public string getNotConnectedString()
	{
		return BLEController.BLEConstantsExternal.notConnected;
	}

	#endregion

	private void completedBTInit(string empty)
	{
		if (BLEReady != null)
			BLEReady();
		BLEReady = null;
	}

	private void connectedSucceed(string btDetails)
	{
		Debug.Log("NSB BLE connection succeed " + btDetails);
		if (BLEFinishConnecting != null)
		{
			BLEFinishConnecting(btDetails);
			BLEFinishConnecting = null;
		}
	}

	private void connectionBroken(string name)
	{
		Debug.Log("NSB BLE connection broken " + name);
#if UNITY_IOS
		String address = name;
		if(address.Contains(">"))
			address = name.Substring(address.IndexOf(">") + 1, address.Length - address.IndexOf(">") -1);
		disconnectBT(address);				//in iOS, when SB is switched off, connection breaks and does not reset, will need to call DisconnectBT() in order to allow new connection
#endif
		if (BLEBreakConnection != null)
		{
			BLEBreakConnection(name);
			BLEBreakConnection = null;
		}
	}

	private void connectionFail(string addressAndErrorLog)
	{
		Debug.Log("NSB BLE connection failed " + addressAndErrorLog);
#if UNITY_IOS
		String address = addressAndErrorLog;
		if(address.Contains(">"))
		address = address.Substring(address.IndexOf(">") + 1, 36);	//UUID has 36 characters
		else
			address = address.Substring(0, 36);
		disconnectBT(address);				//in iOS, when SB is switched off, connection breaks and does not reset, will need to call DisconnectBT() in order to allow new connection
#endif
		if (BLEFailConnection != null)
		{
			BLEFailConnection(addressAndErrorLog);
			BLEFailConnection = null;
		}
	}

	private void updateBTConnectionStatus(string status)
	{
		BLEDeviceStatusUpdate(status);
	}
}
