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

        public GameObject camera;

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
        }

        void InitGraph()
        {
            barChart.GenerateRealtime();

            for (int i = 0; i < medDevices.Length; i++)
            {
                barChart.DataSource.AddCategory(medDevices[i], materials[i]);
            }
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
                float mv = 4.2f;
                camera.transform.position += new Vector3(mv * Time.deltaTime, 0, mv * Time.deltaTime);
                barChart.Invalidate();
                yield return null;
            }
        }    
    }
}