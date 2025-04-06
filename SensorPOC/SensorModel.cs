namespace SensorPOC;

public record SensorModel
{
    public DateTime TimeStamp { get; init; }
    public required string SensorId { get; init; }
    public required string SensorName { get; init; }
    public double? Lat { get; init; }
    public double? Lon { get; init; }

    public override string ToString()
    {
        return $"{TimeStamp:o},{SensorId},{SensorName},{"Lat"},{Lat?.ToString("F2") ?? ""},{"Lon"},{Lon?.ToString("F2") ?? ""}";
    }
}
