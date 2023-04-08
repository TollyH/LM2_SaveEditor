using System.Buffers.Binary;

namespace LM2.SaveTools
{
    public class SaveData
    {
        public class TitleScreenData
        {
            public uint DataCRC => CRC.CalculateChecksum(GetBytes(false));
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
                if (titleScreenSaveBytes.Length != 0x1A)
                {
                    throw new InvalidDataException(
                        $"Title screen save data must be 0x1A (26) bytes long. {titleScreenSaveBytes.Length} bytes were provided.");
                }

                Span<byte> titleScreenSpan = titleScreenSaveBytes.AsSpan();

                if (!ignoreCRC)
                {
                    uint apparentDataCRC = BinaryPrimitives.ReadUInt32LittleEndian(titleScreenSpan[..4]);
                    if (apparentDataCRC != CRC.CalculateChecksum(titleScreenSpan[4..]))
                    {
                        throw new InvalidDataException(
                            "Given title screen save data has an invalid data checksum. Set the ignoreCRC parameter to true to ignore this in the future.");
                    }

                    uint givenVersionCRC = BinaryPrimitives.ReadUInt32LittleEndian(titleScreenSpan[4..8]);
                    if (givenVersionCRC != VersionCRC)
                    {
                        throw new InvalidDataException(
                            "Given title screen save data has an invalid version checksum (this should always be 0x7B, 0x0C, 0x27, 0x49). " +
                            "Set the ignoreCRC parameter to true to ignore this in the future.");
                    }
                }

                FurthestClearedMansion = titleScreenSpan[8];
                FurthestClearedMission = titleScreenSpan[9];
                HighestTowerFloor = titleScreenSpan[10];
                TotalTreasureAcquired = BinaryPrimitives.ReadInt32LittleEndian(titleScreenSpan[11..15]);
                BoosCaptured = titleScreenSpan[15];
                DarkMoonPieces = titleScreenSpan[16];
                EGaddMedals = titleScreenSpan[17];
                PlaytimeSeconds = BinaryPrimitives.ReadInt64LittleEndian(titleScreenSpan[18..26]);
            }

            public byte[] GetBytes(bool includeDataCRC = true)
            {
                byte[] titleSaveBytes = new byte[includeDataCRC ? 26 : 22];

                Span<byte> titleSaveSpan;
                if (includeDataCRC)
                {
                    BinaryPrimitives.WriteUInt32LittleEndian(titleSaveBytes, DataCRC);
                    titleSaveSpan = titleSaveBytes.AsSpan()[4..];
                }
                else
                {
                    titleSaveSpan = titleSaveBytes.AsSpan();
                }

                BinaryPrimitives.WriteUInt32LittleEndian(titleSaveSpan, VersionCRC);
                titleSaveSpan[4] = FurthestClearedMansion;
                titleSaveSpan[5] = FurthestClearedMission;
                titleSaveSpan[6] = HighestTowerFloor;
                BinaryPrimitives.WriteInt32LittleEndian(titleSaveSpan[7..11], TotalTreasureAcquired);
                titleSaveSpan[11] = BoosCaptured;
                titleSaveSpan[12] = DarkMoonPieces;
                titleSaveSpan[13] = EGaddMedals;
                BinaryPrimitives.WriteInt64LittleEndian(titleSaveSpan[14..22], PlaytimeSeconds);

                return titleSaveBytes;
            }
        }

        public class GameData
        {
            public uint DataCRC => CRC.CalculateChecksum(GetBytes(false));
            public static uint VersionCRC => 0xD43203AD;

            public byte[] DiscoveredNIS { get; private set; }

            public bool[] MissionCompletion { get; private set; }
            public bool[] MissionLocked { get; private set; }
            public Grade[] MissionGrade { get; private set; }
            public Grade[] MissionPrevGrade { get; private set; }
            public bool[] MissionBooCaptured { get; private set; }
            public byte[] MissionBooNotifyState { get; private set; }
            public byte[] MissionNotifyState { get; private set; }
            public float[] MissionClearTime { get; private set; }
            public short[] MissionGhostsCaptured { get; private set; }
            public short[] MissionDamageTaken { get; private set; }
            public short[] MissionTreasureCollected { get; private set; }

