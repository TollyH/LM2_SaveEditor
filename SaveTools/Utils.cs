using System.Collections.Immutable;

namespace LM2.SaveTools
{
    public static class Utils
    {
        public static readonly ImmutableDictionary<int, string> MissionIndices = new Dictionary<int, string>()
        {
            { 0, "A-1" },
            { 1, "A-2" },
            { 2, "A-3" },
            { 3, "A-4" },
            { 4, "A-5" },
            { 5, "A-Boss" },
            { 7, "A-Bonus" },

            { 10, "B-1" },
            { 11, "B-2" },
            { 12, "B-3" },
            { 13, "B-4" },
            { 14, "B-5" },
            { 15, "B-Boss" },
            { 18, "B-Bonus" },

            { 20, "C-1" },
            { 21, "C-2" },
            { 22, "C-3" },
            { 23, "C-4" },
            { 24, "C-5" },
            { 25, "C-Boss" },
            { 27, "C-Bonus" },

            { 30, "D-1" },
            { 31, "D-2" },
            { 32, "D-3" },
            { 35, "D-Boss" },
            { 38, "D-Bonus" },

            { 40, "E-1" },
            { 41, "E-2" },
            { 42, "E-3" },
            { 43, "E-4" },
            { 44, "E-5" },
            { 46, "E-Boss" },
            { 49, "E-Bonus" },

            { 50, "King Boo" }
        }.ToImmutableDictionary();

        public static int GetTowerModeIndex(TowerMode mode, TowerFloor floor, TowerDifficulty difficulty)
        {
            return ((int)mode * 12) + ((int)floor * 3) + (int)difficulty;
        }
    }
}
