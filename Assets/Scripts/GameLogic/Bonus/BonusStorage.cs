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

        // Результат новой игры заменяет результат предыдущей.
        PlayerPrefs.SetInt(StarKey, stars);
        PlayerPrefs.SetInt(HearthKey, hearths);
        PlayerPrefs.Save();
    }

    public void SaveBest(IBonusModel bonusModel)
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

        if (stars > BestStarCount)
            PlayerPrefs.SetInt(BestStarKey, stars);

        if (hearths > BestHearthCount)
            PlayerPrefs.SetInt(BestHearthKey, hearths);

        PlayerPrefs.Save();
    }

    public void Clear()
    {
        PlayerPrefs.SetInt(StarKey, 0);
        PlayerPrefs.SetInt(HearthKey, 0);
        PlayerPrefs.Save();
    }
}
