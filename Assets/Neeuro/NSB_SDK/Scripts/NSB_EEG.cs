using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

#if (UNITY_STANDALONE_WIN || UNITY_EDITOR) && NET_4_6 && !UNITY_EDITOR_OSX
using NSB_SDK_WINDOWS;
#elif UNITY_ANDROID
using NSB_SDK_ANDROID;
#elif UNITY_IOS
using NSB_SDK_IOS;
#endif

/// <summary>
/// This file consists of calls to the NSB_EEG as well as functions that the plugins will do callbacks from
/// </summary>
public class NSB_EEG : MonoBehaviour
{
#if !UNITY_EDITOR_OSX
	public static NSB_EEG instance;

	public delegate void btValueCallback(float btValues);
	public delegate void stringReturnVoid(string btDetails);
	//private btValueCallback btAttentionValueDelegate;

#if UNITY_IOS
	private stringReturnVoid BLEBatteryUpdate; 				//Callback that will be invoked when the battery level is updated. This is called frequently.
#endif

	void Awake()
	{
		if (instance != null)
			Destroy(this.gameObject);

		if (instance == null)
		{
			instance = this;
		}

		DontDestroyOnLoad(this.gameObject);
	}

	#region initialize EEG

	/******************************************************************************/
	/*!
	\fn  void EEG_Start ()
	\brief Starts the EEG to grab the data from the NEEURO device
	*/
	/******************************************************************************/
	public void EEG_Start()
	{
		EEGController.getInstance().EEG_StartReceiving();
	}

	/******************************************************************************/
	/*!
	\fn  void EEG_Stop ()
	\brief Stops the grabbing of data from the NEEURO device
	*/
	/******************************************************************************/
	public void EEG_Stop()
	{
		EEGController.getInstance().EEG_StopReceiving();
	}

	#endregion

	#region assignDelegates
	/******************************************************************************/
	/*!
	\fn  void assignChannelDelegate(EEGController.btBoolArrayCallback callBackFunc)
	\brief The assigned delegate will be called each time the 4 channels update.
	\param callBackFunc
	The callBack function to execute
	*/
	/******************************************************************************/
	public void assignChannelDelegate(EEGController.btBoolArrayCallback callBackFunc)
	{
		EEGController.getInstance().assignChannelStatusDelegate(callBackFunc);
	}


	/******************************************************************************/
	/*!
	\fn  void assignGoodConnectionCheckDelegate(EEGController.btBoolCallback callBackFunc)
	\brief The assigned delegate will be called whenever the NEEURO device updates the connection checks. 
	\param callBackFunc
	The callBack function to execute
	*/
	/******************************************************************************/
	public void assignGoodConnectionCheckDelegate(EEGController.btBoolCallback callBackFunc)
	{
		EEGController.getInstance().assignGoodConnectionCheckDelegate(callBackFunc);
	}

	/******************************************************************************/
	/*!
	\fn  void assignSignalReadyStatusDelegate(EEGController.btBoolCallback callBackFunc)
	\brief The assigned delegate will be called whenever the NEEURO device receives data from Bluetooth. 
	\param callBackFunc
	The callBack function to execute
	*/
	/******************************************************************************/
	public void assignSignalReadyStatusDelegate(EEGController.btBoolCallback callBackFunc)
	{
		EEGController.getInstance().assignSignalReadyStatusDelegate(callBackFunc);
	}

	/******************************************************************************/
	/*!
	\fn  void assignMCUIDDelegate(EEGController.btBoolCallback callBackFunc)
	\brief The assigned delegate will be called whenever the NEEURO plugin wants to update the device MCUID
	The callBack function to execute
	*/
	/******************************************************************************/
	public void assignMCUIDDelegate(EEGController.EEGStringCallBack callBackFunc)
	{
		EEGController.getInstance().assignMCUIDCallBack(callBackFunc);
	}

	/******************************************************************************/
	/*!
	\fn  void assignRawDataDelegate(MyEEGHandler.btFloatArrayCallback callBackFunc)
	\brief The assigned delegate will be called when the raw EEG signal is ready. The raw signal has is an array of 1000 floats.
		   The first 250 floats belong to one channel, next 250 floats belong to another channel and so on.
	\param callBackFunc
	The callBack function to execute
	*/
	/******************************************************************************/
	public void assignRawDataDelegate(EEGController.btIntArrayCallback callBackFunc)
	{
		EEGController.getInstance().assignRawDataDelegate(callBackFunc);
	}

