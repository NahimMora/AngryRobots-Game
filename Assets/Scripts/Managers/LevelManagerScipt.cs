using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManagerScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject tankPrefab;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;

    [Header("Level Configuration")]
    [SerializeField] private LevelData currentLevelData;
    [SerializeField] private LevelData nextLevelData;

    private int totalRobots;
    private int robotsDestroyed = 0;
    private bool levelEnded = false;
    private TankLauncherScript tankLauncher;

    void Awake()
    {
        // Si venimos del LevelLoader, sobreescribimos
        if (LevelLoader.currentLevel != null)
            currentLevelData = LevelLoader.currentLevel;
    }

    void Start()
    {
        // No iniciar el nivel automáticamente
        // El menú se encargará de llamar a IniciarNivelConData()
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);

        Debug.Log("🕹️ Esperando selección de nivel desde el menú...");
    }


    public void IniciarNivelConData(LevelData data)
    {
        currentLevelData = data;
        Debug.Log($"▶️ Cargando {data.name}");

        // 🔧 PRIMERO: Posicionar la cámara
        SetupCamera();

        // LUEGO: Spawn del tanque y robots
        SpawnTank();
        SpawnRobots();
    }

    void OnEnable()
    {
        ProyectileScript.OnProyectileFinished += HandleProyectileFinished;
        RobotScript.OnRobotDestroyed += HandleRobotDestroyed;
    }

    void OnDisable()
    {
        ProyectileScript.OnProyectileFinished -= HandleProyectileFinished;
        RobotScript.OnRobotDestroyed -= HandleRobotDestroyed;
    }

    // --- SETUP CÁMARA ---
    void SetupCamera()
    {
        CameraMovement cam = Camera.main.GetComponent<CameraMovement>();
        if (cam != null && currentLevelData != null)
        {
            // Crear la posición con la Z correcta de la cámara
            Vector3 cameraPosition = new Vector3(
                currentLevelData.cameraStartPosition.x,
                currentLevelData.cameraStartPosition.y,
                Camera.main.transform.position.z  // Mantener Z de la cámara
            );

            // 🔧 CRÍTICO: Usar SetStartPosition para actualizar tanto transform como startPosition
            cam.SetStartPosition(cameraPosition);
            cam.SetTarget(null);  // Sin target al inicio

            Debug.Log($"📷 Cámara posicionada en: {cameraPosition}");
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró CameraMovement o LevelData");
        }
    }

    // --- EVENTOS ---
    void HandleProyectileFinished()
    {
        if (levelEnded) return;

        CameraMovement cam = Camera.main.GetComponent<CameraMovement>();
        if (cam != null)
        {
            cam.SetTarget(null);

            // Volver a la posición inicial del nivel, no a (0,0,0)
            if (currentLevelData != null)
            {
                Camera.main.transform.position = new Vector3(
                    currentLevelData.cameraStartPosition.x,
                    currentLevelData.cameraStartPosition.y,
                    Camera.main.transform.position.z
                );
            }
        }

        if (tankLauncher != null)
        {
            tankLauncher.OnProjectileFinished();

            if (tankLauncher.IsOutOfAmmo() && robotsDestroyed < totalRobots)
                Invoke(nameof(Defeat), 1f);
        }
    }

    void HandleRobotDestroyed()
    {
        robotsDestroyed++;
        Debug.Log($"🎯 Robots destruidos: {robotsDestroyed}/{totalRobots}");

        if (robotsDestroyed >= totalRobots && !levelEnded)
            Invoke(nameof(Victory), 2f);
    }

    // --- SPAWNS ---
    void SpawnTank()
    {
        if (tankPrefab == null)
        {
            Debug.LogError("❌ No hay Tank Prefab asignado!");
            return;
        }

        GameObject tankInstance = Instantiate(tankPrefab, currentLevelData.tankSpawnPosition, Quaternion.identity);
        tankLauncher = tankInstance.GetComponent<TankLauncherScript>();

        if (tankLauncher == null)
        {
            Debug.LogError("❌ El Tank Prefab no tiene TankLauncherScript!");
            return;
        }

        // Configurar proyectiles del nivel
        tankLauncher.SetupProjectiles(currentLevelData.proyectilesForThisLevel, currentLevelData.proyectileQuantities);

        Debug.Log("✅ Tanque spawneado en: " + currentLevelData.tankSpawnPosition);
    }

    void SpawnRobots()
    {
        if (currentLevelData.robots == null || currentLevelData.robots.Length == 0)
        {
            Debug.LogWarning("⚠️ No hay robots configurados en este nivel.");
            return;
        }

        totalRobots = currentLevelData.robots.Length;

        for (int i = 0; i < totalRobots; i++)
        {
            var data = currentLevelData.robots[i];
            if (data.robotPrefab == null)
            {
                Debug.LogError($"❌ El robot #{i} no tiene prefab asignado en {currentLevelData.levelName}");
                continue;
            }

            Instantiate(data.robotPrefab, data.spawnPosition, Quaternion.identity);
        }

        Debug.Log($"🤖 Spawneados {totalRobots} robots distintos");
    }

    // --- RESULTADOS ---
    void Victory()
    {
        if (levelEnded) return;
        levelEnded = true;

        Debug.Log("🎉 ¡VICTORIA!");
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    void Defeat()
    {
        if (levelEnded) return;
        levelEnded = true;

        Debug.Log("💀 Derrota - Robots restantes: " + (totalRobots - robotsDestroyed));

        CameraMovement cam = Camera.main.GetComponent<CameraMovement>();
        if (cam != null)
        {
            cam.ResetPositionCamera();  // 🔧 Usa el startPosition interno
        }

        if (defeatPanel != null) defeatPanel.SetActive(true);
    }

    // --- UI ---
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextLevel()
    {
        if (nextLevelData != null)
        {
            LevelLoader.currentLevel = nextLevelData;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Debug.Log("🏁 No hay siguiente nivel asignado.");
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}