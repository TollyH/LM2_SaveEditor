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
        }

        private void SaveItem_Click(object sender, RoutedEventArgs e)
        {
            if (loadedSave is null)
            {
                return;
            }
            FileOperations.WriteSaveData(loadedSavePath, loadedSave);
        }

        private void SaveAsItem_Click(object sender, RoutedEventArgs e)
        {
            if (loadedSave is null)
            {
                return;
            }
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
