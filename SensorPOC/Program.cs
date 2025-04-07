using System.Threading.Channels;

using SensorPOC;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var channel = Channel.CreateUnbounded<SensorModel>();
builder.Services.AddHostedService(_ => new DataGeneratorWorker(channel.Writer));
builder.Services.AddHostedService(provider => new FileWriterWorker(channel.Reader, provider.GetRequiredService<IConfiguration>()));

var host = builder.Build();
host.Run();
