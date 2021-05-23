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

        public string[] medDevices;

        int it = 1;

        // Start is called before the first frame update
        void Start()
        {
            InitGraph();
            InitNeeuro();
            InitMeds();

            StartCoroutine(ReadMed());
        }

        // Update is called once per frame
        void Update()
        {
        }

        void InitGraph()
        {
            barChart.GenerateRealtime();
        }

        void InitNeeuro()
        {
        }

        void InitMeds()
        {
            medReader.Init();
            medDevices = medReader.GetDevices();
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
                barChart.Invalidate();
                yield return null;
            }
        }    
    }
}