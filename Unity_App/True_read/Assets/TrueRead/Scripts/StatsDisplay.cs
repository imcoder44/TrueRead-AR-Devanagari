using UnityEngine;
using TMPro;

public class StatsDisplay : MonoBehaviour
{
    [Header("The Overall Progress Title")]
    public TextMeshProUGUI overallProgressTMP;

    [Header("Your 6 Stat Cards")]
    public TextMeshProUGUI hindiLettersTMP;
    public TextMeshProUGUI bestScoreTMP;
    public TextMeshProUGUI quizScoreTMP;     // Total XP
    public TextMeshProUGUI bestStreakTMP;
    public TextMeshProUGUI totalQuestionsTMP;
    public TextMeshProUGUI accuracyTMP;

    void Start()
    {
        RefreshStats();
    }

    void RefreshStats()
    {
        int totalQuestions = PlayerPrefs.GetInt("total_questions", 0);
        int correctAnswers = PlayerPrefs.GetInt("correct_answers", 0);
        int bestStreak = PlayerPrefs.GetInt("best_streak", 0);
        int bestScore = PlayerPrefs.GetInt("best_score", 0);
        int totalXP = PlayerPrefs.GetInt("total_xp", 0);

        int totalMasteryPoints = 0;
        int maxPossibleMastery = 46 * 5;
        for (int i = 0; i < 46; i++)
        {
            totalMasteryPoints += PlayerPrefs.GetInt("mastery_" + i, 0);
        }

        float accuracy = 0f;
        if (totalQuestions > 0)
        {
            accuracy = ((float)correctAnswers / totalQuestions) * 100f;
        }

        if (accuracyTMP) accuracyTMP.text = Mathf.RoundToInt(accuracy) + "%";
        if (totalQuestionsTMP) totalQuestionsTMP.text = totalQuestions.ToString();
        if (bestStreakTMP) bestStreakTMP.text = bestStreak.ToString();
        if (quizScoreTMP) quizScoreTMP.text = totalXP.ToString();

        // Assuming 3 questions per quiz for now
        if (bestScoreTMP) bestScoreTMP.text = bestScore.ToString() + "/3";

        if (hindiLettersTMP) hindiLettersTMP.text = totalMasteryPoints.ToString() + "/" + maxPossibleMastery.ToString();

        // Calculate Overall Progress Percentage
        float accPercent = accuracy;
        float totalQPercent = Mathf.Clamp01((float)totalQuestions / 50f) * 100f; // Maxed out at 50 questions
        float streakPercent = Mathf.Clamp01((float)bestStreak / 10f) * 100f;     // Maxed out at 10 streak
        float xpPercent = Mathf.Clamp01((float)totalXP / 1000f) * 100f;          // Maxed out at 1000 XP
        float scorePercent = Mathf.Clamp01((float)bestScore / 3f) * 100f;        // Maxed out at 3/3 score
        float masteryPercent = Mathf.Clamp01((float)totalMasteryPoints / maxPossibleMastery) * 100f;

        float overallAvg = (accPercent + totalQPercent + streakPercent + xpPercent + scorePercent + masteryPercent) / 6f;

        if (overallProgressTMP)
        {
            overallProgressTMP.text = Mathf.RoundToInt(overallAvg) + "%";
        }
    }

    public void OnBackPressed()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}