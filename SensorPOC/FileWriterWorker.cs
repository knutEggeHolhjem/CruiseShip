using System.Threading.Channels;

namespace SensorPOC;

public class FileWriterWorker : BackgroundService
{
    private readonly string _baseDirectory;
    private readonly ChannelReader<SensorModel> _reader;
    private const string SensorName = "SensorPOC";
    private const int MaxEntriesPerFile = 100;

    public FileWriterWorker(ChannelReader<SensorModel> reader, IConfiguration configuration)
    {
        string? configuredPath = configuration["Data:Location"];
        _baseDirectory = configuredPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "sensor-data");

        _reader = reader;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Directory.CreateDirectory(_baseDirectory);
        await WriteFiles(stoppingToken).ConfigureAwait(false);
    }

    private async Task WriteFiles(CancellationToken stoppingToken)
    {
        int fileCounter = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            string fileName = $"{SensorName}_{DateTime.UtcNow:yyyyMMdd}_{fileCounter}.csv";
            string filePath = Path.Combine(_baseDirectory, fileName);
            await WriteFile(filePath, stoppingToken).ConfigureAwait(false);
            await WriteDoneFile(filePath).ConfigureAwait(false);

            fileCounter++;
        }
    }

    private async Task WriteFile(string filePath, CancellationToken stoppingToken)
    {
        int entryCount = 0;
        using StreamWriter writer = new(filePath, append: true);
        await writer.WriteLineAsync("TimeStamp,SensorId,SensorName,Key1,Value1,Key2,Value2").ConfigureAwait(false);

        await foreach (var data in _reader.ReadAllAsync(stoppingToken))
        {
            await writer.WriteLineAsync(data.ToCsvString()).ConfigureAwait(false);
            await writer.FlushAsync(stoppingToken).ConfigureAwait(false);
            entryCount++;

            if (entryCount >= MaxEntriesPerFile) break;
        }
    }

    private static async Task WriteDoneFile(string filePath)
    {
        string doneFilePath = filePath.Replace(".csv", ".done");
        await using (FileStream fs = File.Create(doneFilePath)){};
    }
}
