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
        Dictionary<string, float>  cumZScores = new Dictionary<string, float> ();
        Dictionary<string, List<float>> onesCountReads = new Dictionary<string, List<float>> ();

        float it = 1;
        int cumulativeOnesCount = 0;
        int cumulativeZeroesCount = 0;

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
            // USB MEDs
            medReader.Init();
            medDevices = medReader.GetDevices();

            // MED Farm
            // medDevices = new string[] {
            //     "QWR4E002",
            //     "QWR4E004",
            //     "QWR4B008",
            //     "QWR4A013",
            //     "QWR4A003",
            //     "PRNG"
            // };
        }

        void InitNeeuro()
        {
#if !UNITY_EDITOR_OSX && !UNITY_STANDALONE_OSX     
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
                cumZScores[medDevices[i]] = 0;
                onesCountReads[medDevices[i]] = new List<float>();
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
            cumZScores.Clear();
            onesCountReads.Clear();
            InitGraph();
        }

        IEnumerator ReadMed()
        {
            int n = 0;
            
            while (true)
            {
                if (!Pause)
                {   
                    // lineChart.DataSource.AddGroup(it.ToString());
                    for (int i = 0; i < medDevices.Length; i++)
                    {
                        int onesCount = 0;

                        // USB MEDS
                        onesCount = medReader.GetNumBits(medDevices[i]);

                        // MED Farm
                        // if (medDevices[i] == "PRNG")
                        // {
                        //     // PRNG
                        //     byte[] buffer = new byte[4];
                        //     System.Random prng = new System.Random();
                        //     prng.NextBytes(buffer);
                        //     for (int j = 0; j < 4; j++)
                        //     {
                        //         int sb = MedReader.countSetBits(buffer[j]);
                        //         onesCount += sb;
                        //     }
                        // }
                        // else
                        // {
                        //     // MED Farm test
                        //     UnityWebRequest uwr = UnityWebRequest.Get("https://entronet.fp2.dev/api/randint32?deviceId=" + medDevices[i]);
                        //     yield return uwr.SendWebRequest();
                        //     int randint32 = int.Parse(uwr.downloadHandler.text);
                        //     onesCount = MedReader.countSetBits(randint32);
                        // }

                        var zerosCount = (32 - onesCount);
                        if (onesCount > zerosCount)
                        {
                            randomWalks[medDevices[i]] += onesCount;
                        }
                        else if (zerosCount > onesCount)
                        {
                            randomWalks[medDevices[i]] -= zerosCount;
                        }

                        // Graph random walks
                        //lineChart.DataSource.AddPointToCategoryRealtime(medDevices[i], it, randomWalks[medDevices[i]]);

                        cumulativeOnesCount += onesCount;
                        cumulativeZeroesCount += zerosCount;

                        // z-score calculation:
                        // z = (x – μ) / σ
                        // x - data point
                        // μ - average
                        // σ - stddev
                        // n += (onesCount + zerosCount);
                        //int x = cumulativeOnesCount;
                        // float μ = (cumulativeOnesCount / n);
                        
                        // σ stddev calculation:
                        // σ = √(∑(x−μ)²/n)
                        // μ
                        onesCountReads[medDevices[i]].Add(onesCount);
                        float μ = 0;
                        for (n = 0; n < onesCountReads[medDevices[i]].Count; n++) {
                            μ += onesCountReads[medDevices[i]][n];
                        }
                        μ /= n;
                        // x
                        int x = onesCount;
                        // ∑(x−μ)²
                        float sumx_μ2 = 0;
                        for (int j = 0; j< onesCountReads[medDevices[i]].Count; j++) {
                            sumx_μ2 += Mathf.Pow(onesCountReads[medDevices[i]][j] /* x */ - μ, 2);
                        }
                        // σ = √(∑(x−μ)²/n)
                        float σ = Mathf.Sqrt(sumx_μ2/n);

                        // z-score calculation:
                        // z = (x – μ) / σ
                        float z = (x - μ) / σ;
                        if (σ == 0) {
                            cumZScores[medDevices[i]] = 0;
                        } else {
                            cumZScores[medDevices[i]] += z;
                        }

                        // Graph cummulative z-scores
                        lineChart.DataSource.AddPointToCategoryRealtime(medDevices[i], it, cumZScores[medDevices[i]]);
                    }

                    // MED Farm test
                    // UnityWebRequest uwr = UnityWebRequest.Get("https://entronet.fp2.dev/api/randint32?deviceId=QWR4E001");
                    // yield return uwr.SendWebRequest();
                    // int randint32 = int.Parse(uwr.downloadHandler.text);
                    // lineChart.DataSource.AddPointToCategoryRealtime("QWR4E004", it, MedReader.countSetBits(randint32));

                    it += 1;
                }

                // yield return new WaitForSeconds(.1f);
                yield return null;
            }
        }    
    }
}