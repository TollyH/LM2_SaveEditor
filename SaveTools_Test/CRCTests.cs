namespace LM2.SaveTools.Test
{
    [TestClass]
    public class CRCTests
    {
        [TestMethod]
        public void TitleScreen()
        {
            byte[] saveData = new byte[]
            {
                0x7B, 0x0C, 0x27, 0x49, 0x05, 0x00, 0x19, 0xD7,
                0xE9, 0x02, 0x00, 0x20, 0x06, 0x01, 0x04, 0x83,
                0x06, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            uint crc = CRC.CalculateChecksum(saveData);

            Assert.AreEqual(0x06EC6C0AU, crc);
        }

        [TestMethod]
        public void GameData()
        {
            byte[] saveData = File.ReadAllBytes("TestData/CRC_GameData.bin");

            uint crc = CRC.CalculateChecksum(saveData);

            Assert.AreEqual(0x372D116DU, crc);
        }
    }
}
