﻿using System;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

public class MedReader : MonoBehaviour
{
    private static readonly int MF_ERROR_STR_MAX_LEN = 256;
    private static StringBuilder sMFErrorReason = new StringBuilder(MF_ERROR_STR_MAX_LEN);

    [DllImport("meterfeeder", CallingConvention = CallingConvention.Cdecl)]
    private static extern int MF_Initialize(StringBuilder pErrorReason);

    [DllImport("meterfeeder", CallingConvention = CallingConvention.Cdecl)]
    private static extern void MF_Shutdown();

    [DllImport("meterfeeder", CallingConvention = CallingConvention.Cdecl)]
    private static extern int MF_GetNumberGenerators();

    [DllImport("meterfeeder", CallingConvention = CallingConvention.Cdecl)]
    private static extern void MF_GetListGenerators(StringBuilder[] devices);

    [DllImport("meterfeeder", CallingConvention = CallingConvention.Cdecl)]
    private static extern void MF_GetBytes(int length, IntPtr buffer, string generatorSerialNumber, StringBuilder pErrorReason);

    [DllImport("meterfeeder", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr MF_GetByte(string generatorSerialNumber, StringBuilder pErrorReason);

    static bool medInited = false;
    int len = 256;
    int num1s = 0, num0s = 0;

    public void Init()
    {
        int medRes = -1;
        sMFErrorReason.Clear();
        medRes = MF_Initialize(sMFErrorReason);
        Debug.Log($"MeterFeeder MF_Initialize: result:{medRes}, errorReason:{sMFErrorReason}");
        if (medRes != 0)
        {
            medInited = true;

        }
    }

    public int GetNumberDevices()
    {
        if (!medInited) return 0;

        return MF_GetNumberGenerators();
    }

    public string[] GetDevices()
    {
        if (!medInited) return null;

        var devicesSB = new StringBuilder[GetNumberDevices()];
        for (int i = 0; i < devicesSB.Length; i++)
        {
            devicesSB[i] = new StringBuilder(50);
        }

        MF_GetListGenerators(devicesSB);

        var devices = new string[devicesSB.Length];
        for (int i = 0; i < devicesSB.Length; i++)
        {
            // Get just the serial number (== 8 chars in length)
            devices[i] = devicesSB[i].ToString().Substring(0, 8);
        }

        return devices;
    }

    public int GetNumBits(string device)
    {
        IntPtr bufferPtr = Marshal.AllocCoTaskMem(len);
        MF_GetBytes(len, bufferPtr, device, sMFErrorReason);
        byte[] buffer = new byte[len];
        Marshal.Copy(bufferPtr, buffer, 0, len);
        num0s = num1s = 0;
        for (int i = 0; i < len; i++)
        {
            int sb = countSetBits(buffer[i]);
            num1s += sb;
            num0s += (8 - sb);
        }
        return num1s;
    }

    static int countSetBits(int n)
    {
        int count = 0;
        while (n > 0)
        {
            count += n & 1;
            n >>= 1;
        }
        return count;
    }

    void OnApplicationQuit()
    {
        Debug.Log("MeterFeeder MF_Shutdown");
        MF_Shutdown();
    }
}