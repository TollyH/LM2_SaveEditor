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

    public struct BasicGhost
    {
        public byte NumCollected { get; set; }
        public ushort MaxWeight { get; set; }
        public bool IsNew { get; set; }
    }

    public struct Ghost
    {
        public bool Collected { get; set; }
        public byte NumCollected { get; set; }
        public ushort MaxWeight { get; set; }
        public bool IsNew { get; set; }
    }

    public struct Gem
    {
        public bool Collected { get; set; }
        public bool IsNew { get; set; }
    }

    public struct EndlessMode
    {
        public bool Unlocked { get; set; }
        public byte HighestFloor { get; set; }
    }
}
