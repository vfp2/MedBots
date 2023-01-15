using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using ChartAndGraph;

namespace MedBots
{
    public class EntropyManager : MonoBehaviour
    {
        public GraphChart lineChart;
        public MedReader medReader;

        public NSB_EEG nsbEeg;

        public string[] medDevices;

        public Material innerFillMaterial;
        public Material pointMaterial;
        public Material[] materials;

        public bool Pause 
        {
            get {
                return _pause;
            }
            set {
                if (value)
                    Debug.Log("Stopping");
                else
                    Debug.Log("Starting");
                _pause = value;
            }
        }
        private bool _pause;

        Dictionary<string, int> randomWalks = new Dictionary<string, int>();

        float it = 1;

        // Start is called before the first frame update
        void Start()
        {
            InitMeds();
            //InitNeeuro();
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
#if !UNITY_EDITOR_OSX            
            nsbEeg.assignAttentionDelegate((val) => {
                if (Pause) return;
                lineChart.DataSource.AddPointToCategoryRealtime("Attention", it, val);
            });

            nsbEeg.assignMentalWorkloadDelegate((val) => {
                if (Pause) return;
                lineChart.DataSource.AddPointToCategoryRealtime("Mental Workload", it, val);
            });

            nsbEeg.assignRelaxationDelegate((val) => {
                if (Pause) return;
                lineChart.DataSource.AddPointToCategoryRealtime("Relaxation", it, val);
            });
#endif
        }

        void InitGraph()
        {
            lineChart.DataSource.Clear();
            int i = 0;
            for (i = 0; i < medDevices.Length; i++)
            {
                lineChart.DataSource.AddCategory(medDevices[i], materials[i], 2.57999992370605, new ChartAndGraph.MaterialTiling(false, 0), innerFillMaterial, false, pointMaterial, 6.61999988555908, false);
                randomWalks[medDevices[i]] = 0;
            }

            // MED Farm test
            // lineChart.DataSource.AddCategory("QWR4E004", materials[i], 2.57999992370605, new ChartAndGraph.MaterialTiling(false, 0), innerFillMaterial, true, pointMaterial, 6.61999988555908, false);

            // lineChart.DataSource.AddCategory("Attention", materials[i++], 2.57999992370605, new ChartAndGraph.MaterialTiling(false, 0), innerFillMaterial, false, pointMaterial, 6.61999988555908, false);
            // lineChart.DataSource.AddCategory("Mental Workload", materials[i++], 2.57999992370605, new ChartAndGraph.MaterialTiling(false, 0), innerFillMaterial, false, pointMaterial, 6.61999988555908, false);
            // lineChart.DataSource.AddCategory("Relaxation", materials[i++], 2.57999992370605, new ChartAndGraph.MaterialTiling(false, 0), innerFillMaterial, false, pointMaterial, 6.61999988555908, false);
        }

        public void Reset()
        {
            Debug.Log("Resetting");
            it = 1;
            randomWalks.Clear();
            InitGraph();
        }

        IEnumerator ReadMed()
        {
            while (true)
            {
                if (!Pause)
                {   
                    // lineChart.DataSource.AddGroup(it.ToString());
                    for (int i = 0; i < medDevices.Length; i++)
                    {
                        var onesCount = medReader.GetNumBits(medDevices[i]);
                        var zerosCount = (32 - onesCount);
                        if (onesCount > zerosCount)
                        {
                            randomWalks[medDevices[i]] += onesCount;
                        }
                        else if (zerosCount > onesCount)
                        {
                            randomWalks[medDevices[i]] -= zerosCount;
                        }

                        lineChart.DataSource.AddPointToCategoryRealtime(medDevices[i], it, randomWalks[medDevices[i]]);
                    }

                    // MED Farm test
                    // UnityWebRequest uwr = UnityWebRequest.Get("https://entronet.fp2.dev/api/randint32?deviceId=QWR4E001");
                    // yield return uwr.SendWebRequest();
                    // int randint32 = int.Parse(uwr.downloadHandler.text);
                    // lineChart.DataSource.AddPointToCategoryRealtime("QWR4E004", it, MedReader.countSetBits(randint32));

                    it += 1;
                }

                yield return null;
            }
        }    
    }
}