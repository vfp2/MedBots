﻿using System;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

public class MedPlayerController : MonoBehaviour
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
    private static extern int MF_GetNumberGenerators();
    
    [DllImport("meterfeeder", CallingConvention = CallingConvention.Cdecl)]
    private static extern void MF_GetListGenerators(StringBuilder[] devices);

    [DllImport("meterfeeder", CallingConvention = CallingConvention.Cdecl)]
    private static extern int MF_Shutdown();

    static bool medInited = false;

    public float speed = 0.4f;
    Vector2 _dest = Vector2.zero;
    Vector2 _dir = Vector2.zero;
    Vector2 _nextDir = Vector2.zero;

    [Serializable]
    public class PointSprites
    {
        public GameObject[] pointSprites;
    }

    public PointSprites points;

    public static int killstreak = 0;

    // script handles
    private GameGUINavigation GUINav;
    private PacmanGameManager GM;
    private ScoreManager SM;

    private bool _deadPlaying = false;

    public Canvas GameOverCanvas;

    // Use this for initialization
    void Start()
    {
        GM = GameObject.Find("Game Manager").GetComponent<PacmanGameManager>();
        SM = GameObject.Find("Game Manager").GetComponent<ScoreManager>();
        GUINav = GameObject.Find("UI Manager").GetComponent<GameGUINavigation>();
        _dest = transform.position;

        int medRes = -1;
        sMFErrorReason.Clear();
        medRes = MF_Initialize(sMFErrorReason);
        Debug.Log($"MeterFeeder MF_Initialize: result:{medRes}, errorReason:{sMFErrorReason}");
        if (medRes != 0)
        {
            medDevice = GetDevices()[0];
            StartCoroutine(ReadMed());
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (PacmanGameManager.gameState)
        {
            case PacmanGameManager.GameState.Game:
                ReadInputAndMove();
                Animate();
                break;

            case PacmanGameManager.GameState.Dead:
                if (!_deadPlaying)
                    StartCoroutine("PlayDeadAnimation");
                break;
        }
    }

    IEnumerator PlayDeadAnimation()
    {
        _deadPlaying = true;
        GetComponent<Animator>().SetBool("Die", true);
        yield return new WaitForSeconds(1);
        GetComponent<Animator>().SetBool("Die", false);
        _deadPlaying = false;

        if (PacmanGameManager.lives <= 0)
        {
            H_ShowGameOverScreen();
        }

        else
            GM.ResetScene();
    }

    public void H_ShowGameOverScreen()
    {
        StartCoroutine("ShowGameOverScreen");
    }

    IEnumerator ShowGameOverScreen()
    {
        Debug.Log("Showing GAME OVER Screen");
        GameOverCanvas.enabled = true;
        yield return new WaitForSeconds(2);

        Application.LoadLevel("MedBots");
        Time.timeScale = 1.0f;

        // take care of game manager
        PacmanGameManager.DestroySelf();
    }

    void Animate()
    {
        Vector2 dir = _dest - (Vector2)transform.position;
        GetComponent<Animator>().SetFloat("DirX", dir.x);
        GetComponent<Animator>().SetFloat("DirY", dir.y);
    }

    bool Valid(Vector2 direction)
    {
        // cast line from 'next to pacman' to pacman
        // not from directly the center of next tile but just a little further from center of next tile
        Vector2 pos = transform.position;
        direction += new Vector2(direction.x * 0.45f, direction.y * 0.45f);
        RaycastHit2D hit = Physics2D.Linecast(pos + direction, pos);
        return hit.collider.name == "pacdot" || (hit.collider == GetComponent<Collider2D>());
    }

    public void ResetDestination()
    {
        _dest = new Vector2(15f, 11f);
        GetComponent<Animator>().SetFloat("DirX", 1);
        GetComponent<Animator>().SetFloat("DirY", 0);
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
            yield return null;
        }
    }

    int even = 0;
    static bool qrngOn = true;
    private float waitTime = 0.2f;
    private float timer = 0.0f;
    static string medDevice;
    int len = 256;
    int num1s = 0, num0s = 0;

    void ReadInputAndMove()
    {
        // move closer to destination
        Vector2 p = Vector2.MoveTowards(transform.position, _dest, speed);
        GetComponent<Rigidbody2D>().MovePosition(p);

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
                    _nextDir = Vector2.up;
                }
                else
                {
                    _nextDir = Vector2.down;
                }
            }
            else
            {
                if (bitOn)
                {
                    _nextDir = Vector2.left;
                }
                else
                {
                    _nextDir = Vector2.right;
                }
            }


            timer = timer - waitTime;
            even++;
        }
        else
        {
            if (Input.GetAxis("Horizontal") > 0) _nextDir = Vector2.right;
            if (Input.GetAxis("Horizontal") < 0) _nextDir = -Vector2.right;
            if (Input.GetAxis("Vertical") > 0) _nextDir = Vector2.up;
            if (Input.GetAxis("Vertical") < 0) _nextDir = -Vector2.up;
        }

        // if pacman is in the center of a tile
        if (Vector2.Distance(_dest, transform.position) < 0.00001f)
        {
            if (Valid(_nextDir))
            {
                _dest = (Vector2)transform.position + _nextDir;
                _dir = _nextDir;
            }
            else   // if next direction is not valid
            {
                //if (Valid(_dir))  // and the prev. direction is valid
                //    _dest = (Vector2)transform.position + _dir;   // continue on that direction

                // otherwise, do nothing
            }
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

    public Vector2 getDir()
    {
        return _dir;
    }

    public void UpdateScore()
    {
        killstreak++;

        // limit killstreak at 4
        if (killstreak > 4) killstreak = 4;

        Instantiate(points.pointSprites[killstreak - 1], transform.position, Quaternion.identity);
        PacmanGameManager.score += (int)Mathf.Pow(2, killstreak) * 100;

    }
}