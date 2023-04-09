namespace LM2.SaveTools
{
    public static class Utils
    {
        public static int GetTowerModeIndex(TowerMode mode, TowerFloor floor, TowerDifficulty difficulty)
        {
            return ((int)mode * 12) + ((int)floor * 3) + (int)difficulty;
        }
    }
}
