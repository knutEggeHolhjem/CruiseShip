using System.Collections.ObjectModel;

namespace IngesterPOC;

public record IngestResult
{
    public string? TimeStamp { get; set; }
    public string? Source { get; set; }
    public string? FileName { get; set; }
    public int? SuccessEntries { get; set; }
    public int? FailedEntries { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}