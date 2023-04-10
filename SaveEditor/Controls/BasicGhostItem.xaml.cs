using LM2.SaveTools;
using System.Windows.Controls;
using System.Windows.Media;

namespace LM2.SaveEditor.Controls
{
    /// <summary>
    /// Interaction logic for BasicGhostItem.xaml
    /// </summary>
    public partial class BasicGhostItem : UserControl
    {
        public BasicGhostItem(string ghostName, BasicGhostInfo BasicGhostInfo)
        {
            InitializeComponent();

            ghostNameLabel.Text = ghostName;
            newCheckbox.IsChecked = BasicGhostInfo.IsNew;
            ghostsCapturedBox.Text = BasicGhostInfo.NumCollected.ToString();
            maxWeightBox.Text = BasicGhostInfo.MaxWeight.ToString();
        }

        /// <summary>
        /// Gets the BasicGhostInfo state representing the user inputted values.
        /// </summary>
        /// <remarks>
        /// Fields containing invalid data will be set to 0.
        /// </remarks>
        public BasicGhostInfo GetBasicGhostInfo()
        {
            BasicGhostInfo BasicGhostInfo = new()
            {
                IsNew = newCheckbox.IsChecked ?? false
            };

            if (byte.TryParse(ghostsCapturedBox.Text, out byte ghostsCaptured))
            {
                BasicGhostInfo.NumCollected = ghostsCaptured;
            }

            if (ushort.TryParse(maxWeightBox.Text, out ushort damageTaken))
            {
                BasicGhostInfo.MaxWeight = damageTaken;
            }

            return BasicGhostInfo;
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HighlightInvalidInputs();
        }
    }
}
