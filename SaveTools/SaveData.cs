namespace LM2.SaveTools
{
    public class SaveData
    {
        public class TitleScreenData
        {
            public uint DataCRC => CalculateDataCRC();
            public static uint VersionCRC => 0x49270C7B;
            public byte FurthestClearedMansion { get; set; }
            public byte FurthestClearedMission { get; set; }
            public byte HighestTowerFloor { get; set; }
            public int TotalTreasureAcquired { get; set; }
            public byte BoosCaptured { get; set; }
            public byte DarkMoonPieces { get; set; }
            public byte EGaddMedals { get; set; }
            public long PlaytimeSeconds { get; set; }

            public TitleScreenData(byte[] titleScreenSaveBytes, bool ignoreCRC = false)
            {
                throw new NotImplementedException();
            }

            public byte[] GetBytes()
            {
                throw new NotImplementedException();
            }

            public uint CalculateDataCRC()
            {
                throw new NotImplementedException();
            }
        }

        public class GameData
        {
            public uint DataCRC => CalculateDataCRC();
            public static uint VersionCRC => 0xD43203AD;

            public byte[] DiscoveredNIS { get; set; }

            public bool[] MissionCompletion { get; set; }
            public bool[] MissionLocked { get; set; }
            public Grade[] MissionGrade { get; set; }
            public Grade[] MissionPrevGrade { get; set; }
            public bool[] MissionBooCaptured { get; set; }
            public byte[] MissionBooNotifyState { get; set; }
            public byte[] MissionNotifyState { get; set; }
            public float[] MissionClearTime { get; set; }
            public short[] MissionGhostsCaptured { get; set; }
            public short[] MissionDamageTaken { get; set; }
            public short[] MissionTreasureCollected { get; set; }

            public byte[] NumBasicGhostCollected { get; set; }
            public short[] MaxBasicGhostWeight { get; set; }
            public byte[] BasicGhostNotifyState { get; set; }
            public byte[] BasicGhostNotifyBecauseHigherWeight { get; set; }

            public bool AnyOptionalBooCaptured { get; set; }
            public bool JustCollectedPolterpup { get; set; }

            public short[] GhostWeightRequirement { get; set; }
            public byte[] GhostCollectableState { get; set; }
            public byte[] NumGhostCollected { get; set; }
            public short[] MaxGhostWeight { get; set; }
            public byte[] GhostNotifyState { get; set; }
            public byte[] GhostNotifyBecauseHigherWeight { get; set; }

            public bool[] GemCollected { get; set; }
            public byte[] GemNotifyState { get; set; }

            public bool HasPoltergust { get; set; }
            public bool SeenInitialDualScreamAnimation { get; set; }
            public bool HasMarioBeenRevealedInTheStory { get; set; }

            public byte LastMansionPlayed { get; set; }

            public int TotalTreasureAcquired { get; set; }
            public int TreasureToNotifyDuringUnloading { get; set; }
            public int TotalGhostWeightAcquired { get; set; }

            public byte DarklightUpgradeLevel { get; set; }
            public byte DarklightNotifyState { get; set; }
            public byte PoltergustUpgradeLevel { get; set; }
            public byte PoltergustNotifyState { get; set; }
            public bool HasSuperPoltergust { get; set; }
            public byte SuperPoltergustNotifyState { get; set; }

            public bool HasSeenReviveBonePIP { get; set; }

            public short[] BestTowerClearTime { get; set; }
            public int EndlessModeHighestFloorReached { get; set; }
            public byte AnyModeHighestFloorReached { get; set; }
            public int EndlessFloorsUnlocked { get; set; }
            public bool RandomTowerUnlocked { get; set; }
            public byte TowerNotifyState { get; set; }

            public GameData(byte[] gameDataBytes, bool ignoreCRC = false)
            {
                throw new NotImplementedException();
            }

            public byte[] GetBytes()
            {
                throw new NotImplementedException();
            }

            public uint CalculateDataCRC()
            {
                throw new NotImplementedException();
            }
        }

        public SaveData(byte[] saveBytes, bool ignoreCRC = false)
        {
            throw new NotImplementedException();
        }

        public byte[] GetBytes()
        {
            throw new NotImplementedException();
        }
    }
}
