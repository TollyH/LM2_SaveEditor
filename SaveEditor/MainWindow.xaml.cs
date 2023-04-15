using LM2.SaveTools;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        bool doAutoPageUpdate = false;

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

            foreach ((int index, string name) in Utils.MissionIndices)
            {
                _ = titleFurthestMissionCombo.Items.Add(new ComboBoxItem()
                {
                    Content = name,
                    Tag = index
                });
            }
        }

        private void UpdateAllFields()
        {
            missionStack.Children.Clear();
            basicGhostStack.Children.Clear();
            ghostStack.Children.Clear();
            towerTimeStack.Children.Clear();

            if (loadedSave is null)
            {
                return;
            }

            doAutoPageUpdate = false;

            MissionInfo[] missions = loadedSave.GameSaveData.GetMissionInfo();
            for (int i = 0; i < missions.Length; i++)
            {
                if (Utils.MissionIndices.TryGetValue(i, out string? missionLabel))
                {
                    Controls.MissionItem item = new(missionLabel, missions[i])
                    {
                        Tag = i
                    };
                    _ = missionStack.Children.Add(item);
                    // Whether player has poltergust or not is determined by E-1 completion
                    if (i == 0)
                    {
                        item.CompletionChecked += E1_CompletionChanged;
                        item.CompletionUnchecked += E1_CompletionChanged;
                    }
                    // Completion of E-3 "A Train to Catch" causes Mario to be revealed
                    if (i == 42)
                    {
                        item.CompletionChecked += E3_CompletionChecked;
                    }
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

            totalTreasureBox.Text = loadedSave.GameSaveData.TotalTreasureAcquired.ToString();
            totalGhostWeightBox.Text = loadedSave.GameSaveData.TotalGhostWeightAcquired.ToString();

            optionalBooCheckbox.IsChecked = loadedSave.GameSaveData.AnyOptionalBooCaptured;
            dualScreamCheckbox.IsChecked = loadedSave.GameSaveData.SeenInitialDualScreamAnimation;
            marioRevealedCheckbox.IsChecked = loadedSave.GameSaveData.HasMarioBeenRevealedInTheStory;
            lastPlayedMansionCombo.SelectedItem = lastPlayedMansionCombo.Items
                .OfType<ComboBoxItem>()
                .Where(x => (Mansion)x.Tag == loadedSave.GameSaveData.LastMansionPlayed)
                .FirstOrDefault();
            
            endlessHunterFloorBox.Text = loadedSave.GameSaveData.EndlessModeHighestFloorReached[0].ToString();
            endlessRushFloorBox.Text = loadedSave.GameSaveData.EndlessModeHighestFloorReached[1].ToString();
            endlessPolterpupFloorBox.Text = loadedSave.GameSaveData.EndlessModeHighestFloorReached[2].ToString();
            endlessSurpriseFloorBox.Text = loadedSave.GameSaveData.EndlessModeHighestFloorReached[3].ToString();
            highestFloorBox.Text = loadedSave.GameSaveData.AnyModeHighestFloorReached.ToString();

            endlessHunterUnlockedCheckbox.IsChecked = loadedSave.GameSaveData.EndlessFloorsUnlocked[0];
            endlessRushUnlockedCheckbox.IsChecked = loadedSave.GameSaveData.EndlessFloorsUnlocked[1];
            endlessPolterpupUnlockedCheckbox.IsChecked = loadedSave.GameSaveData.EndlessFloorsUnlocked[2];
            endlessSurpriseUnlockedCheckbox.IsChecked = loadedSave.GameSaveData.EndlessFloorsUnlocked[3];
            surpriseUnlockedCheckbox.IsChecked = loadedSave.GameSaveData.RandomTowerUnlocked;

            foreach (TowerMode mode in Enum.GetValues<TowerMode>())
            {
                foreach (TowerFloor floor in Enum.GetValues<TowerFloor>())
                {
                    if (floor == TowerFloor.Endless)
                    {
                        continue;
                    }
                    foreach (TowerDifficulty difficulty in Enum.GetValues<TowerDifficulty>())
                    {
                        int index = Utils.GetTowerModeIndex(mode, floor, difficulty);
                        string modeName = $"{Enum.GetName(mode)}/{Utils.TowerFloorNumerals[floor]}F/{Enum.GetName(difficulty)}";
                        Controls.TowerTimeItem towerTime = new(modeName, loadedSave.GameSaveData.BestTowerClearTime[index])
                        {
                            Tag = index
                        };
                        towerTime.TimeChanged += TowerTime_TimeChanged;
                        _ = towerTimeStack.Children.Add(towerTime);
                    }
                }
            }

            hasPoltergustCheckbox.IsChecked = loadedSave.GameSaveData.HasPoltergust;
            hasSuperPoltergustCheckbox.IsChecked = loadedSave.GameSaveData.HasSuperPoltergust;
            poltergustUpgradeSlider.Value = loadedSave.GameSaveData.PoltergustUpgradeLevel;
            darklightUpgradeSlider.Value = loadedSave.GameSaveData.DarklightUpgradeLevel;

            UpdateGemCheckboxes();

            if (loadedSave.TitleScreenSaveData.FurthestClearedMansion == Mansion.None)
            {
                titleFurthestMissionCombo.SelectedIndex = 0;
            }
            else
            {
                titleFurthestMissionCombo.SelectedItem = titleFurthestMissionCombo.Items
                    .OfType<ComboBoxItem>()
                    .Where(x => (int)x.Tag ==
                        ((int)loadedSave.TitleScreenSaveData.FurthestClearedMansion * 10)
                        + loadedSave.TitleScreenSaveData.FurthestClearedMission)
                    .FirstOrDefault();
            }
            titleHighestFloorBox.Text = loadedSave.TitleScreenSaveData.HighestTowerFloor.ToString();
            titleTreasureBox.Text = loadedSave.TitleScreenSaveData.TotalTreasureAcquired.ToString();
            titleBoosBox.Text = loadedSave.TitleScreenSaveData.BoosCaptured.ToString();
            titleDarkMoonSlider.Value = loadedSave.TitleScreenSaveData.DarkMoonPieces;
            titleEGaddMedalsSlider.Value = loadedSave.TitleScreenSaveData.EGaddMedals;
            titlePlaytimeBox.Text = loadedSave.TitleScreenSaveData.PlaytimeSeconds.ToString();

            doAutoPageUpdate = true;
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

        private void UpdateSaveData(bool titleSaveData = true)
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

            if (int.TryParse(totalTreasureBox.Text, out int treasure))
            {
                loadedSave.GameSaveData.TotalTreasureAcquired = treasure;
            }
            if (int.TryParse(totalGhostWeightBox.Text, out int ghostWeight))
            {
                loadedSave.GameSaveData.TotalGhostWeightAcquired = ghostWeight;
            }

            loadedSave.GameSaveData.AnyOptionalBooCaptured = optionalBooCheckbox.IsChecked ?? false;
            loadedSave.GameSaveData.SeenInitialDualScreamAnimation = dualScreamCheckbox.IsChecked ?? false;
            loadedSave.GameSaveData.HasMarioBeenRevealedInTheStory = marioRevealedCheckbox.IsChecked ?? false;
            loadedSave.GameSaveData.LastMansionPlayed = (Mansion)((ComboBoxItem)lastPlayedMansionCombo.SelectedItem).Tag;

            if (byte.TryParse(endlessHunterFloorBox.Text, out byte hunterFloor))
            {
                loadedSave.GameSaveData.EndlessModeHighestFloorReached[0] = hunterFloor;
            }
            if (byte.TryParse(endlessRushFloorBox.Text, out byte rushFloor))
            {
                loadedSave.GameSaveData.EndlessModeHighestFloorReached[1] = rushFloor;
            }
            if (byte.TryParse(endlessPolterpupFloorBox.Text, out byte polterpupFloor))
            {
                loadedSave.GameSaveData.EndlessModeHighestFloorReached[2] = polterpupFloor;
            }
            if (byte.TryParse(endlessSurpriseFloorBox.Text, out byte surpriseFloor))
            {
                loadedSave.GameSaveData.EndlessModeHighestFloorReached[3] = surpriseFloor;
            }
            if (byte.TryParse(highestFloorBox.Text, out byte highestFloor))
            {
                loadedSave.GameSaveData.AnyModeHighestFloorReached = highestFloor;
            }

            loadedSave.GameSaveData.EndlessFloorsUnlocked[0] = endlessHunterUnlockedCheckbox.IsChecked ?? false;
            loadedSave.GameSaveData.EndlessFloorsUnlocked[1] = endlessRushUnlockedCheckbox.IsChecked ?? false;
            loadedSave.GameSaveData.EndlessFloorsUnlocked[2] = endlessPolterpupUnlockedCheckbox.IsChecked ?? false;
            loadedSave.GameSaveData.EndlessFloorsUnlocked[3] = endlessSurpriseUnlockedCheckbox.IsChecked ?? false;
            loadedSave.GameSaveData.RandomTowerUnlocked = surpriseUnlockedCheckbox.IsChecked ?? false;

            foreach (Controls.TowerTimeItem towerTime in towerTimeStack.Children)
            {
                loadedSave.GameSaveData.BestTowerClearTime[(int)towerTime.Tag] = towerTime.GetTime();
            }

            loadedSave.GameSaveData.HasPoltergust = hasPoltergustCheckbox.IsChecked ?? false;
            loadedSave.GameSaveData.HasSuperPoltergust = hasSuperPoltergustCheckbox.IsChecked ?? false;
            loadedSave.GameSaveData.PoltergustUpgradeLevel = (byte)poltergustUpgradeSlider.Value;
            loadedSave.GameSaveData.DarklightUpgradeLevel = (byte)darklightUpgradeSlider.Value;

            if (titleSaveData)
            {
                UpdateTitlePage();

                if (titleFurthestMissionCombo.SelectedIndex == 0)
                {
                    loadedSave.TitleScreenSaveData.FurthestClearedMansion = Mansion.None;
                    loadedSave.TitleScreenSaveData.FurthestClearedMission = 0xFF;
                }
                else
                {
                    ComboBoxItem selectedItem = (ComboBoxItem)titleFurthestMissionCombo.SelectedItem;
                    loadedSave.TitleScreenSaveData.FurthestClearedMansion = (Mansion)((int)selectedItem.Tag / 10);
                    loadedSave.TitleScreenSaveData.FurthestClearedMission = (byte)((int)selectedItem.Tag % 10);
                }

                if (byte.TryParse(titleHighestFloorBox.Text, out byte titleHighestFloor))
                {
                    loadedSave.TitleScreenSaveData.HighestTowerFloor = titleHighestFloor;
                }

                if (int.TryParse(titleTreasureBox.Text, out int totalTreasure))
                {
                    loadedSave.TitleScreenSaveData.TotalTreasureAcquired = totalTreasure;
                }

                if (byte.TryParse(titleBoosBox.Text, out byte boosCaptured))
                {
                    loadedSave.TitleScreenSaveData.BoosCaptured = boosCaptured;
                }

                loadedSave.TitleScreenSaveData.DarkMoonPieces = (byte)titleDarkMoonSlider.Value;
                loadedSave.TitleScreenSaveData.EGaddMedals = (byte)titleEGaddMedalsSlider.Value;

                if (long.TryParse(titlePlaytimeBox.Text, out long playtimeSeconds))
                {
                    loadedSave.TitleScreenSaveData.PlaytimeSeconds = playtimeSeconds;
                }
            }
        }

        public void HighlightInvalidInputs()
        {
            Brush errorBrush = Brushes.Salmon;

            if (!int.TryParse(totalTreasureBox.Text, out _))
            {
                totalTreasureBox.Background = errorBrush;
            }
            else
            {
                totalTreasureBox.ClearValue(BackgroundProperty);
            }

            if (!int.TryParse(totalGhostWeightBox.Text, out _))
            {
                totalGhostWeightBox.Background = errorBrush;
            }
            else
            {
                totalGhostWeightBox.ClearValue(BackgroundProperty);
            }

            if (!byte.TryParse(endlessHunterFloorBox.Text, out _))
            {
                endlessHunterFloorBox.Background = errorBrush;
            }
            else
            {
                endlessHunterFloorBox.ClearValue(BackgroundProperty);
            }
            if (!byte.TryParse(endlessRushFloorBox.Text, out _))
            {
                endlessRushFloorBox.Background = errorBrush;
            }
            else
            {
                endlessRushFloorBox.ClearValue(BackgroundProperty);
            }
            if (!byte.TryParse(endlessPolterpupFloorBox.Text, out _))
            {
                endlessPolterpupFloorBox.Background = errorBrush;
            }
            else
            {
                endlessPolterpupFloorBox.ClearValue(BackgroundProperty);
            }
            if (!byte.TryParse(endlessSurpriseFloorBox.Text, out _))
            {
                endlessSurpriseFloorBox.Background = errorBrush;
            }
            else
            {
                endlessSurpriseFloorBox.ClearValue(BackgroundProperty);
            }
            if (!byte.TryParse(highestFloorBox.Text, out _))
            {
                highestFloorBox.Background = errorBrush;
            }
            else
            {
                highestFloorBox.ClearValue(BackgroundProperty);
            }

            if (!byte.TryParse(titleHighestFloorBox.Text, out _))
            {
                titleHighestFloorBox.Background = errorBrush;
            }
            else
            {
                titleHighestFloorBox.ClearValue(BackgroundProperty);
            }
            if (!int.TryParse(titleTreasureBox.Text, out _))
            {
                titleTreasureBox.Background = errorBrush;
            }
            else
            {
                titleTreasureBox.ClearValue(BackgroundProperty);
            }
            if (!byte.TryParse(titleBoosBox.Text, out _))
            {
                titleBoosBox.Background = errorBrush;
            }
            else
            {
                titleBoosBox.ClearValue(BackgroundProperty);
            }
            if (!long.TryParse(titlePlaytimeBox.Text, out _))
            {
                titlePlaytimeBox.Background = errorBrush;
            }
            else
            {
                titlePlaytimeBox.ClearValue(BackgroundProperty);
            }
        }

        private void UpdateAdvancedPage()
        {
            if (loadedSave is null || !doAutoPageUpdate)
            {
                return;
            }

            if (!(advancedAutoUpdateCheckbox.IsChecked ?? false))
            {
                UpdateSaveData();
                hasPoltergustCheckbox.IsChecked = loadedSave.GameSaveData.ShouldLuigiHavePoltergust();
                hasSuperPoltergustCheckbox.IsChecked = loadedSave.GameSaveData.ShouldSuperPoltergustBeUnlocked();

                poltergustUpgradeSlider.Value = loadedSave.GameSaveData.IntendedPoltergustUpgradeLevel();
                darklightUpgradeSlider.Value = loadedSave.GameSaveData.IntendedDarklightUpgradeLevel();
            }
        }

        private void UpdateTitlePage()
        {
            if (loadedSave is null || !doAutoPageUpdate)
            {
                return;
            }

            if (!(titleAutoUpdateCheckbox.IsChecked ?? false))
            {
                UpdateSaveData(titleSaveData: false);
                SaveData.TitleScreenData calculatedTitleData = SaveData.TitleScreenData.DetermineFromGameData(loadedSave.GameSaveData);
                if (calculatedTitleData.FurthestClearedMansion == Mansion.None)
                {
                    titleFurthestMissionCombo.SelectedIndex = 0;
                }
                else
                {
                    titleFurthestMissionCombo.SelectedItem = titleFurthestMissionCombo.Items
                        .OfType<ComboBoxItem>()
                        .Where(x => (int)x.Tag ==
                            ((int)calculatedTitleData.FurthestClearedMansion * 10)
                            + calculatedTitleData.FurthestClearedMission)
                        .FirstOrDefault();
                }
                titleHighestFloorBox.Text = calculatedTitleData.HighestTowerFloor.ToString();
                titleTreasureBox.Text = calculatedTitleData.TotalTreasureAcquired.ToString();
                titleBoosBox.Text = calculatedTitleData.BoosCaptured.ToString();
                titleDarkMoonSlider.Value = calculatedTitleData.DarkMoonPieces;
                titleEGaddMedalsSlider.Value = calculatedTitleData.EGaddMedals;
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
                    "If you're using a physical 3DS system, you must have custom firmware and/or homebrew installed, as well as a save manager such as Checkpoint. " +
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender == totalTreasureBox)
            {
                UpdateAdvancedPage();
            }
            HighlightInvalidInputs();
        }

        private void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WebsiteRun_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _ = Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/TollyH/LM2_SaveEditor",
                UseShellExecute = true
            });
        }

        private void E3_CompletionChecked()
        {
            marioRevealedCheckbox.IsChecked = true;
        }

        private void E1_CompletionChanged()
        {
            UpdateAdvancedPage();
        }

        private void TowerTime_TimeChanged()
        {
            if (loadedSave is null)
            {
                return;
            }
            UpdateSaveData();
            if (loadedSave.GameSaveData.ShouldRandomTowerBeUnlocked())
            {
                surpriseUnlockedCheckbox.IsChecked = true;
            }
            bool[] unlockStates = loadedSave.GameSaveData.IntendedEndlessTowerUnlockStates();
            if (unlockStates[0])
            {
                endlessHunterUnlockedCheckbox.IsChecked = true;
            }
            if (unlockStates[1])
            {
                endlessRushUnlockedCheckbox.IsChecked = true;
            }
            if (unlockStates[2])
            {
                endlessPolterpupUnlockedCheckbox.IsChecked = true;
            }
            if (unlockStates[3])
            {
                endlessSurpriseUnlockedCheckbox.IsChecked = true;
            }
        }

        private void mainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTitlePage();
        }
    }
}
