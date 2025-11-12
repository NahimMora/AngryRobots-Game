using UnityEngine;
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Score Configuration")]
    [SerializeField] private int pointsPerPig = 5000;
    [SerializeField] private int pointsPerBlock = 100;
    [SerializeField] private int bonusPerUnusedBird = 10000;
    [SerializeField] private int[] starThresholds = { 30000, 60000, 90000 };

    private int currentScore = 0;
    private int pingsDestroyed = 0;

    public void AddPigScore()
    {
        currentScore += pointsPerPig;
        pingsDestroyed++;
    }

    public void AddBlockScore(float damage)
    {
        currentScore += Mathf.RoundToInt(damage * pointsPerBlock);
    }

    public int GetStars()
    {
        for (int i = starThresholds.Length - 1; i >= 0; i--)
        {
            if (currentScore >= starThresholds[i]) return i + 1;
        }
        return 0;
    }
}