using DominoNext.Models.Music;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DominoNext.Services.Interfaces
{
    public interface IProjectStorageService
    {
        Task<bool> SaveProjectAsync(string filePath, IEnumerable<Note> notes, ProjectMetadata metadata);
        Task<(IEnumerable<Note> notes, ProjectMetadata metadata)> LoadProjectAsync(string filePath);
        Task<bool> ExportMidiAsync(string filePath, IEnumerable<Note> notes);
        Task<IEnumerable<Note>> ImportMidiAsync(string filePath);
    }

    public class ProjectMetadata
    {
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";
        public int BeatsPerMeasure { get; set; } = 4;
        public int TicksPerBeat { get; set; } = 96;
        public double Tempo { get; set; } = 120.0;
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;
    }
}