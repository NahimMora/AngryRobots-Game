using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject gamePanel;

    [Header("References")]
    [SerializeField] private LevelManagerScript levelManager;
    [SerializeField] private LevelData level1Data;
    [SerializeField] private LevelData level2Data;

    void Start()
    {
        // Solo el menú principal visible al inicio
        mainMenuPanel.SetActive(true);
        levelSelectPanel.SetActive(false);
        gamePanel.SetActive(false);
    }

    public void OnPlayButton()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
    }

    public void OnBackToMenu()
    {
        mainMenuPanel.SetActive(true);
        levelSelectPanel.SetActive(false);
        gamePanel.SetActive(false);
    }

    public void LoadLevel1()
    {
        StartLevel(level1Data);
    }

    public void LoadLevel2()
    {
        StartLevel(level2Data);
    }

    private void StartLevel(LevelData data)
    {
        if (data == null)
        {
            Debug.LogError("❌ No hay LevelData asignado!");
            return;
        }

        // Ocultar menús
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
        gamePanel.SetActive(true);

        // Iniciar nivel
        if (levelManager != null)
            levelManager.IniciarNivelConData(data);
        else
            Debug.LogError("⚠️ No hay LevelManager asignado!");
    }

}
