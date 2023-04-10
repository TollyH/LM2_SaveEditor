using LM2.SaveTools;
using Microsoft.Win32;
using System.Windows;

namespace LM2.SaveEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SaveData? loadedSave = null;
        private string loadedSavePath = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void UpdateAllFields()
        {
            missionStack.Children.Clear();
            ghostStack.Children.Clear();

            if (loadedSave is null)
            {
                return;
            }

            MissionInfo[] missions = loadedSave.GameSaveData.GetMissionInfo();
            for (int i = 0; i < missions.Length; i++)
            {
                if (Utils.MissionIndices.TryGetValue(i, out string? missionLabel))
                {
                    _ = missionStack.Children.Add(new Controls.MissionItem(missionLabel, missions[i])
                    {
                        Tag = i
                    });
                }
            }

            GhostInfo[] ghosts = loadedSave.GameSaveData.GetGhostInfo();
            foreach (int index in Utils.TowerGhostIndicesOrder)
            {
                string ghostName = Utils.TowerGhostIndices[index];
                _ = ghostStack.Children.Add(new Controls.GhostItem(ghostName, ghosts[index])
                {
                    Tag = index
                });
            }
        }

        private void UpdateSaveData()
        {
            if (loadedSave is null)
            {
                return;
            }

            foreach (Controls.MissionItem mission in missionStack.Children)
            {
                loadedSave.GameSaveData.UpdateFromMissionInfo((int)mission.Tag, mission.GetMissionInfo());
            }
            foreach (Controls.GhostItem ghost in ghostStack.Children)
            {
                loadedSave.GameSaveData.UpdateFromGhostInfo((int)ghost.Tag, ghost.GetGhostInfo());
            }
        }

        private void LoadItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new()
            {
                Filter = "Save File|*.sav",
                CheckPathExists = true,
                CheckFileExists = true
            };
            if (!openDialog.ShowDialog() ?? true)
            {
                return;
            }
            loadedSave = FileOperations.ReadSaveData(openDialog.FileName);
            loadedSavePath = openDialog.FileName;
            mainTabControl.Visibility = Visibility.Visible;
            UpdateAllFields();
        }

        private void SaveItem_Click(object sender, RoutedEventArgs e)
        {
            if (loadedSave is null)
            {
                return;
            }
            UpdateSaveData();
            FileOperations.WriteSaveData(loadedSavePath, loadedSave);
        }

        private void SaveAsItem_Click(object sender, RoutedEventArgs e)
        {
            if (loadedSave is null)
            {
                return;
            }
            UpdateSaveData();
            SaveFileDialog saveDialog = new()
            {
                AddExtension = true,
                DefaultExt = ".sav",
                Filter = "Save File|*.sav",
                CheckPathExists = true,
                FileName = loadedSavePath
            };
            if (!saveDialog.ShowDialog() ?? true)
            {
                return;
            }
            FileOperations.WriteSaveData(saveDialog.FileName, loadedSave);
        }

        private void CloseItem_Click(object sender, RoutedEventArgs e)
        {
            loadedSave = null;
            loadedSavePath = "";
            mainTabControl.Visibility = Visibility.Collapsed;
        }
    }
}
