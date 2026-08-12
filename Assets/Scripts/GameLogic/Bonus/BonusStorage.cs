using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusStorage : IBonusStorage
{
    private const string StarKey = "LastSession_Stars";
    private const string HearthKey = "LastSession_Hearths";

    public int StarCount => PlayerPrefs.GetInt(StarKey, 0);

    public int HearthCount => PlayerPrefs.GetInt(HearthKey, 0);

    public void Save(IBonusModel bonusModel)
    {
        int stars = 0;
        int hearths = 0;

        foreach (Bonuses bonus in bonusModel.CollectedBonuses)
        {
            switch (bonus)
            {
                case Bonuses.Star:
                    stars++;
                    break;

                case Bonuses.Hearth:
                    hearths++;
                    break;
            }
        }

        int totalStars = StarCount + stars;
        int totalHearths = HearthCount + hearths;

        PlayerPrefs.SetInt(StarKey, totalStars);
        PlayerPrefs.SetInt(HearthKey, totalHearths);

        PlayerPrefs.Save();
    }
}
