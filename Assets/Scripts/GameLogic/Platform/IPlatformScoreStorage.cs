using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlatformScoreStorage
{
    int BestScore { get; }

    int SaveIfBest(int score);
}