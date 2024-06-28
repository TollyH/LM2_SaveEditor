using System.Buffers.Binary;

namespace LM2.SaveTools
{
    public partial class SaveData
    {
        public class TitleScreenData
        {
            public uint DataCRC => CRC.CalculateChecksum(GetBytes(false));
            public static uint VersionCRC => 0x49270C7B;
            public Mansion FurthestClearedMansion { get; set; }
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
                    throw new InvalidSaveFormatException(
                        $"Title screen save data must be 0x1A (26) bytes long. {titleScreenSaveBytes.Length} bytes were provided.");
                }

                Span<byte> titleScreenSpan = titleScreenSaveBytes.AsSpan();

                if (!ignoreCRC)
                {
                    uint apparentDataCRC = BinaryPrimitives.ReadUInt32LittleEndian(titleScreenSpan[..4]);
                    if (apparentDataCRC != CRC.CalculateChecksum(titleScreenSpan[4..]))
                    {
                        throw new InvalidChecksumException(
                            "Given title screen save data has an invalid data checksum. Set the ignoreCRC parameter to true to ignore this in the future.");
                    }

                    uint givenVersionCRC = BinaryPrimitives.ReadUInt32LittleEndian(titleScreenSpan[4..8]);
                    if (givenVersionCRC != VersionCRC)
                    {
                        throw new InvalidChecksumException(
                            "Given title screen save data has an invalid version checksum (this should always be 0x7B, 0x0C, 0x27, 0x49). " +
                            "Set the ignoreCRC parameter to true to ignore this in the future.");
                    }
                }

                FurthestClearedMansion = (Mansion)titleScreenSpan[8];
                FurthestClearedMission = titleScreenSpan[9];
                HighestTowerFloor = titleScreenSpan[10];
                TotalTreasureAcquired = BinaryPrimitives.ReadInt32LittleEndian(titleScreenSpan[11..15]);
                BoosCaptured = titleScreenSpan[15];
                DarkMoonPieces = titleScreenSpan[16];
                EGaddMedals = titleScreenSpan[17];
                PlaytimeSeconds = BinaryPrimitives.ReadInt64LittleEndian(titleScreenSpan[18..26]);
            }

            private TitleScreenData() { }

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
                titleSaveSpan[4] = (byte)FurthestClearedMansion;
                titleSaveSpan[5] = FurthestClearedMission;
                titleSaveSpan[6] = HighestTowerFloor;
                BinaryPrimitives.WriteInt32LittleEndian(titleSaveSpan[7..11], TotalTreasureAcquired);
                titleSaveSpan[11] = BoosCaptured;
                titleSaveSpan[12] = DarkMoonPieces;
                titleSaveSpan[13] = EGaddMedals;
                BinaryPrimitives.WriteInt64LittleEndian(titleSaveSpan[14..22], PlaytimeSeconds);

