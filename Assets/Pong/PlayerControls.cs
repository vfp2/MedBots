using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Text;

public class PlayerControls : MonoBehaviour {

	public KeyCode moveUp = KeyCode.W;
	public KeyCode moveDown = KeyCode.S;
    public KeyCode qrngToggle = KeyCode.Q;
	public float speed = 10.0f;
	public float boundY = 2.25f;
	private Rigidbody2D rb2d;

	// Use this for initialization
	void Start () {
		rb2d = GetComponent<Rigidbody2D> ();

		MedStart();
	}
	
	// Update is called once per frame
	void Update () {
		// var vel = rb2d.velocity;
		// if (Input.GetKey (moveUp)) {
		// 	vel.y = speed;
		// } else if (Input.GetKey (moveDown)) {
		// 	vel.y = -speed;
		// } else if (!Input.anyKey) {
		// 	vel.y = 0;
		// }
		// rb2d.velocity = vel;

        var vel = rb2d.velocity;
        vel = ReadInputAndMove(vel);
		rb2d.velocity = vel;

		var pos = transform.position;
		if (pos.y > boundY) {
			pos.y = boundY;
		} else if (pos.y < -boundY) {
			pos.y = -boundY;
		}
		transform.position = pos;
	}

	//---------------- MEDiness

	public string MedDevice;

	private static readonly int MF_ERROR_STR_MAX_LEN = 256;
    private static StringBuilder sMFErrorReason = new StringBuilder(MF_ERROR_STR_MAX_LEN);
    
    [DllImport("meterfeeder", CallingConvention = CallingConvention.Cdecl)]
    private static extern void MF_GetBytes(int length, IntPtr buffer, string generatorSerialNumber, StringBuilder pErrorReason);
    [DllImport("meterfeeder", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr MF_GetByte(string generatorSerialNumber, StringBuilder pErrorReason);
    [DllImport("meterfeeder", CallingConvention = CallingConvention.Cdecl)]
    private static extern int MF_GetNumberGenerators();
    [DllImport("meterfeeder", CallingConvention = CallingConvention.Cdecl)]
    private static extern void MF_GetListGenerators(StringBuilder[] devices);

    void MedStart()
    {
        MedDevice = GetDevices()[0];
        StartCoroutine(ReadMed());
    }

	public string[] GetDevices()
    {
        var devicesSB = new StringBuilder[MF_GetNumberGenerators()];
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

    IEnumerator ReadMed()
    {
        while (true)
        {
            IntPtr bufferPtr = Marshal.AllocCoTaskMem(len);
            MF_GetBytes(len, bufferPtr, MedDevice, sMFErrorReason);
            byte[] buffer = new byte[len];
            Marshal.Copy(bufferPtr, buffer, 0, len);
            num0s = num1s = 0;
            for (int i = 0; i < len; i++)
            {
                int sb = countSetBits(buffer[i]);
                num1s += sb;
                num0s += (8 - sb);
            }
            yield return null;
        }
    }

    public bool qrngOn = true;
    private float waitTime = 0.2f;
    private float timer = 0.0f;
    int len = 256;
    int num1s = 0, num0s = 0;

    Vector2 ReadInputAndMove(Vector2 vel)
    {
        // get the next direction from keyboard
        if (Input.GetKeyDown(qrngToggle))
        {
            qrngOn = !qrngOn;
            Debug.Log(qrngOn ? gameObject.name + ": Random walking" : "Press WS/↑↓ buttons to walk");
        }

        timer += Time.deltaTime;

        if (qrngOn)
        {
            if (num1s > num0s)
            {
                vel.y = -speed;
                Debug.Log($"1: num1s:{num1s}, num0s:{num0s}");
            }
            else if (num1s == num0s)
            {
                Debug.LogError("SAME NUMBER OF BITS!!!");
                Debug.Log($"2: num1s:{num1s}, num0s:{num0s}");
                vel.y = 0;
            }
            else
            {
                vel.y = speed;
                Debug.Log($"3: num1s:{num1s}, num0s:{num0s}");
            }

            timer = timer - waitTime;
        }
        else
        {
            // Normal up/down logic
            if (Input.GetKey (moveUp)) {
                vel.y = speed;
            } else if (Input.GetKey (moveDown)) {
                vel.y = -speed;
            } else if (!Input.anyKey) {
                vel.y = 0;
            }
        }

        return vel;
    }

    int countSetBits(int n)
    {
        int count = 0;
        while (n > 0)
        {
            count += n & 1;
            n >>= 1;
        }
        return count;
    }
}
