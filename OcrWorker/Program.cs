using OcrWorker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<OcrWorker.OcrWorker>();

var host = builder.Build();
host.Run();