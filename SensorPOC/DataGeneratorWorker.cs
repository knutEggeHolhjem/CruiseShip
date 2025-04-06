using System.Threading.Channels;

namespace SensorPOC;

public class DataGeneratorWorker(ChannelWriter<SensorModel> writer) : BackgroundService
{
    private const string SensorId = "100";
    private const string SensorName = "SensorPOC";
    private static readonly Random Random = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await GenerateData(stoppingToken).ConfigureAwait(false);
    }

    public async Task GenerateData(CancellationToken stoppingToken)
    {
        using var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        while (await periodicTimer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            var lat = RandomDoubleOrNull();
            var lon = RandomDoubleOrNull();

            var dataPoint = new SensorModel
            {
                TimeStamp = DateTime.UtcNow,
                SensorId = SensorId,
                SensorName = SensorName,
                Lat = lat,
                Lon = lon
            };

            _ = writer.TryWrite(dataPoint);
        }
        writer.Complete();
    }

    private static double? RandomDoubleOrNull()
    {
        return Random.NextDouble() < 0.1 ? null : Random.NextDouble() * 180 - 90;
    }
}
