using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ChartAndGraph;

namespace MedBots
{
    public class EntropyManager : MonoBehaviour
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
            nsbEeg.assignAttentionDelegate((val) => {
                barChart.DataSource.SetValue("Attention", it.ToString(), val);
            });

            nsbEeg.assignMentalWorkloadDelegate((val) => {
                barChart.DataSource.SetValue("Mental Workload", it.ToString(), val);
            });

            nsbEeg.assignRelaxationDelegate((val) => {
                barChart.DataSource.SetValue("Relaxation", it.ToString(), val);
            });
        }

        void InitGraph()
        {
            barChart.GenerateRealtime();

            int i = 0;
            for (; i < medDevices.Length; i++)
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
                    barChart.DataSource.SetValue(device, it.ToString(), medReader.GetNumBits(device));
                }
                it++;
                camera.transform.position += new Vector3(cameraMoveSpeed * Time.deltaTime, 0, cameraMoveSpeed * Time.deltaTime);
                barChart.Invalidate();
                yield return null;
            }
        }    
    }
}