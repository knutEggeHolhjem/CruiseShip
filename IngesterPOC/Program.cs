using System.Threading.Channels;

using IngesterPOC;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var channel = Channel.CreateUnbounded<string>();
builder.Services.AddHostedService(provider => new FileDetectionWorker(channel.Writer, provider.GetRequiredService<IConfiguration>()));
builder.Services.AddHostedService(provider => new FileProcessingWorker(provider.GetRequiredService<ILogger<FileProcessingWorker>>(), channel.Reader));

var host = builder.Build();
host.Run();
