using Avalonia.Controls;
using Avalonia.Interactivity;

namespace DominoNext.Views.Settings
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}