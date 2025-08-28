using DominoNext.Models.Music;

namespace DominoNext.ViewModels.Editor
{
    /// <summary>
    /// ����ʱֵѡ��
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