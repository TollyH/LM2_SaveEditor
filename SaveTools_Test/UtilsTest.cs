namespace LM2.SaveTools.Test
{
    [TestClass]
    public class UtilsTest
    {
        [TestMethod]
        public void TowerModeIndices()
        {
            Assert.AreEqual(4, Utils.GetTowerModeIndex(TowerMode.Hunter, TowerFloor.Ten, TowerDifficulty.Hard));
            Assert.AreEqual(20, Utils.GetTowerModeIndex(TowerMode.Rush, TowerFloor.TwentyFive, TowerDifficulty.Expert));
            Assert.AreEqual(33, Utils.GetTowerModeIndex(TowerMode.Polterpup, TowerFloor.Endless, TowerDifficulty.Normal));
            Assert.AreEqual(36, Utils.GetTowerModeIndex(TowerMode.Surprise, TowerFloor.Five, TowerDifficulty.Normal));
        }
    }
}
