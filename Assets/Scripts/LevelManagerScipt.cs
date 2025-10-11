using UnityEngine;

public class LevelManagerScript : MonoBehaviour
{
    [SerializeField] private Transform ProyectileSpawnPoint;
    [SerializeField] private Transform[] robotSpawnPoints;
    [SerializeField] private GameObject[] proyectilePrefabs;
    [SerializeField] private GameObject robotPrefab;

    private int currentProyectileIndex = 0;
    private GameObject currentProyectile;

    void Start()
    {
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
        NextProyectile();
    }

    void SpawnProyectile()
    {
        if (currentProyectileIndex < proyectilePrefabs.Length)
        {
            currentProyectile = Instantiate(proyectilePrefabs[currentProyectileIndex], ProyectileSpawnPoint.position, Quaternion.identity);

            // Avisar a la cámara que siga este pájaro
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
        }
    }
}
