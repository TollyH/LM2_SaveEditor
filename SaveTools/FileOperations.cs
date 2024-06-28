namespace LM2.SaveTools
{
    public static class FileOperations
    {
        public static SaveData ReadSaveData(string filepath, bool ignoreCRC = false)
        {
            return new SaveData(File.ReadAllBytes(filepath), ignoreCRC);
        }

        public static void WriteSaveData(string filepath, SaveData saveData, bool switchVersion)
        {
            File.WriteAllBytes(filepath, saveData.GetBytes(switchVersion));
        }
    }
}
