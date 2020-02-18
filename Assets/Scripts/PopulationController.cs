using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PopulationController : MonoBehaviour
{
    [SerializeField] private GameObject carPrefab;
    private List<GameObject> cars = new List<GameObject>();
    private int genCount = 1;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private int populationSize = 20;
    private Settings settings;
    [SerializeField] private Text speedOMeter;
    [SerializeField] private Image steeringWheel;
    private float timeElapsed = 0f;
    [SerializeField] private GameObject[] waypoints;

    //private TrainingLogArray trainingLog;
    private List<TrainingLog> trainingLog;

    public void RenderRays()
    {
        for (int i = 0; i < populationSize; i++)
        {
            if (!cars[i].GetComponent<CarController>().IsFinished())
            {
                cars[i].GetComponent<CarController>().DrawRays();

                i = populationSize;
            }
        }
    }

    private bool AllFinished()
    {
        foreach (GameObject item in cars)
        {
            if (!item.GetComponent<CarController>().IsFinished()) return false;
        }

        return true;
    }

    private void Awake()
    {
        //BetterColor red = new BetterColor(1f, 0f, 0f);
        //BetterColor green = new BetterColor(0f, 1f, 0f);
        //BetterColor blue = new BetterColor(0f, 0f, 1f);

        trainingLog = new List<TrainingLog>();

        List<BetterColor> fours = new List<BetterColor>();

        for (int i = 0; i < 4; i++)
        {
            fours.Add(new BetterColor(1f, 0f, 0f));

            fours[i].H = i / 4f;
            fours[i].HSVtoRGB();
            fours[i].ModifyColor();
        }

        List<BetterColor> threes = new List<BetterColor>();

        for (int i = 0; i < 3; i++)
        {
            threes.Add(new BetterColor(1f, 0f, 0f));

            threes[i].H = i / 3f;
            threes[i].HSVtoRGB();
            threes[i].ModifyColor();
        }

        //DebugGUI.SetGraphProperties("meanFitness", "Mean Fitness", 0, 2000, 0, blue.GetColor(), true);

        DebugGUI.SetGraphProperties("topFitness", "Top Fitness", 0, 200, 0, fours[0].GetColor(), false);
        DebugGUI.SetGraphProperties("upperQuartile", "Upper Quartile", 0, 200, 0, fours[1].GetColor(), true);
        DebugGUI.SetGraphProperties("medianFitness", "Median Fitness", 0, 200, 0, fours[2].GetColor(), true);
        DebugGUI.SetGraphProperties("lowerQuartile", "Lower Quartile", 0, 200, 0, fours[3].GetColor(), true);

        DebugGUI.SetGraphProperties("bestCarSpeed", "Speed", 0, 0, 1, fours[0].GetColor(), true);
        DebugGUI.SetGraphProperties("throttle", "Throttle", 0, 1, 1, threes[1].GetColor(), false);
        DebugGUI.SetGraphProperties("brake", "Brake", 0, 1, 1, threes[2].GetColor(), false);

        DebugGUI.SetGraphProperties("steering", "Steering", -1, 1, 2, threes[0].GetColor(), true);

        ///

        settings = new Settings();
    }

    private void OnDestroy()
    {
        DebugGUI.RemoveGraph("topFitness");
    }

    // Start is called before the first frame update
    private void Start()
    {
        // check if settings.json exist
        if (File.Exists(Application.dataPath + "/settings.xml"))
        {
            //string json = File.ReadAllText(Application.dataPath + "/settings.json");

            //settings = JsonUtility.FromJson<Settings>(json);

            XMLManager.LoadData(out settings, Application.dataPath + "/settings.xml");

            Debug.Log("settings file loaded");
        }
        else
        {
            settings.loadNetworkAtStart = false;
            settings.saveBestNetwork = true;

            //string json = JsonUtility.ToJson(settings);
            //File.WriteAllText(Application.dataPath + "/settings.json", json);

            XMLManager.SaveData(settings, Application.dataPath + "/settings.xml");

            Debug.Log("settings file created");
        }

        if (populationSize % 2 != 0)
        {
            populationSize++;
        }

        timeElapsed = 0f;
        for (int i = 0; i < populationSize; i++)
        {
            cars.Add(Instantiate(carPrefab, transform.position, transform.rotation, transform));

            float value = (float)i / (float)populationSize;
            //Vector3 color;

            float H = value;

            //cars[i].GetComponent<CarController>().SetLayers(layers);
            //cars[i].GetComponent<CarController>().SetGoal(goal);
            cars[i].GetComponent<CarController>().SetStart(transform);
            cars[i].GetComponent<CarController>().SetWaypoints(waypoints);

            cars[i].GetComponent<CarController>().InitNetwork();

            if (settings.loadNetworkAtStart)
            {
                if (i != 0)
                {
                    cars[i].GetComponent<CarController>().Mutate(cars[0]);
                }
                else
                {
                    if (File.Exists(Application.dataPath + "/network.xml"))
                    {
                        cars[i].GetComponent<CarController>().LoadData(Application.dataPath + "/network.xml");
                    }
                }
            }

            cars[i].GetComponent<CarController>().SetHue(H);
        }

        mainCamera.GetComponent<CameraController>().SetCamerTarget(cars[0]);

        //currentSpeeds = new List<float>();
    }

    // Update is called once per frame
    private void Update()
    {
        DebugGUI.LogPersistent("fpsCounter", "FPS: " + (1 / Time.smoothDeltaTime).ToString("F2"));

        DebugGUI.LogPersistent("genCount", "Generation: " + genCount.ToString());

        int activeCars = 0;
        foreach (GameObject item in cars)
        {
            if (!item.GetComponent<CarController>().IsFinished()) activeCars++;
        }
        DebugGUI.LogPersistent("activeCars", "Active Cars: " + activeCars.ToString());

        for (int i = 0; i < populationSize; i++)
        {
            if (!cars[i].GetComponent<CarController>().IsFinished())
            {
                if (cars[i].GetComponent<CarController>().output.Length != 0)
                {
                    float speed = cars[i].GetComponent<CarController>().currentSpeed;

                    DebugGUI.Graph("bestCarSpeed", speed);

                    speedOMeter.text = speed.ToString("F0") + " MPH";

                    float throttleBrake = cars[i].GetComponent<CarController>().output[1];

                    //DebugGUI.Graph("throttle", Mathf.Clamp(cars[i].GetComponent<CarController>().output[1], 0f, 1f));
                    //DebugGUI.Graph("brake", Mathf.Clamp(cars[i].GetComponent<CarController>().output[2], 0f, 1f));

                    float engine = cars[i].GetComponent<CarController>().output[1];
                    if (engine > 0f)
                    {
                        DebugGUI.Graph("throttle", engine);
                        DebugGUI.Graph("brake", 0f);
                    }
                    else
                    {
                        DebugGUI.Graph("throttle", 0f);
                        DebugGUI.Graph("brake", Mathf.Abs(engine));
                    }

                    //DebugGUI.Graph("throttle", cars[i].GetComponent<CarController>().output[1]);
                    DebugGUI.Graph("steering", cars[i].GetComponent<CarController>().output[0]);

                    float angle = cars[i].GetComponent<CarController>().output[0] * 90f;

                    steeringWheel.transform.rotation = Quaternion.Euler(0f, 0f, -1f * angle);

                    //DebugGUI.Graph("currentRPM", cars[i].GetComponent<CarController>().GetRPM() / 1000f);

                    i = populationSize;
                }
            }
        }

        if (AllFinished())
        {
            timeElapsed = 0f;

            mainCamera.GetComponent<CameraController>().ResetCamera();

            // End of generation training

            cars = cars.OrderByDescending(e => e.GetComponent<CarController>().GetFitness()).ToList();
            //cars = cars.OrderByDescending(e => e.GetComponent<CarController>().GradientSort()).ToList();

            // Graphing
            DebugGUI.Graph("topFitness", cars[0].GetComponent<CarController>().GetFitness());

            // calculate median

            float limit = (populationSize / 2f) - 1f;

            float medianIndex = (2 / 3f) * limit;

            float median;

            if (medianIndex % 1 != 0)
            {
                int temp1 = Mathf.FloorToInt(medianIndex);
                int temp2 = Mathf.CeilToInt(medianIndex);

                median = (cars[temp1].GetComponent<CarController>().GetFitness() + cars[temp2].GetComponent<CarController>().GetFitness()) / 2f;
            }
            else
            {
                median = cars[(int)medianIndex].GetComponent<CarController>().GetFitness();
            }

            TrainingLog data = new TrainingLog
            {
                generation = genCount,
                topFitness = cars[0].GetComponent<CarController>().GetFitness(),
                medianFitness = median
            };

            trainingLog.Add(data);

            JSONManager.SaveData(trainingLog.ToArray(), Application.dataPath + "/trainingLog.json");

            DebugGUI.Graph("medianFitness", median);

            // calculate quartiles
            float Q3Index = (1 / 3f) * limit; // 0.25 is used because it's sorted in descending order
            float Q3;
            if (Q3Index % 1 != 0)
            {
                int temp1 = Mathf.FloorToInt(Q3Index);
                int temp2 = Mathf.CeilToInt(Q3Index);

                Q3 = (cars[temp1].GetComponent<CarController>().GetFitness() + cars[temp2].GetComponent<CarController>().GetFitness()) / 2f;
            }
            else
            {
                Q3 = cars[(int)Q3Index].GetComponent<CarController>().GetFitness();
            }

            DebugGUI.Graph("upperQuartile", Q3);

            float Q1Index = limit;
            float Q1;
            if (Q1Index % 1 != 0)
            {
                int temp1 = Mathf.FloorToInt(Q1Index);
                int temp2 = Mathf.CeilToInt(Q1Index);

                Q1 = (cars[temp1].GetComponent<CarController>().GetFitness() + cars[temp2].GetComponent<CarController>().GetFitness()) / 2f;
            }
            else
            {
                Q1 = cars[(int)Q1Index].GetComponent<CarController>().GetFitness();
            }
            DebugGUI.Graph("lowerQuartile", Q1);

            float IQR = Q3 - Q1;

            List<float> speedList = new List<float>();

            foreach (GameObject item in cars)
            {
                speedList.Add(item.GetComponent<CarController>().avgSpeed);
            }
            speedList = speedList.OrderByDescending(e => e).ToList();

            DebugGUI.Graph("medianSpeed", speedList[(populationSize / 2) - 1]);
            DebugGUI.Graph("topSpeed", speedList[0]);

            for (int i = 0; i < populationSize / 2; i++)
            {
                // check for outlier
                float outlier = Q3 + (1.5f * IQR);

                if (cars[0].GetComponent<CarController>().GetFitness() > outlier)
                {
                    cars[(populationSize / 2) + i].GetComponent<CarController>().Mutate(cars[0]);
                }
                else
                {
                    // take best half and delete worst half

                    if (i + 1 == populationSize / 2)
                    {
                        cars[(populationSize / 2) + i].GetComponent<CarController>().Reproduce(cars[i], cars[0]);
                    }
                    else
                    {
                        cars[(populationSize / 2) + i].GetComponent<CarController>().Reproduce(cars[i], cars[i + 1]);
                    }
                }

                //cars[(populationSize / 2) + i].GetComponent<CarController>().ResetCar();
                //cars[i].GetComponent<CarController>().ResetCar();
            }

            // reset all cars

            foreach (GameObject item in cars)
            {
                item.GetComponent<CarController>().ResetCar();
            }

            // saving best network to file

            if (settings.saveBestNetwork)
            {
                cars[0].GetComponent<CarController>().SaveData();
            }

            genCount++;
        }
        else
        {
            for (int i = 0; i < populationSize; i++)
            {
                if (!cars[i].GetComponent<CarController>().IsFinished())
                {
                    mainCamera.GetComponent<CameraController>().SetCamerTarget(cars[i]);

                    i = populationSize;
                }
            }

            timeElapsed += Time.deltaTime;

            DebugGUI.LogPersistent("timeElapsed", "Time Elapsed: " + timeElapsed.ToString("F3"));
        }
    }
}