using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject carPrefab;
    private List<GameObject> cars = null;
    [SerializeField] private int limit = 10;
    private string path;
    [SerializeField] private Button startButton;
    private float timeElapsed;
    [SerializeField] private GameObject[] waypoints;

    private void GoToStart()
    {
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }

    // Start is called before the first frame update
    private void Start()
    {
        startButton.onClick.AddListener(GoToStart);

        // check if file exist;
        path = Application.dataPath + "/pretrainedNetwork.xml";

        if (File.Exists(path))
        {
            cars = new List<GameObject>
            {
                Instantiate(carPrefab, transform.position, transform.rotation, transform)
            };

            cars[0].GetComponent<CarController>().InitNetwork();

            cars[0].GetComponent<CarController>().DisableWaypoints();
            cars[0].GetComponent<CarController>().LoadData(path);
            cars[0].GetComponent<CarController>().SetHue((float)(cars.Count - 1) / (float)limit);
            cars[0].GetComponent<CarController>().SetStart(transform);
            cars[0].GetComponent<CarController>().SetWaypoints(waypoints);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (cars != null)
        {
            foreach (GameObject item in cars)
            {
                if (item.GetComponent<CarController>().IsFinished())
                {
                    item.GetComponent<CarController>().ResetCar();
                }
            }
        }

        timeElapsed += Time.deltaTime;

        if (timeElapsed >= 5f && cars.Count < limit)
        {
            timeElapsed = 0f;

            cars.Add(Instantiate(carPrefab, transform.position, transform.rotation, transform));

            cars[cars.Count - 1].GetComponent<CarController>().InitNetwork();

            cars[cars.Count - 1].GetComponent<CarController>().DisableWaypoints();
            cars[cars.Count - 1].GetComponent<CarController>().LoadData(path);
            cars[cars.Count - 1].GetComponent<CarController>().SetHue((float)(cars.Count - 1) / (float)limit);
            cars[cars.Count - 1].GetComponent<CarController>().SetStart(transform);
            cars[cars.Count - 1].GetComponent<CarController>().SetWaypoints(waypoints);
        }
    }
}