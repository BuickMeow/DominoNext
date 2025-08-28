using DominoNext.Models.Music;

namespace DominoNext.ViewModels.Editor
{
    /// <summary>
    /// 音符时值选项
    /// </summary>
    public class NoteDurationOption
    {
        public string DisplayName { get; }
        public MusicalFraction Fraction { get; }
        public string Symbol { get; }

        public NoteDurationOption(string displayName, MusicalFraction fraction, string symbol)
        {
            DisplayName = displayName;
            Fraction = fraction;
            Symbol = symbol;
        }

        public override string ToString() => DisplayName;
    }
}