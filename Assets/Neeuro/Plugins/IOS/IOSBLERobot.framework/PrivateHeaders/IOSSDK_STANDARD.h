//
//  IOSSDK.h
//  IOSBLESBFrame
//
//  Created by Serene Teng on 7/12/16.
//  Copyright © 2016 Serene Teng. All rights reserved.
//

#import <Foundation/Foundation.h>


typedef void(*DelegateSDK) (const char*);
typedef void(*voidDelegateBoolSDK) (bool);

@interface IOSSDK : NSObject

+ (void) Initialized;
+ (void) startScan;
+ (void) stopScan;
+ (void) startStopEEG:(bool)toStart;
+ (void) ClearBLEDeviceList;
+ (void) SendStartCommand:(const char*)deviceAddress :(int)type;
+ (void) SendStopCommand;
+ (void) DisconnectDevice:(const char*)deviceAddress :(int)type;
+ (void) ConnectDevice:(const char*)deviceAddress :(int)type;
+ (void) CancelConnection:(const char*)deviceAddress :(int)type;
+ (NSString*) grabMCUID:(const char*)deviceAddress;

//Calling function in NFL_SenzeBand.m
+ (void)settingStatus;
+ (void)nfl_developerCode: (NSString *)developerCode;
+ (void)nfl_Authenticate;


//Framework calling for Delegate
//Calling Delegate in CBController.m for UnitySendMessage
+ (void) BLE_SetDeviceConnectStatus:(DelegateSDK) callbackNFL;
+ (void) BLE_SetDeviceConnectFailStatus:(DelegateSDK) callbackNFL;
+ (void) BLE_SetDeviceNotConnectStatus:(DelegateSDK) callbackNFL;
+ (void) BLE_SetDeviceList:(DelegateSDK) callbackNFL;
+ (void) BLE_completedBTInit:(DelegateSDK) callbackNFL;
+ (void) BLE_SetUpdateRSSI:(DelegateSDK) callbackNFL;
+ (void) BLE_bluetoothSwitchStatus:(DelegateSDK) callbackNFL;


//Calling Delegate in NFL_SenzeBand.m for UnitySendMessage
+ (void) FNFL_EEG_GetMentalStateData:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetBatteryLevelUpdate:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetAccXYZ:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetChannelStatus:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetGoodConnectionCheck:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetSignalReadyStatus:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetFreqTypePower:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetData:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetAuthenticationStatus:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetAuthenticationResult:(voidDelegateBoolSDK) callbackNFL;

+ (NSString*) GetEnvironmentData:(int) index;
+ (bool) SetEnvironmentThreshold:(int) index :(NSString*) ret;

//End of Framework calling for Delegate


@end