	/******************************************************************************/
	/*!
	\fn  void assignEnvironmentDataDelegate(MyEEGHandler.EEGStringCallBack callBackFunc)
	\brief The assigned delegate will be called when the raw Environment data is ready. 
		   The input string will have the following format:
		   environmentString = std::string("SMOOTHED_GAMMA,") +
                        ToString(sbInstance.EEG_smoothedGamma[0]) + "," +
                        ToString(sbInstance.EEG_smoothedGamma[1]) + "," +
                        ToString(sbInstance.EEG_smoothedGamma[2]) + "," +
                        ToString(sbInstance.EEG_smoothedGamma[3]) + "," +
                        std::string("SMOOTHED_MEAN,") +
                        ToString(sbInstance.EEG_smoothedMean[0]) + "," +
                        ToString(sbInstance.EEG_smoothedMean[1]) + "," +
                        ToString(sbInstance.EEG_smoothedMean[2]) + "," +
                        ToString(sbInstance.EEG_smoothedMean[3]);
	\param callBackFunc
	The callBack function to execute
	*/
	/******************************************************************************/
	public void assignEnvironmentDataDelegate(EEGController.EEGStringCallBack callBackFunc)
	{
		EEGController.getInstance().assignEnvironmentDataDelegate(callBackFunc);
	}


	/******************************************************************************/
	/*!
		\fn  void assignAttentionDelegate(MyEEGHandler.btFloatCallback callBackFunc)
	\brief The assigned delegate will be called when the attention signal is ready.
	\param callBackFunc
	The callBack function to execute
	*/
	/******************************************************************************/
	public void assignAttentionDelegate(EEGController.btFloatCallback callBackFunc)
	{
		EEGController.getInstance().assignAttentionDelegate(callBackFunc);
	}

	/******************************************************************************/
	/*!
		\fn  void assignRelaxationDelegate(MyEEGHandler.btFloatCallback callBackFunc)
	\brief The assigned delegate will be called when the relaxation signal is ready. This is an array of 4 floats
	\param callBackFunc
	The callBack function to execute
	*/
	/******************************************************************************/
	public void assignRelaxationDelegate(EEGController.btFloatCallback callBackFunc)
	{
		EEGController.getInstance().assignRelaxationDelegate(callBackFunc);
	}

	/******************************************************************************/
	/*!
		\fn  void assignMentalWorkloadDelegate(MyEEGHandler.btFloatCallback callBackFunc)
	\brief The assigned delegate will be called when the attention signal is ready. This is an array of 4 floats
	\param callBackFunc
	The callBack function to execute
	*/
	/******************************************************************************/
	public void assignMentalWorkloadDelegate(EEGController.btFloatCallback callBackFunc)
	{
		EEGController.getInstance().assignMentalWorkloadDelegate(callBackFunc);
	}


	/******************************************************************************/
	/*!
	\fn  void assignABDTDelegate(MyEEGHandler.btFloatArrayCallback callBackFunc)
	\brief The assigned delegate will be called when the ABDT(Alpha, Beta, Delta..) signal is ready. This is a 2D array of 4x5 floats
	\param callBackFunc
	The callBack function to execute
	*/
	/******************************************************************************/
	public void assignABDTDelegate(EEGController.btFloat2DArrayCallback callBackFunc)
	{
		EEGController.getInstance().assignABDTDelegate(callBackFunc);
	}

	/******************************************************************************/
	/*!
	\fn  void assignAccDelegate(MyEEGHandler.btFloatArrayCallback callBackFunc)
	\brief The assigned delegate will be called when accelerometer updates. This is an array of 3 floats
	\param callBackFunc
	The callBack function to execute
	*/
	/******************************************************************************/
	public void assignAccDelegate(EEGController.btFloatArrayCallback callBackFunc)
	{
		EEGController.getInstance().assignACCDelegate(callBackFunc);
	}

	/******************************************************************************/
	/*!
	\fn void assignBatteryStatus(BLEConnectedCallBack batteryStatusCallBack)
	\brief  Gets the battery status of the NEEURO device. The callBack will be called as frequent as the EEG data.
	\param batteryStatusCallBack
	CallBack to return the battery status once its ready.
	*/
	/******************************************************************************/
	public void assignBatteryStatus(EEGController.EEGStringCallBack batteryStatusCallBack)
	{
		EEGController.getInstance().assignBatteryUpdateCallBack(batteryStatusCallBack);
	}

	/******************************************************************************/
	/*!
	\fn  void unloadDelegates()
	\brief Call this when you are going to change scenes or when the delegates you previously assigned will be null
	*/
	/******************************************************************************/
	public void unloadDelegates()
	{
		EEGController.getInstance().assignRawDataDelegate(null);
		EEGController.getInstance().assignEnvironmentDataDelegate(null);
		EEGController.getInstance().assignAttentionDelegate(null);
		EEGController.getInstance().assignRelaxationDelegate(null);
		EEGController.getInstance().assignMentalWorkloadDelegate(null);
		EEGController.getInstance().assignABDTDelegate(null);
		EEGController.getInstance().assignACCDelegate(null);
		EEGController.getInstance().assignChannelStatusDelegate(null);
		EEGController.getInstance().assignGoodConnectionCheckDelegate(null);
		EEGController.getInstance().assignMCUIDCallBack(null);
		EEGController.getInstance().assignSignalReadyStatusDelegate(null);
	}


	/******************************************************************************/
	/*!
    \fn  void GetEnvironmentData()
    \Returns a string containing

    */
	/******************************************************************************/
	public string GetEnvironmentData()
	{
		return EEGController.getInstance().GetEnvironmentData(0);
	}

	#endregion
#endif
}