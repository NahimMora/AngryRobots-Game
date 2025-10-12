using UnityEngine;

public class LevelManagerScript : MonoBehaviour
{
    [SerializeField] private Transform ProyectileSpawnPoint;
    [SerializeField] private Transform[] robotSpawnPoints;
    [SerializeField] private Transform CameraSpawnPoint;

    [SerializeField] private GameObject[] proyectilePrefabs;
    [SerializeField] private GameObject robotPrefab;
    private int currentProyectileIndex = 0;
    private GameObject currentProyectile;

    void Start()
    {
        CameraMovement cam = Camera.main.GetComponent<CameraMovement>();
        if (cam != null && CameraSpawnPoint != null)
        {
            cam.SetStartPosition(CameraSpawnPoint.position);
        }

        SpawnProyectile();
        SpawnRobots();
    }

    void OnEnable()
    {
        ProyectileScript.OnProyectileFinished += HandleProyectileFinished;
    }

    void OnDisable()
    {
        ProyectileScript.OnProyectileFinished -= HandleProyectileFinished;
    }

    void HandleProyectileFinished()
    {
        CameraMovement cam = Camera.main.GetComponent<CameraMovement>();
        if (cam != null)
        {
            cam.SetTarget(null);
            cam.ResetPositionCamera();
            Debug.Log("Posicion: " + CameraSpawnPoint.position);

        }
        NextProyectile();
    }

    void SpawnProyectile()
    {
        if (currentProyectileIndex < proyectilePrefabs.Length)
        {
            currentProyectile = Instantiate(proyectilePrefabs[currentProyectileIndex], ProyectileSpawnPoint.position, Quaternion.identity);

            CameraMovement cam = Camera.main.GetComponent<CameraMovement>();
            if (cam != null)
            {
                cam.SetTarget(currentProyectile.transform);
            }
        }
    }

    void SpawnRobots()
    {
        foreach (Transform spawnPoint in robotSpawnPoints)
        {
            GameObject robot = Instantiate(robotPrefab, spawnPoint.position, Quaternion.identity);
            robot.transform.localScale = new Vector3(4f, 4f, 4f);
        }
    }

    public void NextProyectile()
    {
        currentProyectileIndex++;
        if (currentProyectileIndex < proyectilePrefabs.Length)
        {
            SpawnProyectile();
        }
        else
        {
            Debug.Log("No quedan proyectiles. Fin del nivel.");
            CameraMovement cam = Camera.main.GetComponent<CameraMovement>();
            if (cam != null)
            {
                cam.ResetPositionCamera(); // ✅ Usa este método
            }
        }
    }

}
