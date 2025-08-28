using DominoNext.ViewModels.Editor;

namespace DominoNext.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public string Greeting { get; } = "Welcome to Avalonia!";

        public PianoRollViewModel PianoRoll { get; } = new();
    }
}