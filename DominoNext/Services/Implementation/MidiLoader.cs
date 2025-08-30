using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Common;
using DominoNext.Services.Interfaces;

namespace DominoNext.Services.Implementation
{
    public class MidiLoader : IMidiLoader
    {
        private static IEnumerable<Encoding> GetCandidateEncodings()
        {
            var encodings = new List<Encoding>();
            void TryAddEncoding(Func<Encoding> factory)
            {
                try { encodings.Add(factory()); } catch { }
            }

            TryAddEncoding(() => Encoding.UTF8);
            TryAddEncoding(() => Encoding.Unicode);
            TryAddEncoding(() => Encoding.BigEndianUnicode);
            TryAddEncoding(() => Encoding.GetEncoding("GB18030"));
            TryAddEncoding(() => Encoding.GetEncoding(936));
            TryAddEncoding(() => Encoding.GetEncoding(932));
            TryAddEncoding(() => Encoding.Default);
            TryAddEncoding(() => Encoding.Latin1);

            return encodings;
        }

        public MidiFile LoadMidi(string filePath)
        {
            MidiFile? best = null;
            int bestScore = int.MinValue;

            foreach (var enc in GetCandidateEncodings())
            {
                try
                {
                    var settings = new ReadingSettings { TextEncoding = enc };
                    var midi = MidiFile.Read(filePath, settings);

                    int score = 0;
                    foreach (var tc in midi.GetTrackChunks())
                    {
                        var nameEvent = tc.Events.OfType<SequenceTrackNameEvent>().FirstOrDefault();
                        if (nameEvent != null && !string.IsNullOrEmpty(nameEvent.Text))
                        {
                            foreach (var ch in nameEvent.Text)
                            {
                                if (ch == '\uFFFD')
                                    score -= 2;
                                else if (!char.IsControl(ch))
                                    score += (ch > 127) ? 2 : 1;
                                else
                                    score -= 1;
                            }
                        }
                    }

                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = midi;
                        if (bestScore > 5)
                            break;
                    }
                }
                catch
                {
                    // ignore and try next encoding
                }
            }

            if (best == null)
            {
                best = MidiFile.Read(filePath);
            }

            return best;
        }
    }
}
