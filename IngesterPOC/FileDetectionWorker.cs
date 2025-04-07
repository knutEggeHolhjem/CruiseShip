using System.Threading.Channels;

namespace IngesterPOC;

public class FileDetectionWorker : BackgroundService
{
    private readonly ChannelWriter<string> _writer;
    private readonly string _baseDirectory;

    public FileDetectionWorker(ChannelWriter<string> writer, IConfiguration configuration)
    {
        _writer = writer;
        string? configuredPath = configuration["Data:Location"];
        _baseDirectory = configuredPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "sensor-data");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using FileSystemWatcher watcher = new(_baseDirectory)
        {
            EnableRaisingEvents = true
        };

        watcher.Created += OnNewFileDetected;

        await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
    }

    private void OnNewFileDetected(object sender, FileSystemEventArgs e)
    {
        if (e.FullPath.EndsWith(".csv")) return;
        if (File.Exists(e.FullPath))
        {
            _writer.TryWrite(e.FullPath);
        }
        else
        {
            // Broken state
        }
    }
}
