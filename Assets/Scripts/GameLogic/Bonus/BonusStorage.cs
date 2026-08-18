using UnityEngine;

public class BonusStorage : IBonusStorage
{
    private const string StarKey = "LastSession_Stars";
    private const string HeartKey = "LastSession_Hearths";
    private const string BestStarKey = "BestBonus_Stars";
    private const string BestHeartKey = "BestBonus_Hearths";

    public int StarCount => PlayerPrefs.GetInt(StarKey, 0);
    public int HeartCount => PlayerPrefs.GetInt(HeartKey, 0);
    public int BestStarCount => PlayerPrefs.GetInt(BestStarKey, 0);
    public int BestHeartCount => PlayerPrefs.GetInt(BestHeartKey, 0);

    public void Save(IBonusModel bonusModel)
    {
        CountBonuses(bonusModel, out int stars, out int hearths);

        PlayerPrefs.SetInt(StarKey, stars);
        PlayerPrefs.SetInt(HeartKey, hearths);
    }

    public void SaveBest(IBonusModel bonusModel)
    {
        CountBonuses(bonusModel, out int stars, out int hearts);

        if (stars > BestStarCount)
            PlayerPrefs.SetInt(BestStarKey, stars);

        if (hearts > BestHeartCount)
            PlayerPrefs.SetInt(BestHeartKey, hearts);
    }

    public void Clear()
    {
        PlayerPrefs.SetInt(StarKey, 0);
        PlayerPrefs.SetInt(HeartKey, 0);
    }

    public void Flush()
    {
        PlayerPrefs.Save();
    }

    private static void CountBonuses(IBonusModel bonusModel, out int stars, out int hearts)
    {
        stars = 0;
        hearts = 0;

        foreach (BonusType bonus in bonusModel.CollectedBonuses)
        {
            switch (bonus)
            {
                case BonusType.Star:
                    stars++;
                    break;

                case BonusType.Heart:
                    hearts++;
                    break;
            }
        }
    }
}
