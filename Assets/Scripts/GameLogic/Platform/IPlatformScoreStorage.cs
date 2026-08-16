using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlatformScoreStorage
{
    int BestScore { get; }
    int BestTime { get; }

    void SaveIfBest(int score, int time);
}