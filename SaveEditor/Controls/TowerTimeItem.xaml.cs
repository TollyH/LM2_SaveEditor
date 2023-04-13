using System.Windows.Controls;
using System.Windows.Media;

namespace LM2.SaveEditor.Controls
{
    /// <summary>
    /// Interaction logic for TowerTimeItem.xaml
    /// </summary>
    public partial class TowerTimeItem : UserControl
    {
        public TowerTimeItem(string towerModeName, ushort time)
        {
            InitializeComponent();

            towerModeLabel.Text = towerModeName;
            timeBox.Text = time.ToString();
        }

        /// <remarks>
        /// If the field contains an invalid value, 0 will be returned.
        /// </remarks>
        public ushort GetTime()
        {
            return ushort.TryParse(timeBox.Text, out ushort time) ? time : (ushort)0;
        }

        public void HighlightInvalidInputs()
        {
            Brush errorBrush = Brushes.Salmon;

            if (!ushort.TryParse(timeBox.Text, out _))
            {
                timeBox.Background = errorBrush;
            }
            else
            {
                timeBox.ClearValue(BackgroundProperty);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HighlightInvalidInputs();
        }
    }
}
