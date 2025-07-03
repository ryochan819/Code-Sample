using Steamworks;

public static class SteamAchievement
{
    public static void UnlockAchievement(Achievement achievement)
    {
        if (SteamManager.Initialized)
        {
            Steamworks.SteamUserStats.GetAchievement(achievement.ToString(), out bool achievementCompleted);
            if (!achievementCompleted)
            {
                SteamUserStats.SetAchievement(achievement.ToString());
                SteamUserStats.StoreStats();
            }
        }
    }

    public enum Achievement
    {
        MENU_DRAW
    }
}
