using UnityEngine;

[System.Serializable]
public class RobotSpawnData
{
    public GameObject robotPrefab;
    public Vector3 spawnPosition;
}

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("General Info")]
    public string levelName;

    [Header("Proyectiles")]
    public GameObject[] proyectilesForThisLevel;
    public int[] proyectileQuantities;

    [Header("Spawns")]
    public Vector3 tankSpawnPosition;
    public Vector3 cameraStartPosition;

    [Header("Robots del nivel")]
    public RobotSpawnData[] robots;  // 👈 lista de robots con su prefab y posición
}
