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

            uint crc = CRC.CalculateChecksum(0x00000000, saveData, 0, 4);
            crc = CRC.CalculateChecksum(crc, saveData, 4, 1);
            crc = CRC.CalculateChecksum(crc, saveData, 5, 1);
            crc = CRC.CalculateChecksum(crc, saveData, 6, 1);
            crc = CRC.CalculateChecksum(crc, saveData, 7, 4);
            crc = CRC.CalculateChecksum(crc, saveData, 11, 1);
            crc = CRC.CalculateChecksum(crc, saveData, 12, 1);
            crc = CRC.CalculateChecksum(crc, saveData, 13, 1);
            crc = CRC.CalculateChecksum(crc, saveData, 14, 8);

            Assert.AreEqual(0x06EC6C0A, crc);
        }
    }
}
