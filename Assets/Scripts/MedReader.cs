using System;
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
    private static extern void MF_GetBytes(int length, IntPtr buffer, string generatorSerialNumber, StringBuilder pErrorReason);

    [DllImport("meterfeeder", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr MF_GetByte(string generatorSerialNumber, StringBuilder pErrorReason);

    [DllImport("meterfeeder", CallingConvention = CallingConvention.Cdecl)]
    private static extern int MF_Shutdown();

    static bool medInited = false;

    // Use this for initialization
    void Start()
    {
        int medRes = -1;
        sMFErrorReason.Clear();
        medRes = MF_Initialize(sMFErrorReason);
        Debug.Log($"MeterFeeder MF_Initialize: result:{medRes}, errorReason:{sMFErrorReason}");
        if (medRes != 0)
        {
            StartCoroutine(ReadMed());

        }
    }

    IEnumerator ReadMed()
    {
        while (true)
        {
            foreach (string medDevice in medDevices) {
                IntPtr bufferPtr = Marshal.AllocCoTaskMem(len);
                MF_GetBytes(len, bufferPtr, medDevice, sMFErrorReason);
                byte[] buffer = new byte[len];
                Marshal.Copy(bufferPtr, buffer, 0, len);
                num0s = num1s = 0;
                for (int i = 0; i < len; i++)
                {
                    int sb = countSetBits(buffer[i]);
                    num1s += sb;
                    num0s += (8 - sb);
                }
                Debug.Log(medDevice + " " + num1s + " bits");
                yield return null;
            }
        }
    }

    int even = 0;
    static bool qrngOn = true;
    private float waitTime = 0.2f;
    private float timer = 0.0f;
    static string[] medDevices = {"QWR4E002", "QWR4E001", "QWR4E004"};
    int len = 256;
    int num1s = 0, num0s = 0;

    void ReadInputAndMove()
    {
        // move closer to destination
        // get the next direction from keyboard
        if (Input.GetKeyDown("space"))
        {
            qrngOn = !qrngOn;
            Debug.Log(qrngOn ? "Random walking" : "Press ↑←↓→ buttons to walk");
        }

        timer += Time.deltaTime;

        if (qrngOn)
        {

            bool bitOn = false;

            if (num1s > num0s)
            {
                bitOn = true;
                //Debug.Log($"1: num1s:{num1s}, num0s:{num0s}");
            }
            else if (num1s == num0s)
            {
                //Debug.LogError("SAME NUMBER OF BITS!!!");
                //Debug.Log($"2: num1s:{num1s}, num0s:{num0s}");
                return;
            }
            else
            {
                //Debug.Log($"3: num1s:{num1s}, num0s:{num0s}");
            }

            if (even % 2 != 0)
            {
                if (bitOn)
                {
                    // _nextDir = Vector2.up;
                }
                else
                {
                    // _nextDir = Vector2.down;
                }
            }
            else
            {
                if (bitOn)
                {
                    // _nextDir = Vector2.left;
                }
                else
                {
                    // _nextDir = Vector2.right;
                }
            }


            timer = timer - waitTime;
            even++;
        }
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