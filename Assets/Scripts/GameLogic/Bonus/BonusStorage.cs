using UnityEngine;

public class BonusStorage : IBonusStorage
{
    private const string StarKey = "LastSession_Stars";
    private const string HearthKey = "LastSession_Hearths";
    private const string BestStarKey = "BestBonus_Stars";
    private const string BestHearthKey = "BestBonus_Hearths";

    public int StarCount => PlayerPrefs.GetInt(StarKey, 0);
    public int HearthCount => PlayerPrefs.GetInt(HearthKey, 0);
    public int BestStarCount => PlayerPrefs.GetInt(BestStarKey, 0);
    public int BestHearthCount => PlayerPrefs.GetInt(BestHearthKey, 0);

    public void Save(IBonusModel bonusModel)
    {
        CountBonuses(bonusModel, out int stars, out int hearths);

        PlayerPrefs.SetInt(StarKey, stars);
        PlayerPrefs.SetInt(HearthKey, hearths);
    }

    public void SaveBest(IBonusModel bonusModel)
    {
        CountBonuses(bonusModel, out int stars, out int hearths);

        if (stars > BestStarCount)
            PlayerPrefs.SetInt(BestStarKey, stars);

        if (hearths > BestHearthCount)
            PlayerPrefs.SetInt(BestHearthKey, hearths);
    }

    public void Clear()
    {
        PlayerPrefs.SetInt(StarKey, 0);
        PlayerPrefs.SetInt(HearthKey, 0);
    }

    public void Flush()
    {
        PlayerPrefs.Save();
    }

    private static void CountBonuses(IBonusModel bonusModel, out int stars, out int hearths)
    {
        stars = 0;
        hearths = 0;

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
    }
}
