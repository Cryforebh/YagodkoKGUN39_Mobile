using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformScoreStorage : IPlatformScoreStorage
{
    private const string BestScoreKey = "BestPlatformScore";
    private const string BestTimeKey = "BestPlatformTime";

    public int BestScore => PlayerPrefs.GetInt(BestScoreKey, 0);
    public int BestTime => PlayerPrefs.GetInt(BestTimeKey, 0);

    public void SaveIfBest(int score, int time)
    {
        bool hasResult = PlayerPrefs.HasKey(BestScoreKey);

        if (hasResult && score <= BestScore)
            return;

        PlayerPrefs.SetInt(BestScoreKey, score);
        PlayerPrefs.SetInt(BestTimeKey, time);
        PlayerPrefs.Save();
    }
}
