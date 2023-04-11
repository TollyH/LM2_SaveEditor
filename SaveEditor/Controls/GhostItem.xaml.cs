using LM2.SaveTools;
using System.Windows.Controls;
using System.Windows.Media;

namespace LM2.SaveEditor.Controls
{
    /// <summary>
    /// Interaction logic for GhostItem.xaml
    /// </summary>
    public partial class GhostItem : UserControl
    {
        public GhostItem(string ghostName, GhostInfo ghostInfo)
        {
            InitializeComponent();

            ghostNameLabel.Text = ghostName;
            collectedCheckbox.IsChecked = ghostInfo.Collected;
            newCheckbox.IsChecked = ghostInfo.IsNew;
            ghostsCapturedBox.Text = ghostInfo.NumCollected.ToString();
            maxWeightBox.Text = ghostInfo.MaxWeight.ToString();
        }

        /// <summary>
        /// Gets the GhostInfo state representing the user inputted values.
        /// </summary>
        /// <remarks>
        /// Fields containing invalid data will be set to 0.
        /// </remarks>
        public GhostInfo GetGhostInfo()
        {
            GhostInfo ghostInfo = new()
            {
                Collected = collectedCheckbox.IsChecked ?? false,
                IsNew = newCheckbox.IsChecked ?? false
            };

            if (byte.TryParse(ghostsCapturedBox.Text, out byte ghostsCaptured))
            {
                ghostInfo.NumCollected = ghostsCaptured;
            }

            if (ushort.TryParse(maxWeightBox.Text, out ushort damageTaken))
            {
                ghostInfo.MaxWeight = damageTaken;
            }

            return ghostInfo;
        }

        public void HighlightInvalidInputs()
        {
            Brush errorBrush = Brushes.Salmon;

            if (!byte.TryParse(ghostsCapturedBox.Text, out _))
            {
                ghostsCapturedBox.Background = errorBrush;
            }
            else
            {
                ghostsCapturedBox.ClearValue(BackgroundProperty);
            }

            if (!ushort.TryParse(maxWeightBox.Text, out _))
            {
                maxWeightBox.Background = errorBrush;
            }
            else
            {
                maxWeightBox.ClearValue(BackgroundProperty);
            }
        }

        public void ChangeCollectedState(bool collected)
        {
            collectedCheckbox.IsChecked = collected;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HighlightInvalidInputs();
        }
    }
}
