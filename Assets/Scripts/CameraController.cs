using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float distance = 10f;
    [SerializeField] private float height = 10f;
    [SerializeField] private GameObject populationSpawner;
    private Vector3 previousPos;
    private GameObject target;
    public void ResetCamera()
    {
        Vector3 difference = populationSpawner.transform.forward;

        difference.y = 0f;
        difference.Normalize();
        difference *= distance;

        Vector3 newPos = populationSpawner.transform.position - difference;
        newPos.y = height;

        transform.position = newPos;
        transform.LookAt(populationSpawner.transform);

        previousPos = transform.position;
    }

    public void SetCamerTarget(GameObject target)
    {
        this.target = target;
    }

    private void OnPostRender()
    {
        populationSpawner.GetComponent<PopulationController>().RenderRays();
    }

    // Start is called before the first frame update
    private void Start()
    {
        //ResetCamera();
    }

    // Update is called once per frame
    private void Update()
    {
        if (target != null)
        {
            Vector3 difference = target.transform.position - transform.position;
            //difference.y = 0f;
            //difference.Normalize();
            //difference.y = -1f;
            //difference.Normalize();
            //difference *= distance;

            difference.y = 0f;
            difference.Normalize();
            difference *= distance;

            Vector3 newPos = target.transform.position - difference;
            newPos.y = height;
            //newPos.y = cameraHeight;

            newPos = SplineInterpolation.Interpolate(previousPos, transform.position, newPos, SplineInterpolation.Extrapolate(transform.position, newPos), Time.deltaTime);

            previousPos = transform.position;

            transform.position = newPos;

            Vector3 lookTarget = target.GetComponent<CarController>().GetVelocity() + target.transform.position;

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookTarget - transform.position), Time.deltaTime);
        }
        else
        {
            ResetCamera();
        }
    }
}