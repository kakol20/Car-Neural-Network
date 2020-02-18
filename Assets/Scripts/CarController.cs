using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CarController : MonoBehaviour
{
    [HideInInspector] public float avgSpeed = 0f;
    [HideInInspector] public float currentSpeed = 0f;
    [HideInInspector] public float[] output;
    [HideInInspector] public float topSpeed = 0f;
    [SerializeField] private float brakeForce = 2000f;
    [SerializeField] private float checkForStopTimeLimit = 5f;
    private BetterColor color;
    private bool died = false;
    private float distanceTravelled = 0.0f;
    [SerializeField] [Range(0f, 1f)] private float drag = 0.15f;
    private bool finished = false;
    private float fitness = 0.0f;
    private bool followingWaypoints = true;
    [SerializeField] [Range(0.5f, 1f)] private float frontBrakeBias = 0.64f;
    [SerializeField] private WheelCollider FrontR, FrontL, RearR, RearL;
    private bool hitGoal = false;
    private float[] input;
    private Vector3 lastPosition;
    [SerializeField] private int[] layers = { 8, 6, 3 };
    [SerializeField] private bool legacyControls = true;
    [SerializeField] private Material lineMat;
    [SerializeField] private float maximumVelocity = 40f;
    [SerializeField] private float maxRayDistance = 10f;
    [SerializeField] private float minimumVelocity = 0.1f;
    private NeuralNetwork network;
    private int nextWaypoint = 0;
    [SerializeField] private int rayCount = 7;
    private List<float> speed;
    private Transform start;
    [SerializeField] private float steeringAngle = 75f;
    [SerializeField] private float throttleForce = 10000f;
    private float timeElapsed = 0.0f;
    [SerializeField] private float timeLimit = 15f;
    private float velocity;
    private GameObject[] waypoints;
    private int waypointsHit = 0;
    private float waypointTimeElapsed = 0f;
    public void DisableWaypoints()
    {
        followingWaypoints = false;
    }

    public void DrawRays()
    {
        for (int i = 0; i < rayCount; i++)
        {
            float distance = 0.0f;

            float angle = (180.0f / (float)(rayCount - 1f)) * (float)i * -1.0f;

            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 direction = Quaternion.Euler(0.0f, angle, 0.0f) * right;
            direction.Normalize();

            Ray ray = new Ray(transform.position, direction);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                distance = hit.distance;
            }
            //distance = Mathf.Clamp(distance, 0f, maxRayDistance);

            Debug.DrawRay(transform.position, direction * distance);

            Vector3 destination = transform.position + (direction * distance);

            //LineRenderer line = GetComponent<LineRenderer>();

            GL.PushMatrix();

            GL.Begin(GL.LINES);
            lineMat.SetPass(0);
            GL.Color(Color.green);
            GL.Vertex3(transform.position.x, transform.position.y, transform.position.z);
            GL.Vertex3(destination.x, destination.y, destination.z);
            GL.End();

            GL.PopMatrix();

            //LineRenderer line
        }
    }

    public BetterColor GetColor()
    {
        return new BetterColor(color);
    }

    public float GetFitness()
    {
        // First calculate fitness

        fitness = 50f;
        fitness += distanceTravelled / timeElapsed;
        fitness -= (waypoints[nextWaypoint].transform.position - transform.position).magnitude;
        //fitness += distanceTravelled;

        fitness += 100f * waypointsHit;

        if (hitGoal)
        {
            fitness += 50f;
        }

        if (died)
        {
            fitness -= 25f;
        }

        return fitness < 0f ? 0f : fitness / 10f;

        //return fitness;
    }

    public NeuralNetwork GetNetwork()
    {
        return network;
    }

    public float GetSteeringAngle()
    {
        return output[0] * steeringAngle;
    }

    public Vector3 GetVelocity()
    {
        return legacyControls ? transform.forward * velocity : GetComponent<Rigidbody>().velocity;
    }

    public float GradientSort()
    {
        float fitness = GetFitness();

        return fitness > 0f ? Random.Range(0f, fitness) : Random.Range(fitness, 0f);
    }

    public void InitNetwork()
    {
        network = new NeuralNetwork(layers);
    }

    public bool IsFinished()
    {
        return finished;
    }

    public void LoadData(string path)
    {
        //FileStream file = new FileStream(Application.dataPath + "/network.xml", FileMode.Open);
        //XmlSerializer xmls = new XmlSerializer(typeof(NetworkData));
        //NetworkData data = xmls.Deserialize(file) as NetworkData;

        XMLManager.LoadData(out NetworkData data, path);

        network.CopyBiases(data.biases);
        network.CopyWeights(data.weights);

        //file.Close();
    }

    public void Clone(GameObject parent, bool copyColor)
    {
        network = new NeuralNetwork(parent.GetComponent<CarController>().GetNetwork());

        if (copyColor)
        {
            color = parent.GetComponent<CarController>().GetColor();
        }
    }

    public void Mutate(GameObject parent)
    {
        network = new NeuralNetwork(parent.GetComponent<CarController>().GetNetwork());
        network.Mutate();

        color = parent.GetComponent<CarController>().GetColor();

        color.MutateColor();
        color.ModifyColor();
        color.V = 1f;
    }

    public void Reproduce(GameObject parent1, GameObject parent2)
    {
        network = new NeuralNetwork(parent1.GetComponent<CarController>().GetNetwork());
        network.Reproduce(parent2.GetComponent<CarController>().GetNetwork());

        color.MixColors(parent1.GetComponent<CarController>().GetColor(), parent2.GetComponent<CarController>().GetColor());

        color.MutateColor();
        color.ModifyColor();
        color.V = 1f;
    }

    public void ResetCar()
    {
        died = false;
        distanceTravelled = 0.0f;
        finished = false;
        hitGoal = false;
        nextWaypoint = 0;
        timeElapsed = 0.0f;
        transform.position = start.position;
        transform.rotation = start.rotation;
        waypointsHit = 0;
        waypointTimeElapsed = 0f;

        if (!legacyControls)
        {
            FrontL.motorTorque = 0f;
            FrontR.motorTorque = 0f;
            RearL.motorTorque = 0f;
            RearR.motorTorque = 0f;

            GetComponent<Rigidbody>().velocity *= 0f;
        }

        speed = new List<float>();

        lastPosition = transform.position;

        velocity = 0f;
    }

    public void SaveData()
    {
        NetworkData data = new NetworkData();

        List<float[][]> weights = new List<float[][]>();
        for (int x = 0; x < network.GetWeights().Length; x++)
        {
            List<float[]> weightsY = new List<float[]>();
            for (int y = 0; y < network.GetWeights()[x].Length; y++)
            {
                List<float> weightZ = new List<float>();
                for (int z = 0; z < network.GetWeights()[x][y].Length; z++)
                {
                    weightZ.Add(network.GetWeights()[x][y][z]);
                }

                weightsY.Add(weightZ.ToArray());
            }

            weights.Add(weightsY.ToArray());
        }
        data.weights = weights.ToArray();

        List<float[]> biases = new List<float[]>();
        for (int x = 0; x < network.GetBiases().Length; x++)
        {
            List<float> biasesY = new List<float>();
            for (int y = 0; y < network.GetBiases()[x].Length; y++)
            {
                biasesY.Add(network.GetBiases()[x][y]);
            }
            biases.Add(biasesY.ToArray());
        }
        data.biases = biases.ToArray();

        XMLManager.SaveData(data, Application.dataPath + "/network.xml");
    }

    //public void SetGoal(Transform goal)
    //{
    //    this.goal = goal;
    //}

    public void SetHue(float hue)
    {
        color.H = hue;
        color.ModifyColor();
    }

    public void SetLayers(int[] layers)
    {
        this.layers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }
    }

    public void SetStart(Transform start)
    {
        this.start = start;
    }

    public void SetWaypoints(GameObject[] copyWaypoints)
    {
        waypoints = new GameObject[copyWaypoints.Length];

        for (int i = 0; i < copyWaypoints.Length; i++)
        {
            waypoints[i] = copyWaypoints[i];
        }
    }

    private void Awake()
    {
        //color = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

        //renderer.material.set

        speed = new List<float>();

        color = new BetterColor();

        if (!File.Exists(Application.dataPath + "/carSettings.xml")) // check if file exists
        {
            CarSettings carSettings = new CarSettings
            {
                brakeForce = this.brakeForce,
                drag = this.drag,
                maxVelocity = this.maximumVelocity,
                steeringAngle = this.steeringAngle,
                throttleForce = this.throttleForce
            };

            //XmlSerializer xmls = new XmlSerializer(typeof(CarSettings));

            //FileStream file = new FileStream(Application.dataPath + "/carSettings.xml", FileMode.Create);

            //xmls.Serialize(file, carSettings);

            //file.Close();

            XMLManager.SaveData(carSettings, Application.dataPath + "/carSettings.xml");
        }
        else // load modded file
        {
            XMLManager.LoadData(out CarSettings carSettings, Application.dataPath + "/carSettings.xml");

            brakeForce = carSettings.brakeForce;
            drag = carSettings.drag;
            maximumVelocity = carSettings.maxVelocity;
            steeringAngle = carSettings.steeringAngle;
            throttleForce = carSettings.throttleForce;
        }

        if (!legacyControls)
        {
            GetComponent<Rigidbody>().centerOfMass = new Vector3(0f, -0.5f, -0.2f);
            GetComponent<Rigidbody>().drag = drag;
        }
    }

    private void CalculateInput()
    {
        List<float> tempInput = new List<float>();

        for (int i = 0; i < rayCount; i++)
        {
            float distance = 0.0f;

            float angle = (180.0f / (float)(rayCount - 1f)) * (float)i * -1.0f;

            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 direction = Quaternion.Euler(0.0f, angle, 0.0f) * right;

            Ray ray = new Ray(transform.position, direction);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                distance = hit.distance;
            }
            tempInput.Add(distance / maxRayDistance);
        }

        tempInput.Add(currentSpeed);

        //tempInput.Add((waypoints[nextWaypoint].transform.position - transform.position).magnitude);
        //tempInput.Add(GetRPM() / 1000f);

        input = tempInput.ToArray();
    }

    private void ControlsRealistic()
    {
        //if ()

        //float throttle = Mathf.Clamp(output[1], 0f, 1f) * throttleForce;
        //float brake = Mathf.Clamp(output[2], 0f, 1f) * brakeForce;

        float engine = output[1];
        float throttle, brake;

        if (engine > 0f)
        {
            brake = 0f;
            throttle = engine * throttleForce;
        }
        else
        {
            brake = Mathf.Abs(engine) * brakeForce;
            throttle = 0f;
        }

        float steering = output[0] * steeringAngle;

        RearL.motorTorque = throttle;
        RearR.motorTorque = throttle;

        FrontL.brakeTorque = (2f * brake) * frontBrakeBias;
        FrontR.brakeTorque = (2f * brake) * frontBrakeBias;
        RearL.brakeTorque = (2f * brake) * (1f - frontBrakeBias);
        RearR.brakeTorque = (2f * brake) * (1f - frontBrakeBias);

        FrontL.steerAngle = steering;
        FrontR.steerAngle = steering;

        currentSpeed = GetComponent<Rigidbody>().velocity.magnitude * 2.2369f;

        if (waypointTimeElapsed > checkForStopTimeLimit && followingWaypoints)
        {
            if (GetComponent<Rigidbody>().velocity.magnitude <= minimumVelocity) finished = true;
        }
    }

    private void ControlsSimple(float deltaTime)
    {
        float acceleration = 0f;

        // Calculate forward movement

        acceleration += Mathf.Clamp(output[1], 0f, 1f) * throttleForce;

        acceleration -= Mathf.Clamp(output[2], 0f, 1f) * brakeForce;

        acceleration -= drag * Time.fixedDeltaTime;

        velocity += acceleration;

        // Limit speed
        velocity = Mathf.Clamp(velocity, 0f, maximumVelocity);

        // Calculate steering

        if (velocity > 0f)
        {
            float rotate = output[0] * steeringAngle;

            transform.Rotate(0f, rotate * deltaTime, 0f);
        }

        float translate = velocity * deltaTime;

        transform.Translate(0f, 0f, translate);

        // Calculate speed in mph
        currentSpeed = velocity * 2.2369f;

        // Check for finish

        if (timeElapsed > checkForStopTimeLimit)
        {
            if (velocity < minimumVelocity) finished = true;
        }
    }

    private void CopyOutput(float[] output)
    {
        this.output = new float[output.Length];
        for (int i = 0; i < output.Length; i++)
        {
            this.output[i] = output[i];
        }
    }

    private void FixedUpdate()
    {
        // Control Car

        if (!finished)
        {
            timeElapsed += Time.fixedDeltaTime;
            waypointTimeElapsed += Time.fixedDeltaTime;

            distanceTravelled += (transform.position - lastPosition).magnitude;

            // Run Network

            CalculateInput();

            CopyOutput(network.FeedForward(input));

            if (legacyControls)
            {
                ControlsSimple(Time.fixedDeltaTime);
            }
            else
            {
                ControlsRealistic();
            }

            lastPosition = transform.position;

            //if ((goal.position - transform.position).magnitude <= distanceBuffer)
            //{
            //    finished = true;
            //    hitGoal = true;
            //}

            if (waypointTimeElapsed > timeLimit && followingWaypoints) finished = true;
        }
        else
        {
            if (!legacyControls)
            {
                GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f);
                RearL.motorTorque = 0f;
                RearR.motorTorque = 0f;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("Wall"))
        {
            finished = true;
            died = true;

            if (!legacyControls)
            {
                GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f);
                RearL.motorTorque = 0f;
                RearR.motorTorque = 0f;
            }

            //Debug.Log("Wall hit");
        }
        else if (collision.collider.gameObject.CompareTag("Goal"))
        {
            finished = true;
            hitGoal = true;

            if (!legacyControls)
            {
                GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f);
                RearL.motorTorque = 0f;
                RearR.motorTorque = 0f;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Waypoint"))
        {
            if (Equals(other.transform.position, waypoints[nextWaypoint].transform.position))
            {
                nextWaypoint++;

                nextWaypoint %= waypoints.Length;

                waypointsHit++;

                if (waypointsHit >= waypoints.Length)
                {
                    finished = true;
                    hitGoal = true;
                }

                waypointTimeElapsed = 0f;
            }
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        lastPosition = transform.position;
    }

    // Update is called once per frame
    private void Update()
    {
        if (finished)
        {
            if (speed.Count() > 0)
            {
                avgSpeed = 0f;

                speed = speed.OrderByDescending(e => e).ToList();

                topSpeed = speed[0];

                foreach (float item in speed)
                {
                    avgSpeed += item;
                }

                avgSpeed /= (float)speed.Count();

                speed = new List<float>();
            }
        }
        else
        {
            speed.Add(currentSpeed); // speed in mph
        }

        BetterColor value = new BetterColor(color.GetColor());
        //Color value = color.GetColor();

        if (died)
        {
            value.V = 0.25f;
        }

        if (hitGoal)
        {
            value.S = 0.25f;
        }

        if (finished)
        {
            value.A = 0.5f;
        }
        else
        {
            value.A = 1f;
        }

        value.HSVtoRGB();

        GetComponent<Renderer>().material.SetColor("_Color", value.GetColor());
    }
}