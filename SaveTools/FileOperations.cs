namespace LM2.SaveTools
{
    public static class FileOperations
    {
        public static SaveData ReadSaveData(string filepath, bool ignoreCRC = false)
        {
            return new SaveData(File.ReadAllBytes(filepath), ignoreCRC);
        }

        public static void WriteSaveData(string filepath, SaveData saveData)
        {
            File.WriteAllBytes(filepath, saveData.GetBytes());
        }
    }
}