                return titleSaveBytes;
            }

            public static TitleScreenData DetermineFromGameData(GameData gameData)
            {
                TitleScreenData titleScreenData = new()
                {
                    FurthestClearedMansion = Mansion.None,
                    FurthestClearedMission = 0xFF,
                    TotalTreasureAcquired = gameData.TotalTreasureAcquired,
                    BoosCaptured = (byte)gameData.MissionBooCaptured.Count(x => x),
                    HighestTowerFloor = gameData.AnyModeHighestFloorReached,
                    DarkMoonPieces = 1,
                    EGaddMedals = 0,
                    // There is no way to determine this with GameData
                    PlaytimeSeconds = 0
                };

                for (int i = 59; i >= 0; i--)
                {
                    if (!Utils.MissionIndices.ContainsKey(i))
                    {
                        continue;
                    }
                    if (gameData.MissionCompletion[i])
                    {
                        // Get just the tens digit of the mission index
                        titleScreenData.FurthestClearedMansion = (Mansion)(i / 10);
                        // Get just the rightmost digit of the mission index
                        titleScreenData.FurthestClearedMission = (byte)(i % 10);
                        break;
                    }
                }

                // E-4 "Ambush Manoeuvre" provides 10 boos
                if (gameData.MissionCompletion[43])
                {
                    titleScreenData.BoosCaptured += 9;
                }

                // Gloomy Manor Boss
                if (gameData.MissionCompletion[5])
                {
                    titleScreenData.DarkMoonPieces++;
                }
                // Haunted Towers Boss
                if (gameData.MissionCompletion[15])
                {
                    titleScreenData.DarkMoonPieces++;
                }
                // Old Clockworks Boss
                if (gameData.MissionCompletion[25])
                {
                    titleScreenData.DarkMoonPieces++;
                }
                // Secret Mine Boss
                if (gameData.MissionCompletion[35])
                {
                    titleScreenData.DarkMoonPieces++;
                }
                // Treacherous Mansion Boss
                if (gameData.MissionCompletion[46])
                {
                    titleScreenData.DarkMoonPieces++;
                }

                // King Boo
                if (gameData.MissionCompletion[50])
                {
                    titleScreenData.EGaddMedals++;
                }
                // Vault complete
                if (gameData.NumBasicGhostCollected.Count(x => x > 0) >= 27
                    && gameData.GhostCollectableState.Count(x => x == 2) >= 45
                    && gameData.GemCollected.Count(x => x) >= 65
                    && gameData.MissionBooCaptured.Count(x => x) >= 22)
                {
                    titleScreenData.EGaddMedals++;
                }
                // All missions at gold rank
                if (gameData.MissionGrade.Count(x => x == Grade.Gold) >= 34)
                {
                    titleScreenData.EGaddMedals++;
                }

                return titleScreenData;
            }
        }

        public partial class GameData
        {
            public static uint VersionCRC3DS => 0xD43203AD;
            public static uint VersionCRCSwitch => 0x3DAAEBA5;

            // Only used for Switch saves
            public static int SaveGameVersionNumber => 3;

            public byte[] DiscoveredNIS { get; private set; }

            public bool[] MissionCompletion { get; private set; }
            public bool[] MissionLocked { get; private set; }
            public Grade[] MissionGrade { get; private set; }
            public Grade[] MissionPrevGrade { get; private set; }
            public bool[] MissionBooCaptured { get; private set; }
            public byte[] MissionBooNotifyState { get; private set; }
            public byte[] MissionNotifyState { get; private set; }
            public float[] MissionClearTime { get; private set; }
            public ushort[] MissionGhostsCaptured { get; private set; }
            public ushort[] MissionDamageTaken { get; private set; }
            public ushort[] MissionTreasureCollected { get; private set; }

            public byte[] NumBasicGhostCollected { get; private set; }
            public ushort[] MaxBasicGhostWeight { get; private set; }
            public byte[] BasicGhostNotifyState { get; private set; }
            public byte[] BasicGhostNotifyBecauseHigherWeight { get; private set; }

            public bool AnyOptionalBooCaptured { get; set; }
            public bool JustCollectedPolterpup { get; set; }

            public ushort[] GhostWeightRequirement { get; private set; }
            public byte[] GhostCollectableState { get; private set; }
            public byte[] NumGhostCollected { get; private set; }
            public ushort[] MaxGhostWeight { get; private set; }
            public byte[] GhostNotifyState { get; private set; }
            public byte[] GhostNotifyBecauseHigherWeight { get; private set; }

            public bool[] GemCollected { get; private set; }
            public byte[] GemNotifyState { get; private set; }

            public bool HasPoltergust { get; set; }
            public bool SeenInitialDualScreamAnimation { get; set; }
            public bool HasMarioBeenRevealedInTheStory { get; set; }

            public Mansion LastMansionPlayed { get; set; }

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

            public ushort[] BestTowerClearTime { get; private set; }
            public byte[] EndlessModeHighestFloorReached { get; set; }
            public byte AnyModeHighestFloorReached { get; set; }
            public bool[] EndlessFloorsUnlocked { get; set; }
            public bool RandomTowerUnlocked { get; set; }
            public byte TowerNotifyState { get; set; }

            // Switch-Exclusive Fields
            public bool RumbleEnabled { get; set; }
            public bool GyroscopeEnabled { get; set; }
            public bool RStickOmnidirectional { get; set; }
            public bool XAxisInverted { get; set; }
            public bool YAxisInverted { get; set; }
            public byte GyroscopeSensitivity { get; set; }
            public byte FirstPersonStickSensitivity { get; set; }
            public byte LevelBrightness { get; set; }
            public bool HideMinimap { get; set; }


            public GameData(byte[] gameDataBytes, bool ignoreCRC = false)
            {
                bool switchVersion = false;
                uint correctVersionCRC = VersionCRC3DS;
                if (gameDataBytes.Length == 0xF2A)
                {
                    switchVersion = true;
                    correctVersionCRC = VersionCRCSwitch;
                }
                else if (gameDataBytes.Length != 0xF1D)
                {
                    throw new InvalidSaveFormatException(
                        $"Game save data must be either 0xF1D (3869) or 0xF2A (3882) bytes long. {gameDataBytes.Length} bytes were provided.");
                }

                Span<byte> gameDataSpan = gameDataBytes.AsSpan();

                if (!ignoreCRC)
                {
                    uint apparentDataCRC = BinaryPrimitives.ReadUInt32LittleEndian(gameDataSpan[..4]);
                    if (apparentDataCRC != CRC.CalculateChecksum(gameDataSpan[4..]))
                    {
                        throw new InvalidChecksumException(
                            "Given game save data has an invalid data checksum. Set the ignoreCRC parameter to true to ignore this in the future.");
                    }

                    uint givenVersionCRC = BinaryPrimitives.ReadUInt32LittleEndian(gameDataSpan[4..8]);
                    if (givenVersionCRC != correctVersionCRC)
                    {
                        string correctString = switchVersion ? "0xA5, 0xEB, 0xAA, 0x3D" : "0xAD, 0x03, 0x32, 0xD4";
                        throw new InvalidChecksumException(
                            $"Given game save data has an invalid version checksum (this should always be {correctString}). " +
                            "Set the ignoreCRC parameter to true to ignore this in the future.");
                    }
                }

                if (switchVersion)
                {
                    if (!ignoreCRC)
                    {
                        int givenVersion = BinaryPrimitives.ReadInt32LittleEndian(gameDataSpan[8..12]);
                        if (givenVersion != SaveGameVersionNumber)
                        {
                            throw new InvalidChecksumException(
                                $"Given game save data has an invalid version number (expected {SaveGameVersionNumber}, got {givenVersion}). " +
                                "Set the ignoreCRC parameter to true to ignore this in the future.");
                        }
                    }

                    // Omit first 4 bytes of span so that offsets remain consistent between 3DS and Switch saves
                    // (we don't need to read any of the first 12 bytes again now).
                    gameDataSpan = gameDataSpan[4..];
                }

                DiscoveredNIS = gameDataSpan[8..0x808].ToArray();

                MissionCompletion = new bool[60];
                for (int i = 0; i < 60; i++)
                {
                    MissionCompletion[i] = gameDataSpan[0x808 + i] != 0;
                }

                MissionLocked = new bool[60];
                for (int i = 0; i < 60; i++)
                {
                    MissionLocked[i] = gameDataSpan[0x844 + i] != 0;
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
                    MissionBooCaptured[i] = gameDataSpan[0x8F8 + i] != 0;
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

                MissionGhostsCaptured = new ushort[60];
                for (int i = 0; i < 60; i++)
                {
                    MissionGhostsCaptured[i] = BinaryPrimitives.ReadUInt16LittleEndian(gameDataSpan[((i * 2) + 0xA9C)..]);
                }

                MissionDamageTaken = new ushort[60];
                for (int i = 0; i < 60; i++)
                {
                    MissionDamageTaken[i] = BinaryPrimitives.ReadUInt16LittleEndian(gameDataSpan[((i * 2) + 0xB14)..]);
                }

                MissionTreasureCollected = new ushort[60];
                for (int i = 0; i < 60; i++)
                {
                    MissionTreasureCollected[i] = BinaryPrimitives.ReadUInt16LittleEndian(gameDataSpan[((i * 2) + 0xB8C)..]);
                }

                NumBasicGhostCollected = new byte[29];
                for (int i = 0; i < 29; i++)
                {
                    NumBasicGhostCollected[i] = gameDataSpan[0xC04 + i];
                }

                MaxBasicGhostWeight = new ushort[29];
                for (int i = 0; i < 29; i++)
                {
                    MaxBasicGhostWeight[i] = BinaryPrimitives.ReadUInt16LittleEndian(gameDataSpan[((i * 2) + 0xC21)..]);
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

                AnyOptionalBooCaptured = gameDataSpan[0xC95] != 0;
                JustCollectedPolterpup = gameDataSpan[0xC96] != 0;

                GhostWeightRequirement = new ushort[45];
                for (int i = 0; i < 45; i++)
                {
                    GhostWeightRequirement[i] = BinaryPrimitives.ReadUInt16LittleEndian(gameDataSpan[((i * 2) + 0xC97)..]);
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

                MaxGhostWeight = new ushort[45];
                for (int i = 0; i < 45; i++)
                {
                    MaxGhostWeight[i] = BinaryPrimitives.ReadUInt16LittleEndian(gameDataSpan[((i * 2) + 0xD4B)..]);
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
                    GemCollected[i] = gameDataSpan[0xDFF + i] != 0;
                }

                GemNotifyState = new byte[78];
                for (int i = 0; i < 78; i++)
                {
                    GemNotifyState[i] = gameDataSpan[0xE4D + i];
                }

                HasPoltergust = gameDataSpan[0xE9B] != 0;
                SeenInitialDualScreamAnimation = gameDataSpan[0xE9C] != 0;
                HasMarioBeenRevealedInTheStory = gameDataSpan[0xE9D] != 0;
                LastMansionPlayed = (Mansion)gameDataSpan[0xE9E];
                TotalTreasureAcquired = BinaryPrimitives.ReadInt32LittleEndian(gameDataSpan[0xE9F..0xEA3]);
                TreasureToNotifyDuringUnloading = BinaryPrimitives.ReadInt32LittleEndian(gameDataSpan[0xEA3..0xEA7]);
                TotalGhostWeightAcquired = BinaryPrimitives.ReadInt32LittleEndian(gameDataSpan[0xEA7..0xEAB]);
                DarklightUpgradeLevel = gameDataSpan[0xEAB];
                DarklightNotifyState = gameDataSpan[0xEAC];
                PoltergustUpgradeLevel = gameDataSpan[0xEAD];
                PoltergustNotifyState = gameDataSpan[0xEAE];
                HasSuperPoltergust = gameDataSpan[0xEAF] != 0;
                SuperPoltergustNotifyState = gameDataSpan[0xEB0];
                HasSeenReviveBonePIP = gameDataSpan[0xEB1] != 0;

                BestTowerClearTime = new ushort[48];
                for (int i = 0; i < 48; i++)
                {
                    BestTowerClearTime[i] = BinaryPrimitives.ReadUInt16LittleEndian(gameDataSpan[((i * 2) + 0xEB2)..]);
                }

                EndlessModeHighestFloorReached = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    EndlessModeHighestFloorReached[i] = gameDataSpan[0xF12 + i];
                }

                AnyModeHighestFloorReached = gameDataSpan[0xF16];

                EndlessFloorsUnlocked = new bool[4];
                for (int i = 0; i < 4; i++)
                {
                    EndlessFloorsUnlocked[i] = gameDataSpan[0xF17 + i] != 0;
                }

                RandomTowerUnlocked = gameDataSpan[0xF1B] != 0;
                TowerNotifyState = gameDataSpan[0xF1C];

                if (switchVersion)
                {
                    RumbleEnabled = gameDataSpan[0xF1D] != 0;
                    GyroscopeEnabled = gameDataSpan[0xF1E] != 0;
                    RStickOmnidirectional = gameDataSpan[0xF1F] != 0;
                    XAxisInverted = gameDataSpan[0xF20] != 0;
                    YAxisInverted = gameDataSpan[0xF21] != 0;
                    GyroscopeSensitivity = gameDataSpan[0xF22];
                    FirstPersonStickSensitivity = gameDataSpan[0xF23];
                    LevelBrightness = gameDataSpan[0xF24];
                    HideMinimap = gameDataSpan[0xF25] != 0;
                }
                else
                {
                    // 3DS saves don't define these values, so just initialise them to the defaults
                    RumbleEnabled = true;
                    GyroscopeEnabled = true;
                    RStickOmnidirectional = true;
                    XAxisInverted = false;
                    YAxisInverted = false;
                    GyroscopeSensitivity = 2;
                    FirstPersonStickSensitivity = 2;
                    LevelBrightness = 2;
                    HideMinimap = false;
                }
            }

            public uint DataCRC(bool switchVersion)
            {
                return CRC.CalculateChecksum(GetBytes(false, switchVersion));
            }

            public byte[] GetBytes(bool includeDataCRC = true, bool switchVersion = false)
            {
                int additionalBytes = switchVersion ? 13 : 0;
                byte[] gameSaveBytes = new byte[(includeDataCRC ? 3869 : 3865) + additionalBytes];

                Span<byte> gameSaveSpan;
                if (includeDataCRC)
                {
                    BinaryPrimitives.WriteUInt32LittleEndian(gameSaveBytes, DataCRC(switchVersion));
                    gameSaveSpan = gameSaveBytes.AsSpan()[4..];
                }
                else
                {
                    gameSaveSpan = gameSaveBytes.AsSpan();
                }

                int offset = 0;

                BinaryPrimitives.WriteUInt32LittleEndian(gameSaveSpan, switchVersion ? VersionCRCSwitch : VersionCRC3DS);
                offset += 4;

                if (switchVersion)
                {
                    BinaryPrimitives.WriteInt32LittleEndian(gameSaveSpan[offset..], SaveGameVersionNumber);
                    offset += 4;
                }

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
                foreach (ushort ghosts in MissionGhostsCaptured)
                {
                    BinaryPrimitives.WriteUInt16LittleEndian(gameSaveSpan[offset..], ghosts);
                    offset += 2;
                }
                foreach (ushort damage in MissionDamageTaken)
                {
                    BinaryPrimitives.WriteUInt16LittleEndian(gameSaveSpan[offset..], damage);
                    offset += 2;
                }
                foreach (ushort treasure in MissionTreasureCollected)
                {
                    BinaryPrimitives.WriteUInt16LittleEndian(gameSaveSpan[offset..], treasure);
                    offset += 2;
                }

                foreach (byte collected in NumBasicGhostCollected)
                {
                    gameSaveSpan[offset++] = collected;
                }
                foreach (ushort weight in MaxBasicGhostWeight)
                {
                    BinaryPrimitives.WriteUInt16LittleEndian(gameSaveSpan[offset..], weight);
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

                foreach (ushort weight in GhostWeightRequirement)
                {
                    BinaryPrimitives.WriteUInt16LittleEndian(gameSaveSpan[offset..], weight);
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
                foreach (ushort weight in MaxGhostWeight)
                {
                    BinaryPrimitives.WriteUInt16LittleEndian(gameSaveSpan[offset..], weight);
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

                gameSaveSpan[offset++] = (byte)LastMansionPlayed;

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

                foreach (ushort time in BestTowerClearTime)
                {
                    BinaryPrimitives.WriteUInt16LittleEndian(gameSaveSpan[offset..], time);
                    offset += 2;
                }
                foreach (byte highest in EndlessModeHighestFloorReached)
                {
                    gameSaveSpan[offset++] = highest;
                }
                gameSaveSpan[offset++] = AnyModeHighestFloorReached;
                foreach (bool unlocked in EndlessFloorsUnlocked)
                {
                    gameSaveSpan[offset++] = (byte)(unlocked ? 1 : 0);
                }
                gameSaveSpan[offset++] = (byte)(RandomTowerUnlocked ? 1 : 0);
                gameSaveSpan[offset++] = TowerNotifyState;

                if (switchVersion)
                {
                    gameSaveSpan[offset++] = (byte)(RumbleEnabled ? 1 : 0);
                    gameSaveSpan[offset++] = (byte)(GyroscopeEnabled ? 1 : 0);
                    gameSaveSpan[offset++] = (byte)(RStickOmnidirectional ? 1 : 0);
                    gameSaveSpan[offset++] = (byte)(XAxisInverted ? 1 : 0);
                    gameSaveSpan[offset++] = (byte)(YAxisInverted ? 1 : 0);
                    gameSaveSpan[offset++] = GyroscopeSensitivity;
                    gameSaveSpan[offset++] = FirstPersonStickSensitivity;
                    gameSaveSpan[offset++] = LevelBrightness;
                    gameSaveSpan[offset++] = (byte)(HideMinimap ? 1 : 0);
                }

                return gameSaveBytes;
            }
        }

        public TitleScreenData TitleScreenSaveData { get; private set; }
        public GameData GameSaveData { get; private set; }

        public SaveData(byte[] saveBytes, bool ignoreCRC = false)
        {
            if (saveBytes.Length is not 0xF37 and not 0xF44)
            {
                throw new InvalidSaveFormatException(
                    $"Save data must be 0xF37 (3895) or 0xF44 (3908) bytes long. {saveBytes.Length} bytes were provided.");
            }

            TitleScreenSaveData = new(saveBytes[..0x1A], ignoreCRC);
            GameSaveData = new(saveBytes[0x1A..], ignoreCRC);
        }

        public byte[] GetBytes(bool switchVersion = false)
        {
            byte[] saveData = new byte[switchVersion ? 0xF44 : 0xF37];

            Array.Copy(TitleScreenSaveData.GetBytes(), saveData, 0x1A);
            Array.Copy(GameSaveData.GetBytes(switchVersion: switchVersion), 0, saveData, 0x1A, switchVersion ? 0xF2A : 0xF1D);

            return saveData;
        }
    }

    public class InvalidChecksumException : Exception
    {
        public InvalidChecksumException(string message) : base(message) { }
    }

    public class InvalidSaveFormatException : Exception
    {
        public InvalidSaveFormatException(string message) : base(message) { }
    }
}
