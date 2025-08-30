using System.IO;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace DominoNext.Services.Interfaces
{
    public interface IMidiLoader
    {
        MidiFile LoadMidi(string filePath);
    }
}