            public byte[] NumBasicGhostCollected { get; private set; }
            public short[] MaxBasicGhostWeight { get; private set; }
            public byte[] BasicGhostNotifyState { get; private set; }
            public byte[] BasicGhostNotifyBecauseHigherWeight { get; private set; }

            public bool AnyOptionalBooCaptured { get; set; }
            public bool JustCollectedPolterpup { get; set; }

            public short[] GhostWeightRequirement { get; private set; }
            public byte[] GhostCollectableState { get; private set; }
            public byte[] NumGhostCollected { get; private set; }
            public short[] MaxGhostWeight { get; private set; }
            public byte[] GhostNotifyState { get; private set; }
            public byte[] GhostNotifyBecauseHigherWeight { get; private set; }

            public bool[] GemCollected { get; private set; }
            public byte[] GemNotifyState { get; private set; }

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

            public short[] BestTowerClearTime { get; private set; }
            public int EndlessModeHighestFloorReached { get; set; }
            public byte AnyModeHighestFloorReached { get; set; }
            public int EndlessFloorsUnlocked { get; set; }
            public bool RandomTowerUnlocked { get; set; }
            public byte TowerNotifyState { get; set; }

            public GameData(byte[] gameDataBytes, bool ignoreCRC = false)
            {
                if (gameDataBytes.Length != 0xF1D)
                {
                    throw new InvalidDataException(
                        $"Game save data must be 0xF1D (3869) bytes long. {gameDataBytes.Length} bytes were provided.");
                }

                Span<byte> gameDataSpan = gameDataBytes.AsSpan();

                if (!ignoreCRC)
                {
                    uint apparentDataCRC = BinaryPrimitives.ReadUInt32LittleEndian(gameDataSpan[..4]);
                    if (apparentDataCRC != CRC.CalculateChecksum(gameDataSpan[4..]))
                    {
                        throw new InvalidDataException(
                            "Given game save data has an invalid data checksum. Set the ignoreCRC parameter to true to ignore this in the future.");
                    }

                    uint givenVersionCRC = BinaryPrimitives.ReadUInt32LittleEndian(gameDataSpan[4..8]);
                    if (givenVersionCRC != VersionCRC)
                    {
                        throw new InvalidDataException(
                            "Given game save data has an invalid version checksum (this should always be 0xAD, 0x03, 0x32, 0xD4). " +
                            "Set the ignoreCRC parameter to true to ignore this in the future.");
                    }
                }

                DiscoveredNIS = gameDataSpan[8..0x808].ToArray();

                MissionCompletion = new bool[60];
                for (int i = 0; i < 60; i++)
                {
                    MissionCompletion[i] = gameDataSpan[0x808 + i] == 1;
                }

                MissionLocked = new bool[60];
                for (int i = 0; i < 60; i++)
                {
                    MissionLocked[i] = gameDataSpan[0x844 + i] == 1;
                }

                MissionGrade = new Grade[60];
                for (int i = 0; i < 60; i++)
                {
                    MissionGrade[i] = (Grade)gameDataSpan[0x880 + i];
                }

                MissionPrevGrade = new Grade[60];
                for (int i = 0; i < 60; i++)
                {
                    MissionPrevGrade[i] = (Grade)gameDataSpan[0x8BC + i];
                }

                MissionBooCaptured = new bool[60];
                for (int i = 0; i < 60; i++)
                {
                    MissionBooCaptured[i] = gameDataSpan[0x8F8 + i] == 1;
                }

                MissionBooNotifyState = new byte[60];
                for (int i = 0; i < 60; i++)
                {
                    MissionBooNotifyState[i] = gameDataSpan[0x934 + i];
                }

                MissionNotifyState = new byte[60];
                for (int i = 0; i < 60; i++)
                {
                    MissionNotifyState[i] = gameDataSpan[0x970 + i];
                }

                MissionClearTime = new float[60];
                for (int i = 0; i < 60; i++)
                {
                    MissionClearTime[i] = BinaryPrimitives.ReadSingleLittleEndian(gameDataSpan[((i * 4) + 0x9AC)..]);
                }

                MissionGhostsCaptured = new short[60];
                for (int i = 0; i < 60; i++)
                {
                    MissionGhostsCaptured[i] = BinaryPrimitives.ReadInt16LittleEndian(gameDataSpan[((i * 2) + 0xA9C)..]);
                }

                MissionDamageTaken = new short[60];
                for (int i = 0; i < 60; i++)
                {
                    MissionDamageTaken[i] = BinaryPrimitives.ReadInt16LittleEndian(gameDataSpan[((i * 2) + 0xB14)..]);
                }

                MissionTreasureCollected = new short[60];
                for (int i = 0; i < 60; i++)
                {
                    MissionTreasureCollected[i] = BinaryPrimitives.ReadInt16LittleEndian(gameDataSpan[((i * 2) + 0xB8C)..]);
                }

                NumBasicGhostCollected = new byte[29];
                for (int i = 0; i < 29; i++)
                {
                    NumBasicGhostCollected[i] = gameDataSpan[0xC04 + i];
                }

                MaxBasicGhostWeight = new short[29];
                for (int i = 0; i < 29; i++)
                {
                    MaxBasicGhostWeight[i] = BinaryPrimitives.ReadInt16LittleEndian(gameDataSpan[((i * 2) + 0xC21)..]);
                }

                BasicGhostNotifyState = new byte[29];
                for (int i = 0; i < 29; i++)
                {
                    BasicGhostNotifyState[i] = gameDataSpan[0xC5B + i];
                }

                BasicGhostNotifyBecauseHigherWeight = new byte[29];
                for (int i = 0; i < 29; i++)
                {
                    BasicGhostNotifyBecauseHigherWeight[i] = gameDataSpan[0xC78 + i];
                }

                AnyOptionalBooCaptured = gameDataSpan[0xC95] == 1;
                JustCollectedPolterpup = gameDataSpan[0xC96] == 1;

                GhostWeightRequirement = new short[45];
                for (int i = 0; i < 45; i++)
                {
                    GhostWeightRequirement[i] = BinaryPrimitives.ReadInt16LittleEndian(gameDataSpan[((i * 2) + 0xC97)..]);
                }

                GhostCollectableState = new byte[45];
                for (int i = 0; i < 45; i++)
                {
                    GhostCollectableState[i] = gameDataSpan[0xCF1 + i];
                }

                NumGhostCollected = new byte[45];
                for (int i = 0; i < 45; i++)
                {
                    NumGhostCollected[i] = gameDataSpan[0xD1E + i];
                }

                MaxGhostWeight = new short[45];
                for (int i = 0; i < 45; i++)
                {
                    MaxGhostWeight[i] = BinaryPrimitives.ReadInt16LittleEndian(gameDataSpan[((i * 2) + 0xD4B)..]);
                }

                GhostNotifyState = new byte[45];
                for (int i = 0; i < 45; i++)
                {
                    GhostNotifyState[i] = gameDataSpan[0xDA5 + i];
                }

                GhostNotifyBecauseHigherWeight = new byte[45];
                for (int i = 0; i < 45; i++)
                {
                    GhostNotifyBecauseHigherWeight[i] = gameDataSpan[0xDD2 + i];
                }

                GemCollected = new bool[78];
                for (int i = 0; i < 78; i++)
                {
                    GemCollected[i] = gameDataSpan[0xDFF + i] == 1;
                }

                GemNotifyState = new byte[78];
                for (int i = 0; i < 78; i++)
                {
                    GemNotifyState[i] = gameDataSpan[0xE4D + i];
                }

                HasPoltergust = gameDataSpan[0xE9B] == 1;
                SeenInitialDualScreamAnimation = gameDataSpan[0xE9C] == 1;
                HasMarioBeenRevealedInTheStory = gameDataSpan[0xE9D] == 1;
                LastMansionPlayed = gameDataSpan[0xE9E];
                TotalTreasureAcquired = BinaryPrimitives.ReadInt32LittleEndian(gameDataSpan[0xE9F..0xEA3]);
                TreasureToNotifyDuringUnloading = BinaryPrimitives.ReadInt32LittleEndian(gameDataSpan[0xEA3..0xEA7]);
                TotalGhostWeightAcquired = BinaryPrimitives.ReadInt32LittleEndian(gameDataSpan[0xEA7..0xEAB]);
                DarklightUpgradeLevel = gameDataSpan[0xEAB];
                DarklightNotifyState = gameDataSpan[0xEAC];
                PoltergustUpgradeLevel = gameDataSpan[0xEAD];
                PoltergustNotifyState = gameDataSpan[0xEAE];
                HasSuperPoltergust = gameDataSpan[0xEAF] == 1;
                SuperPoltergustNotifyState = gameDataSpan[0xEB0];
                HasSeenReviveBonePIP = gameDataSpan[0xEB1] == 1;

                BestTowerClearTime = new short[48];
                for (int i = 0; i < 48; i++)
                {
                    BestTowerClearTime[i] = BinaryPrimitives.ReadInt16LittleEndian(gameDataSpan[((i * 2) + 0xEB2)..]);
                }

                EndlessModeHighestFloorReached = BinaryPrimitives.ReadInt32LittleEndian(gameDataSpan[0xF12..0xF16]);
                AnyModeHighestFloorReached = gameDataSpan[0xF16];
                EndlessFloorsUnlocked = BinaryPrimitives.ReadInt32LittleEndian(gameDataSpan[0xF17..0xF1B]);
                RandomTowerUnlocked = gameDataSpan[0xF1B] == 1;
                TowerNotifyState = gameDataSpan[0xF1C];
            }

