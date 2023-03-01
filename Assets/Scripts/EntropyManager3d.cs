using System.Diagnostics;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ChartAndGraph;

namespace MedBots
{
    public class EntropyManager3d : MonoBehaviour
    {
        public WorldSpaceBarChart barChart;
        public MedReader medReader;
 
        public NSB_EEG nsbEeg;

        public string[] medDevices;

        public GameObject camera;

        public float cameraMoveSpeed;

        public Material[] materials;

        int it = 1;

        // Start is called before the first frame update
        void Start()
        {
            InitMeds();
            InitNeeuro();
            InitGraph();

            StartCoroutine(ReadMed());
        }

        // Update is called once per frame
        void Update()
        {
        }

        void InitMeds()
        {
            medReader.Init();
            medDevices = medReader.GetDevices();
        }

        void InitNeeuro()
        {
    #if !UNITY_EDITOR_OSX && !UNITY_STANDALONE_OSX
            nsbEeg.assignAttentionDelegate((val) => {
                UnityEngine.Debug.Log("Attention " + val);
                it++;
                barChart.DataSource.SetValue("Attention", it.ToString(), val*1000);
                barChart.Invalidate();
            });

            nsbEeg.assignMentalWorkloadDelegate((val) => {
                UnityEngine.Debug.Log("Mental " + val);
                barChart.DataSource.SetValue("Mental Workload", it.ToString(), val*1000);
            });

            nsbEeg.assignRelaxationDelegate((val) => {
                UnityEngine.Debug.Log("AtRelaxationtention " + val);
                barChart.DataSource.SetValue("Relaxation", it.ToString(), val*100);
            });
    #endif
        }

        void InitGraph()
        {
            barChart.GenerateRealtime();

            int i = medDevices?.Length > 0 ? 0 : -1;
            for (; i < medDevices?.Length; i++)
            {
                barChart.DataSource.AddCategory(medDevices[i], materials[i]);
            }

            barChart.DataSource.AddCategory("Attention", materials[++i]);
            barChart.DataSource.AddCategory("Mental Workload", materials[++i]);
            barChart.DataSource.AddCategory("Relaxation", materials[++i]);
        }

        IEnumerator ReadMed()
        {
            while (true)
            {
                barChart.DataSource.AddGroup(it.ToString());
                foreach (string device in medDevices) {
                    var numBits = medReader.GetNumBits(device);
                    UnityEngine.Debug.Log("numBits: " + numBits);
                    barChart.DataSource.SetValue(device, it.ToString(), numBits);
                }
                it++;
                camera.transform.position += new Vector3(cameraMoveSpeed * Time.deltaTime, 0, cameraMoveSpeed * Time.deltaTime);
                barChart.Invalidate();
                yield return null;
            }
        }    
    }
}