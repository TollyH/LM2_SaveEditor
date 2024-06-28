namespace LM2.SaveTools.Test
{
    [TestClass]
    public class SaveDataTests
    {
        [TestMethod]
        public void TitleDataObjectBytes()
        {
            byte[] titleData = new byte[]
            {
                0xC2, 0x24, 0xC4, 0x50, 0x7B, 0x0C, 0x27, 0x49,
                0x05, 0x00, 0x19, 0xD7, 0xE9, 0x02, 0x00, 0x20,
                0x06, 0x01, 0x3C, 0x83, 0x06, 0x00, 0x00, 0x00,
                0x00, 0x00
            };
            SaveData.TitleScreenData obj = new(titleData);

            Assert.AreEqual(0x50C424C2U, obj.DataCRC);
            Assert.AreEqual(Mansion.KingBoosIllusion, obj.FurthestClearedMansion);
            Assert.AreEqual(0, obj.FurthestClearedMission);
            Assert.AreEqual(25, obj.HighestTowerFloor);
            Assert.AreEqual(190935, obj.TotalTreasureAcquired);
            Assert.AreEqual(32, obj.BoosCaptured);
            Assert.AreEqual(6, obj.DarkMoonPieces);
            Assert.AreEqual(1, obj.EGaddMedals);
            Assert.AreEqual(426812, obj.PlaytimeSeconds);

            Assert.IsTrue(Enumerable.SequenceEqual(titleData, obj.GetBytes()));
        }

        [TestMethod]
        public void TitleDataCRCCheck()
        {
            byte[] titleData = new byte[]
            {
                0xDE, 0xAD, 0xBE, 0xEF, 0x7B, 0x0C, 0x27, 0x49,
                0x05, 0x00, 0x19, 0xD7, 0xE9, 0x02, 0x00, 0x20,
                0x06, 0x01, 0x3C, 0x83, 0x06, 0x00, 0x00, 0x00,
                0x00, 0x00
            };

            _ = Assert.ThrowsException<InvalidChecksumException>(() => new SaveData.TitleScreenData(titleData));
        }

        [TestMethod]
        public void TitleDataIgnoreCRCCheck()
        {
            byte[] titleData = new byte[]
            {
                0xDE, 0xAD, 0xBE, 0xEF, 0x7B, 0x0C, 0x27, 0x49,
                0x05, 0x00, 0x19, 0xD7, 0xE9, 0x02, 0x00, 0x20,
                0x06, 0x01, 0x3C, 0x83, 0x06, 0x00, 0x00, 0x00,
                0x00, 0x00
            };

            _ = new SaveData.TitleScreenData(titleData, ignoreCRC: true);
        }

        [TestMethod]
        public void TitleDataLenCheck()
        {
            byte[] titleData = new byte[]
            {
                0xC2, 0x24, 0xC4, 0x50, 0x7B, 0x0C, 0x27, 0x49,
                0x05, 0x00, 0x19, 0xD7, 0xE9, 0x02, 0x00, 0x20,
                0x06, 0x01, 0x3C, 0x83, 0x06, 0x00, 0x00, 0x00,
                0x00
            };

            _ = Assert.ThrowsException<InvalidSaveFormatException>(() => new SaveData.TitleScreenData(titleData));
        }

        [TestMethod]
        public void GameDataObjectBytes()
        {
            byte[] gameData = File.ReadAllBytes("TestData/SaveData_GameData.bin");

            SaveData.GameData obj = new(gameData);

            Assert.AreEqual(0xC121B670U, obj.DataCRC);

            Assert.IsTrue(obj.MissionCompletion[0]);
            Assert.IsFalse(obj.MissionCompletion[9]);
            Assert.IsTrue(obj.MissionCompletion[35]);
            Assert.IsFalse(obj.MissionCompletion[59]);

            Assert.AreEqual(Grade.Gold, obj.MissionGrade[0]);
            Assert.AreEqual(Grade.Bronze, obj.MissionGrade[9]);
            Assert.AreEqual(Grade.Silver, obj.MissionGrade[35]);
            Assert.AreEqual(Grade.Bronze, obj.MissionGrade[59]);

            Assert.AreEqual(167, obj.MissionClearTime[0]);
            Assert.AreEqual(0, obj.MissionClearTime[9]);
            Assert.AreEqual(394, obj.MissionClearTime[35]);
            Assert.AreEqual(0, obj.MissionClearTime[59]);

            Assert.AreEqual(1227, obj.MissionTreasureCollected[0]);
            Assert.AreEqual(0, obj.MissionTreasureCollected[9]);
            Assert.AreEqual(60, obj.MissionTreasureCollected[35]);
            Assert.AreEqual(0, obj.MissionTreasureCollected[59]);

            Assert.IsFalse(obj.JustCollectedPolterpup);

            Assert.IsTrue(obj.SeenInitialDualScreamAnimation);

            Assert.AreEqual(Mansion.TreacherousMansion, obj.LastMansionPlayed);

            Assert.AreEqual(2, obj.TowerNotifyState);

            Assert.IsTrue(Enumerable.SequenceEqual(gameData, obj.GetBytes()));
        }

        [TestMethod]
        public void TitleDataFromGameData()
        {
            byte[] gameData = File.ReadAllBytes("TestData/SaveData_GameData.bin");

            byte[] titleData = new byte[]
            {
                0x7B, 0x0C, 0x27, 0x49,
                0x05, 0x00, 0x19, 0xD7, 0xE9, 0x02, 0x00, 0x20,
                0x06, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00
            };

            SaveData.GameData obj = new(gameData);

            Assert.IsTrue(Enumerable.SequenceEqual(titleData, SaveData.TitleScreenData.DetermineFromGameData(obj).GetBytes(includeDataCRC: false)));
        }

        [TestMethod]
        public void GameDataCRCCheck()
        {
            byte[] gameData = File.ReadAllBytes("TestData/SaveData_GameDataBadCRC.bin");

            _ = Assert.ThrowsException<InvalidChecksumException>(() => new SaveData.GameData(gameData));
        }

        [TestMethod]
        public void GameDataIgnoreCRCCheck()
        {
            byte[] gameData = File.ReadAllBytes("TestData/SaveData_GameDataBadCRC.bin");

            _ = new SaveData.GameData(gameData, ignoreCRC: true);
        }

        [TestMethod]
        public void GameDataLenCheck()
        {
            byte[] gameData = File.ReadAllBytes("TestData/SaveData_GameDataWrongLengthShort.bin"); ;
            _ = Assert.ThrowsException<InvalidSaveFormatException>(() => new SaveData.GameData(gameData));

            gameData = File.ReadAllBytes("TestData/SaveData_GameDataWrongLengthLong.bin"); ;
            _ = Assert.ThrowsException<InvalidSaveFormatException>(() => new SaveData.GameData(gameData));
        }

        [TestMethod]
        public void FullDataObjectBytes()
        {
            byte[] saveBytes = File.ReadAllBytes("TestData/SaveData_FullData.bin");

            SaveData obj = new(saveBytes);

            Assert.IsTrue(Enumerable.SequenceEqual(saveBytes, obj.GetBytes()));
        }
    }
}
