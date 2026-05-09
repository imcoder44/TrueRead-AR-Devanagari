using System;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;

    void Awake()
    {
        // This makes the "invisible backpack" follow you between scenes!
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    // ── XP and Levels ─────────────────────────────────
    private static readonly int[] XP_THRESHOLDS =
        { 0, 200, 500, 1000, 2000, 3500, 5000, 7500 };

    public void AddXP(int amount)
    {
        int xp = PlayerPrefs.GetInt("total_xp", 0) + amount;
        PlayerPrefs.SetInt("total_xp", xp);
        int level = 1;
        for (int i = 0; i < XP_THRESHOLDS.Length; i++)
            if (xp >= XP_THRESHOLDS[i]) level = i + 1;
        PlayerPrefs.SetInt("current_level", level);
        PlayerPrefs.Save();
    }

    public int GetLevel() => PlayerPrefs.GetInt("current_level", 1);
    public int GetXP() => PlayerPrefs.GetInt("total_xp", 0);

    // ── Session saving ─────────────────────────────────
    public void SaveSession(int score, int questions, int streak)
    {
        int n = PlayerPrefs.GetInt("session_count", 0);
        PlayerPrefs.SetInt("session_count", n + 1);

        PlayerPrefs.SetInt("total_questions", PlayerPrefs.GetInt("total_questions", 0) + questions);
        PlayerPrefs.SetInt("correct_answers", PlayerPrefs.GetInt("correct_answers", 0) + score);

        int bestStreak = PlayerPrefs.GetInt("best_streak", 0);
        if (streak > bestStreak) PlayerPrefs.SetInt("best_streak", streak);

        int bestScore = PlayerPrefs.GetInt("best_score", 0);
        if (score > bestScore) PlayerPrefs.SetInt("best_score", score);

        // Give XP (10 points per correct answer)
        AddXP(score * 10);

        PlayerPrefs.SetString("last_open_date", DateTime.Now.ToString("yyyy-MM-dd"));
        PlayerPrefs.Save();
    }

    // ── Badge unlocking ────────────────────────────────
    public void UnlockBadge(string badgeKey)
    {
        if (PlayerPrefs.GetInt("badge_" + badgeKey, 0) == 1) return;
        PlayerPrefs.SetInt("badge_" + badgeKey, 1);
        AddXP(GetBadgeXP(badgeKey));
        PlayerPrefs.Save();
    }

    public bool HasBadge(string key) => PlayerPrefs.GetInt("badge_" + key, 0) == 1;

    int GetBadgeXP(string key)
    {
        switch (key)
        {
            case "firststep": return 10;
            case "hotstreak": return 30;
            case "onfire": return 60;
            case "perfect": return 80;
            case "hindihero": return 500;
            default: return 20;
        }
    }
}