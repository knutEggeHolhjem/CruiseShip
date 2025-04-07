using System.Threading.Channels;

namespace SensorPOC;

public class DataGeneratorWorker : BackgroundService
{
    private string _sensorId;
    private string _sensorName;
    private int _dataPerSecond;
    private static readonly Random Random = new();
    private readonly ChannelWriter<SensorModel> _writer;

    public DataGeneratorWorker(ChannelWriter<SensorModel> writer, IConfiguration configuration)
    {
        _sensorId = configuration["Data:SensorId"] ?? throw new ArgumentNullException("Data:SensorId is missing from configuration");
        _sensorName = configuration["Data:SensorName"] ?? throw new ArgumentNullException("Data:SensorName is missing from configuration");
        _dataPerSecond = configuration.GetValue<int>("Data:DataPerSecond");

        _writer = writer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await GenerateData(stoppingToken).ConfigureAwait(false);
    }

    public async Task GenerateData(CancellationToken stoppingToken)
    {
        using var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(_dataPerSecond));

        while (await periodicTimer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            var lat = RandomDoubleOrNull();
            var lon = RandomDoubleOrNull();

            var dataPoint = new SensorModel
            {
                TimeStamp = DateTime.UtcNow,
                SensorId = _sensorId,
                SensorName = _sensorName,
                Lat = lat,
                Lon = lon
            };

            await _writer.WriteAsync(dataPoint, stoppingToken).ConfigureAwait(false);
        }
        _writer.Complete();
    }

    private static double? RandomDoubleOrNull()
    {
        return Random.NextDouble() < 0.1 ? null : Random.NextDouble() * 100 - 100;
    }
}
