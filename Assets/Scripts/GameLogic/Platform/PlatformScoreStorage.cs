using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformScoreStorage : IPlatformScoreStorage
{
    private const string BestScoreKey = "BestPlatformScore";

    public int BestScore => PlayerPrefs.GetInt(BestScoreKey, 0);

    public int SaveIfBest(int score)
    {
        int bestScore = BestScore;

        if (score <= bestScore)
            return bestScore;

        PlayerPrefs.SetInt(BestScoreKey, score);
        PlayerPrefs.Save();

        return score;
    }
}
