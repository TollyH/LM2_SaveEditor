using LM2.SaveTools;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;

namespace LM2.SaveEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SaveData? loadedSave = null;
        private string loadedSavePath = "";

        private Mansion selectedGemMansion = Mansion.GloomyManor;

        private readonly CheckBox[] gemCollectedCheckBoxes;
        private readonly CheckBox[] gemNewCheckBoxes;

        public MainWindow()
        {
            InitializeComponent();

            gemCollectedCheckBoxes = new CheckBox[13] {
                gemCollected0, gemCollected1, gemCollected2, gemCollected3, gemCollected4,
                gemCollected5, gemCollected6, gemCollected7, gemCollected8,
                gemCollected9, gemCollected10, gemCollected11, gemCollected12
            };
            gemNewCheckBoxes = new CheckBox[13] {
                gemNew0, gemNew1, gemNew2, gemNew3, gemNew4,
                gemNew5, gemNew6, gemNew7, gemNew8,
                gemNew9, gemNew10, gemNew11, gemNew12
            };
        }

        private void UpdateAllFields()
        {
            missionStack.Children.Clear();
            basicGhostStack.Children.Clear();
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

            BasicGhostInfo[] basicGhosts = loadedSave.GameSaveData.GetBasicGhostInfo();
            foreach (int index in Utils.EvershadeGhostIndicesOrder)
            {
                string ghostName = Utils.EvershadeGhostIndices[index];
                _ = basicGhostStack.Children.Add(new Controls.BasicGhostItem(ghostName, basicGhosts[index])
                {
                    Tag = index
                });
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

            UpdateGemCheckboxes();
        }

        private void UpdateGemCheckboxes()
        {
            if (loadedSave is null)
            {
                return;
            }

            GemInfo[] gemInfo = loadedSave.GameSaveData.GetGemInfo();
            for (int i = 0; i < gemCollectedCheckBoxes.Length; i++)
            {
                GemInfo gem = gemInfo[Utils.GetGemIndex(i, selectedGemMansion)];
                gemCollectedCheckBoxes[i].IsChecked = gem.Collected;
                gemNewCheckBoxes[i].IsChecked = gem.IsNew;
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
            foreach (Controls.BasicGhostItem basicGhost in basicGhostStack.Children)
            {
                loadedSave.GameSaveData.UpdateFromBasicGhostInfo((int)basicGhost.Tag, basicGhost.GetBasicGhostInfo());
            }
            foreach (Controls.GhostItem ghost in ghostStack.Children)
            {
                loadedSave.GameSaveData.UpdateFromGhostInfo((int)ghost.Tag, ghost.GetGhostInfo());
            }

            for (int i = 0; i < gemCollectedCheckBoxes.Length; i++)
            {
                loadedSave.GameSaveData.UpdateFromGemInfo(Utils.GetGemIndex(i, selectedGemMansion), new GemInfo()
                {
                    Collected = gemCollectedCheckBoxes[i].IsChecked ?? false,
                    IsNew = gemNewCheckBoxes[i].IsChecked ?? false
                });
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
            try
            {
                loadedSave = FileOperations.ReadSaveData(openDialog.FileName);
            }
            catch (InvalidChecksumException)
            {
                _ = MessageBox.Show(
                    "The provided save file has an invalid checksum. This will be automatically corrected upon saving.",
                    "Invalid Checksum", MessageBoxButton.OK, MessageBoxImage.Warning);
                loadedSave = FileOperations.ReadSaveData(openDialog.FileName, ignoreCRC: true);
            }
            catch (Exception exc)
            {
                _ = MessageBox.Show(
                    "The provided save file is invalid and could not be loaded. " +
                    "If you're using a physical 3DS system, you must have custom firmware/homebrew and a save manager such as Checkpoint installed. " +
                    "Saves copied from the \"Nintendo 3DS\" folder on the SD card will not work." +
                    $"\r\n\r\n{exc.GetType()}: {exc.Message}",
                    "Invalid Save File", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
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

        private void gemMansionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Store the gem values for the previous selected mansion in the save data so they aren't lost
            UpdateSaveData();
            selectedGemMansion = (Mansion)((ComboBoxItem)gemMansionCombo.SelectedItem).Tag;
            UpdateGemCheckboxes();
        }

        private void CompleteAllMissions_Click(object sender, RoutedEventArgs e)
        {
            foreach (Controls.MissionItem mission in missionStack.Children)
            {
                mission.ChangeLockedState(false);
                mission.ChangeCompletionState(true);
            }
        }

        private void GoldAllMissions_Click(object sender, RoutedEventArgs e)
        {
            foreach (Controls.MissionItem mission in missionStack.Children)
            {
                mission.ChangeGradeSelection(Grade.Gold);
            }
        }

        private void CatchAllMissionBoos_Click(object sender, RoutedEventArgs e)
        {
            foreach (Controls.MissionItem mission in missionStack.Children)
            {
                mission.ChangeBooState(true);
            }
        }

        private void CatchAllTowerGhosts_Click(object sender, RoutedEventArgs e)
        {
            foreach (Controls.GhostItem ghost in ghostStack.Children)
            {
                ghost.ChangeCollectedState(true);
            }
        }

        private void AllMansionGems_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox checkBox in gemCollectedCheckBoxes)
            {
                checkBox.IsChecked = true;
            }
        }
    }
}
