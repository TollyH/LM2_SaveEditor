namespace LM2.SaveTools
{
    public struct MissionInfo
    {
        public bool Completed { get; set; }
        public bool Locked { get; set; }
        public bool BooCaptured { get; set; }
        public float FastestTime { get; set; }
        public ushort GhostsCaptured { get; set; }
        public ushort DamageTaken { get; set; }
        public ushort TreasureCollected { get; set; }
        public Grade Grade { get; set; }
    }

    public struct BasicGhostInfo
    {
        public byte NumCollected { get; set; }
        public ushort MaxWeight { get; set; }
        public bool IsNew { get; set; }
    }

    public struct GhostInfo
    {
        public bool Collected { get; set; }
        public byte NumCollected { get; set; }
        public ushort MaxWeight { get; set; }
        public bool IsNew { get; set; }
    }

    public struct GemInfo
    {
        public bool Collected { get; set; }
        public bool IsNew { get; set; }
    }

    public struct EndlessModeInfo
    {
        public bool Unlocked { get; set; }
        public byte HighestFloor { get; set; }
    }

    public partial class SaveData
    {
        public partial class GameData
        {
            public MissionInfo[] GetMissionInfo()
            {
                List<MissionInfo> list = new();
                for (int i = 0; i < MissionCompletion.Length; i++)
                {
                    list.Add(new MissionInfo()
                    {
                        Completed = MissionCompletion[i],
                        Locked = MissionLocked[i],
                        BooCaptured = MissionBooCaptured[i],
                        FastestTime = MissionClearTime[i],
                        GhostsCaptured = MissionGhostsCaptured[i],
                        DamageTaken = MissionDamageTaken[i],
                        TreasureCollected = MissionTreasureCollected[i],
                        Grade = MissionGrade[i]
                    });
                }
                return list.ToArray();
            }

            public BasicGhostInfo[] GetBasicGhostInfo()
            {
                List<BasicGhostInfo> list = new();
                for (int i = 0; i < NumBasicGhostCollected.Length; i++)
                {
                    list.Add(new BasicGhostInfo()
                    {
                        NumCollected = NumBasicGhostCollected[i],
                        MaxWeight = MaxBasicGhostWeight[i],
                        IsNew = BasicGhostNotifyState[i] == 2
                    });
                }
                return list.ToArray();
            }

            public GhostInfo[] GetGhostInfo()
            {
                List<GhostInfo> list = new();
                for (int i = 0; i < NumGhostCollected.Length; i++)
                {
                    list.Add(new GhostInfo()
                    {
                        Collected = GhostCollectableState[i] == 2,
                        NumCollected = NumGhostCollected[i],
                        MaxWeight = MaxGhostWeight[i],
                        IsNew = GhostNotifyState[i] == 2
                    });
                }
                return list.ToArray();
            }

            public GemInfo[] GetGemInfo()
            {
                List<GemInfo> list = new();
                for (int i = 0; i < GemCollected.Length; i++)
                {
                    list.Add(new GemInfo()
                    {
                        Collected = GemCollected[i],
                        IsNew = GemNotifyState[i] == 2
                    });
                }
                return list.ToArray();
            }

            public EndlessModeInfo[] GetEndlessModeInfo()
            {
                List<EndlessModeInfo> list = new();
                for (int i = 0; i < EndlessFloorsUnlocked.Length; i++)
                {
                    list.Add(new EndlessModeInfo()
                    {
                        Unlocked = EndlessFloorsUnlocked[i],
                        HighestFloor = EndlessModeHighestFloorReached[i]
                    });
                }
                return list.ToArray();
            }

            public void UpdateFromMissionInfo(int index, MissionInfo missionInfo)
            {
                MissionCompletion[index] = missionInfo.Completed;
                MissionLocked[index] = missionInfo.Locked;
                MissionBooCaptured[index] = missionInfo.BooCaptured;
                MissionClearTime[index] = missionInfo.FastestTime;
                MissionGhostsCaptured[index] = missionInfo.GhostsCaptured;
                MissionDamageTaken[index] = missionInfo.DamageTaken;
                MissionTreasureCollected[index] = missionInfo.TreasureCollected;
                MissionGrade[index] = missionInfo.Grade;
            }

            public void UpdateFromBasicGhostInfo(int index, BasicGhostInfo basicGhostInfo)
            {
                NumBasicGhostCollected[index] = basicGhostInfo.NumCollected;
                MaxBasicGhostWeight[index] = basicGhostInfo.MaxWeight;
                BasicGhostNotifyState[index] = (byte)(basicGhostInfo.IsNew ? 2 : 0);
            }

            public void UpdateFromGhostInfo(int index, GhostInfo ghostInfo)
            {
                GhostCollectableState[index] = (byte)(ghostInfo.Collected ? 2 : 0);
                NumGhostCollected[index] = ghostInfo.NumCollected;
                MaxGhostWeight[index] = ghostInfo.MaxWeight;
                GhostNotifyState[index] = (byte)(ghostInfo.IsNew ? 2 : 0);
            }

            public void UpdateFromGemInfo(int index, GemInfo gemInfo)
            {
                GemCollected[index] = gemInfo.Collected;
                GemNotifyState[index] = (byte)(gemInfo.IsNew ? 2 : 0);
            }

            public void UpdateFromEndlessModeInfo(int index, EndlessModeInfo endlessModeInfo)
            {
                EndlessFloorsUnlocked[index] = endlessModeInfo.Unlocked;
                EndlessModeHighestFloorReached[index] = endlessModeInfo.HighestFloor;
            }
        }
    }
}
