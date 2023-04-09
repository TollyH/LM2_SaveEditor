namespace LM2.SaveTools
{
    public partial class SaveData
    {
        public partial class GameData
        {
            public byte IntendedPoltergustUpgradeLevel()
            {
                if (TotalTreasureAcquired < 2_000)
                {
                    return 1;
                }
                else if (TotalTreasureAcquired < 7_000)
                {
                    return 2;
                }
                return 3;
            }

            public byte IntendedDarklightUpgradeLevel()
            {
                if (TotalTreasureAcquired < 4_000)
                {
                    return 1;
                }
                else if (TotalTreasureAcquired < 10_000)
                {
                    return 2;
                }
                return 3;
            }

            public bool ShouldLuigiHavePoltergust()
            {
                // Has A-1 "Poltergust 5000" been completed?
                return MissionCompletion[0];
            }

            public bool ShouldSuperPoltergustBeUnlocked()
            {
                return TotalTreasureAcquired >= 20_000;
            }

            public bool ShouldMarioBeRevealed()
            {
                // Has E-3 "A Train to Catch" been completed?
                return MissionCompletion[42];
            }

            public bool ShouldRandomTowerBeUnlocked()
            {
                // Have Hunter, Rush, and Polterpup modes all been completed?
                // (on any difficulty and floor setting)
                return BestTowerClearTime[..12].Any(x => x > 0) && BestTowerClearTime[12..24].Any(x => x > 0)
                    && BestTowerClearTime[24..36].Any(x => x > 0) && BestTowerClearTime[36..].Any(x => x > 0);
            }

            public bool[] IntendedEndlessTowerUnlockStates()
            {
                bool[] states = new bool[4];

                // Hunter endless is unlocked once 25F mode has been cleared in Hunter mode
                // (in any difficulty)
                states[0] = BestTowerClearTime[6..9].Any(x => x > 0);

                // Rush endless is unlocked once 25F mode has been cleared in Rush mode
                // (in any difficulty)
                states[1] = BestTowerClearTime[18..21].Any(x => x > 0);

                // Polterpup endless is unlocked once 25F mode has been cleared in Polterpup mode
                // (in any difficulty)
                states[2] = BestTowerClearTime[30..33].Any(x => x > 0);

                // Surprise endless is unlocked once 25F mode has been cleared in Surprise mode
                // (in any difficulty)
                states[3] = BestTowerClearTime[42..45].Any(x => x > 0);

                return states;
            }
        }
    }
}