            public byte[] GetBytes(bool includeDataCRC = true)
            {
                byte[] gameSaveBytes = new byte[includeDataCRC ? 3869 : 3865];

                Span<byte> gameSaveSpan;
                if (includeDataCRC)
                {
                    BinaryPrimitives.WriteUInt32LittleEndian(gameSaveBytes, DataCRC);
                    gameSaveSpan = gameSaveBytes.AsSpan()[4..];
                }
                else
                {
                    gameSaveSpan = gameSaveBytes.AsSpan();
                }

                int offset = 0;

                BinaryPrimitives.WriteUInt32LittleEndian(gameSaveSpan, VersionCRC);
                offset += 4;

                foreach (byte nis in DiscoveredNIS)
                {
                    gameSaveSpan[offset++] = nis;
                }

                foreach (bool completion in MissionCompletion)
                {
                    gameSaveSpan[offset++] = (byte)(completion ? 1 : 0);
                }
                foreach (bool locked in MissionLocked)
                {
                    gameSaveSpan[offset++] = (byte)(locked ? 1 : 0);
                }
                foreach (Grade grade in MissionGrade)
                {
                    gameSaveSpan[offset++] = (byte)grade;
                }
                foreach (Grade grade in MissionPrevGrade)
                {
                    gameSaveSpan[offset++] = (byte)grade;
                }
                foreach (bool boo in MissionBooCaptured)
                {
                    gameSaveSpan[offset++] = (byte)(boo ? 1 : 0);
                }
                foreach (byte booNotify in MissionBooNotifyState)
                {
                    gameSaveSpan[offset++] = booNotify;
                }
                foreach (byte notify in MissionNotifyState)
                {
                    gameSaveSpan[offset++] = notify;
                }
                foreach (float time in MissionClearTime)
                {
                    BinaryPrimitives.WriteSingleLittleEndian(gameSaveSpan[offset..], time);
                    offset += 4;
                }
                foreach (short ghosts in MissionGhostsCaptured)
                {
                    BinaryPrimitives.WriteInt16LittleEndian(gameSaveSpan[offset..], ghosts);
                    offset += 2;
                }
                foreach (short damage in MissionDamageTaken)
                {
                    BinaryPrimitives.WriteInt16LittleEndian(gameSaveSpan[offset..], damage);
                    offset += 2;
                }
                foreach (short treasure in MissionTreasureCollected)
                {
                    BinaryPrimitives.WriteInt16LittleEndian(gameSaveSpan[offset..], treasure);
                    offset += 2;
                }

                foreach (byte collected in NumBasicGhostCollected)
                {
                    gameSaveSpan[offset++] = collected;
                }
                foreach (short weight in MaxBasicGhostWeight)
                {
                    BinaryPrimitives.WriteInt16LittleEndian(gameSaveSpan[offset..], weight);
                    offset += 2;
                }
                foreach (byte notify in BasicGhostNotifyState)
                {
                    gameSaveSpan[offset++] = notify;
                }
                foreach (byte notify in BasicGhostNotifyBecauseHigherWeight)
                {
                    gameSaveSpan[offset++] = notify;
                }

                gameSaveSpan[offset++] = (byte)(AnyOptionalBooCaptured ? 1 : 0);
                gameSaveSpan[offset++] = (byte)(JustCollectedPolterpup ? 1 : 0);

                foreach (short weight in GhostWeightRequirement)
                {
                    BinaryPrimitives.WriteInt16LittleEndian(gameSaveSpan[offset..], weight);
                    offset += 2;
                }
                foreach (byte state in GhostCollectableState)
                {
                    gameSaveSpan[offset++] = state;
                }
                foreach (byte collected in NumGhostCollected)
                {
                    gameSaveSpan[offset++] = collected;
                }
                foreach (short weight in MaxGhostWeight)
                {
                    BinaryPrimitives.WriteInt16LittleEndian(gameSaveSpan[offset..], weight);
                    offset += 2;
                }
                foreach (byte notify in GhostNotifyState)
                {
                    gameSaveSpan[offset++] = notify;
                }
                foreach (byte notify in GhostNotifyBecauseHigherWeight)
                {
                    gameSaveSpan[offset++] = notify;
                }

                foreach (bool gem in GemCollected)
                {
                    gameSaveSpan[offset++] = (byte)(gem ? 1 : 0);
                }
                foreach (byte notify in GemNotifyState)
                {
                    gameSaveSpan[offset++] = notify;
                }

                gameSaveSpan[offset++] = (byte)(HasPoltergust ? 1 : 0);
                gameSaveSpan[offset++] = (byte)(SeenInitialDualScreamAnimation ? 1 : 0);
                gameSaveSpan[offset++] = (byte)(HasMarioBeenRevealedInTheStory ? 1 : 0);

                gameSaveSpan[offset++] = LastMansionPlayed;

                BinaryPrimitives.WriteInt32LittleEndian(gameSaveSpan[offset..], TotalTreasureAcquired);
                offset += 4;
                BinaryPrimitives.WriteInt32LittleEndian(gameSaveSpan[offset..], TreasureToNotifyDuringUnloading);
                offset += 4;
                BinaryPrimitives.WriteInt32LittleEndian(gameSaveSpan[offset..], TotalGhostWeightAcquired);
                offset += 4;

                gameSaveSpan[offset++] = DarklightUpgradeLevel;
                gameSaveSpan[offset++] = DarklightNotifyState;
                gameSaveSpan[offset++] = PoltergustUpgradeLevel;
                gameSaveSpan[offset++] = PoltergustNotifyState;
                gameSaveSpan[offset++] = (byte)(HasSuperPoltergust ? 1 : 0);
                gameSaveSpan[offset++] = SuperPoltergustNotifyState;

                gameSaveSpan[offset++] = (byte)(HasSeenReviveBonePIP ? 1 : 0);

                foreach (short time in BestTowerClearTime)
                {
                    BinaryPrimitives.WriteInt16LittleEndian(gameSaveSpan[offset..], time);
                    offset += 2;
                }
                BinaryPrimitives.WriteInt32LittleEndian(gameSaveSpan[offset..], EndlessModeHighestFloorReached);
                offset += 4;
                gameSaveSpan[offset++] = AnyModeHighestFloorReached;
                BinaryPrimitives.WriteInt32LittleEndian(gameSaveSpan[offset..], EndlessFloorsUnlocked);
                offset += 4;
                gameSaveSpan[offset++] = (byte)(RandomTowerUnlocked ? 1 : 0);
                gameSaveSpan[offset++] = TowerNotifyState;

                return gameSaveBytes;
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
