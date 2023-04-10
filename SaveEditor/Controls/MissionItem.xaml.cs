using LM2.SaveTools;
using System.Windows.Controls;
using System.Windows.Media;

namespace LM2.SaveEditor.Controls
{
    /// <summary>
    /// Interaction logic for MissionItem.xaml
    /// </summary>
    public partial class MissionItem : UserControl
    {
        public MissionItem(string missionLabel, MissionInfo missionInfo)
        {
            InitializeComponent();

            missionNumber.Text = missionLabel;
            completeCheckbox.IsChecked = missionInfo.Completed;
            lockedCheckbox.IsChecked = missionInfo.Locked;
            booCheckbox.IsChecked = missionInfo.BooCaptured;
            fastestTimeBox.Text = missionInfo.FastestTime.ToString();
            ghostsCapturedBox.Text = missionInfo.GhostsCaptured.ToString();
            damageTakenBox.Text = missionInfo.DamageTaken.ToString();
            treasureCollectedBox.Text = missionInfo.TreasureCollected.ToString();
            gradeCombo.SelectedIndex = (int)missionInfo.Grade;
        }

        /// <summary>
        /// Gets the MissionInfo state representing the user inputted values.
        /// </summary>
        /// <remarks>
        /// Fields containing invalid data will be set to 0.
        /// </remarks>
        public MissionInfo GetMissionInfo()
        {
            MissionInfo missionInfo = new()
            {
                Completed = completeCheckbox.IsChecked ?? false,
                Locked = lockedCheckbox.IsChecked ?? false,
                BooCaptured = booCheckbox.IsChecked ?? false
            };

            if (float.TryParse(fastestTimeBox.Text, out float fastestTime))
            {
                missionInfo.FastestTime = fastestTime;
            }

            if (ushort.TryParse(ghostsCapturedBox.Text, out ushort ghostsCaptured))
            {
                missionInfo.GhostsCaptured = ghostsCaptured;
            }

            if (ushort.TryParse(damageTakenBox.Text, out ushort damageTaken))
            {
                missionInfo.DamageTaken = damageTaken;
            }

            if (ushort.TryParse(treasureCollectedBox.Text, out ushort treasureCollected))
            {
                missionInfo.TreasureCollected = treasureCollected;
            }

            missionInfo.Grade = (Grade)gradeCombo.SelectedIndex;

            return missionInfo;
        }

        public void HighlightInvalidInputs()
        {
            Brush errorBrush = Brushes.Salmon;

            if (!float.TryParse(fastestTimeBox.Text, out _))
            {
                fastestTimeBox.Background = errorBrush;
            }
            else
            {
                fastestTimeBox.ClearValue(BackgroundProperty);
            }

            if (!ushort.TryParse(ghostsCapturedBox.Text, out _))
            {
                ghostsCapturedBox.Background = errorBrush;
            }
            else
            {
                ghostsCapturedBox.ClearValue(BackgroundProperty);
            }

            if (!ushort.TryParse(damageTakenBox.Text, out _))
            {
                damageTakenBox.Background = errorBrush;
            }
            else
            {
                damageTakenBox.ClearValue(BackgroundProperty);
            }

            if (!ushort.TryParse(treasureCollectedBox.Text, out _))
            {
                treasureCollectedBox.Background = errorBrush;
            }
            else
            {
                treasureCollectedBox.ClearValue(BackgroundProperty);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HighlightInvalidInputs();
        }
    }
}
