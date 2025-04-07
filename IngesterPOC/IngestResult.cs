using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace IngesterPOC;

[JsonSerializable(typeof(IngestResult))]
public record IngestResult
{
    public string? TimeStamp { get; set; }
    public string? Source { get; set; }
    public string? FileName { get; set; }
    public int? SuccessEntries { get; set; }
    public int? FailedEntries { get; set; }
    public ReadOnlyCollection<string> Errors { get; set; } = new List<string>().AsReadOnly();
}