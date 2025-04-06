using System.Threading.Channels;

namespace SensorPOC;

public class FileWriterWorker(ChannelReader<SensorModel> reader) : BackgroundService
{
    private const string SensorName = "SensorPOC";
    private const int MaxEntriesPerFile = 100;
    private static readonly string BaseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "sensor-data");

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Directory.CreateDirectory(BaseDirectory);
        await WriteFiles(stoppingToken).ConfigureAwait(false);
    }

    private async Task WriteFiles(CancellationToken stoppingToken)
    {
        int fileCounter = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            string fileName = $"{SensorName}_{DateTime.UtcNow:yyyyMMdd}_{fileCounter}.csv";
            string filePath = Path.Combine(BaseDirectory, fileName);
            await WriteFile(filePath, stoppingToken).ConfigureAwait(false);
            fileCounter++;
        }
    }

    private async Task WriteFile(string filePath, CancellationToken stoppingToken)
    {
        int entryCount = 0;
        using StreamWriter writer = new(filePath, append: true);
        await writer.WriteLineAsync("TimeStamp,SensorId,SensorName,Key1,Value1,Key2,Value2").ConfigureAwait(false);

        await foreach (var data in reader.ReadAllAsync(stoppingToken))
        {
            await writer.WriteLineAsync(data.ToString()).ConfigureAwait(false);
            await writer.FlushAsync(stoppingToken).ConfigureAwait(false);
            entryCount++;

            if (entryCount >= MaxEntriesPerFile) break;
        }
    }
}
