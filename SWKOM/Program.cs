using SWKOM.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map endpoints
app.MapTest();

app.Run();
