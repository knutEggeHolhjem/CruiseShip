using System.Threading.Channels;

using SensorPOC;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var channel = Channel.CreateUnbounded<SensorModel>();
builder.Services.AddHostedService(_ => new DataGeneratorWorker(channel.Writer));
builder.Services.AddHostedService(_ => new FileWriterWorker(channel.Reader));

var host = builder.Build();
host.Run();
