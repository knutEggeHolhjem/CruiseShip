using System.Threading.Channels;

using SensorPOC;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var channel = Channel.CreateUnbounded<SensorModel>();
builder.Services.AddHostedService(provider => new DataGeneratorWorker(channel.Writer, provider.GetRequiredService<IConfiguration>()));
builder.Services.AddHostedService(provider => new FileWriterWorker(channel.Reader, provider.GetRequiredService<IConfiguration>()));

var host = builder.Build();
host.Run();
