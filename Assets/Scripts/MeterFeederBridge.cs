using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class MeterFeederBridge : MonoBehaviour
{
    [DllImport("meterfeeder")]
    private static extern int Initialize(out IntPtr err);

    // Start is called before the first frame update
    void Start()
    {
        IntPtr err;
        int res = Initialize(out err);
        var hello = Marshal.PtrToStringAnsi(err);
        Debug.Log($"res: {res}, {hello}");
    }
}
