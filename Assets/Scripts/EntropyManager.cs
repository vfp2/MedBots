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

        // Start is called before the first frame update
        void Start()
        {
            InitMeds();
            InitNeeuro();
            InitGraph();
        }

        // Update is called once per frame
        int it = 0;
        void Update()
        {
            // if (medDevices != null)
            // {
            //     foreach (string device in medDevices) {
            //         Debug.Log("Updating " + device);
            //         barChart.DataSource.SetValue(device, it.ToString(), medReader.GetNumBits(device));
            //     }
            //     it++;
            // }
        }

        void InitMeds()
        {
            medReader.Init();
            medDevices = medReader.GetDevices();

            it++;
            foreach (string device in medDevices) {
                int ones = medReader.GetNumBits(device);
                Debug.Log("Updating " + device + " with " + ones + "/" + it);
                barChart.DataSource.SetValue(device, it.ToString(), medReader.GetNumBits(device));
            }

            // it++;
            // foreach (string device in medDevices) {
            //     int ones = medReader.GetNumBits(device);
            //     Debug.Log("Updating " + device + " with " + ones + "/" + it);
            //     barChart.DataSource.SetValue(device, it.ToString(), medReader.GetNumBits(device));
            // }

            // it++;
            // foreach (string device in medDevices) {
            //     int ones = medReader.GetNumBits(device);
            //     Debug.Log("Updating " + device + " with " + ones + "/" + it);
            //     barChart.DataSource.SetValue(device, it.ToString(), medReader.GetNumBits(device));
            // }
        }

        void InitNeeuro()
        {
        }

        void InitGraph()
        {
            
        }
    }
}