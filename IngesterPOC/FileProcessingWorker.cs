using System.Text.Json;
using System.Threading.Channels;

namespace IngesterPOC;

public class FileProcessingWorker : BackgroundService
{
    private readonly ILogger<FileProcessingWorker> _logger;
    private readonly ChannelReader<string> _reader;

    public FileProcessingWorker(ILogger<FileProcessingWorker> logger, ChannelReader<string> reader)
    {
        _logger = logger;
        _reader = reader;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var doneFilePath in _reader.ReadAllAsync(stoppingToken))
        {
            string filePath = doneFilePath.Replace(".done", ".csv");
            var ingestResult = await ProcessFile(filePath, stoppingToken).ConfigureAwait(false);
            string ingestResultJson = JsonSerializer.Serialize(ingestResult);

            _logger.LogInformation("Ingest Result: {JsonString}", ingestResultJson);

            if (File.Exists(doneFilePath))
            {
                File.Delete(doneFilePath);
            }
        }
    }

    private static async Task<IngestResult> ProcessFile(string filePath, CancellationToken stoppingToken)
    {
        int successEntries = 0;
        int failedEntries = 0;
        var errors = new List<string>();
        string? source = null;
        string? timestamp = null;
        string fileName = Path.GetFileName(filePath);

        var lines = await File.ReadAllLinesAsync(filePath, stoppingToken).ConfigureAwait(false);

        foreach (var line in lines.Skip(1))
        {
            var values = line.Split(',');

            string? timeStamp = values[0];
            string? sensorId = values[1];
            string? sensorName = values[2];
            string? key1 = values[3];
            string? value1 = values[4];
            string? key2 = values[5];
            string? value2 = values[6];

            if (string.IsNullOrEmpty(source)) source = sensorName;
            if (string.IsNullOrEmpty(timestamp)) timestamp = timeStamp;

            if (string.IsNullOrEmpty(value1) || string.IsNullOrEmpty(value2))
            {
                failedEntries++;
                errors.Add($"Missing value for row: {line}");
            }
            else
            {
                successEntries++;
            }
        }

        return new IngestResult
        {
            TimeStamp = timestamp,
            Source = source,
            FileName = fileName,
            SuccessEntries = successEntries,
            FailedEntries = failedEntries,
            Errors = errors.AsReadOnly()
        };
    }
}
