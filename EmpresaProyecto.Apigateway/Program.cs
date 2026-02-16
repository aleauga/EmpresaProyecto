using Ocelot.DependencyInjection;
using Ocelot.Middleware;


var builder = WebApplication.CreateBuilder(args);

// Configurar CORS para permitir solicitudes desde el frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configuración de Ocelot según ambiente
builder.Configuration
    .AddJsonFile("ocelot.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);


builder.Services.AddOcelot(builder.Configuration);


var app = builder.Build();

// Usar CORS
app.UseCors();

await app.UseOcelot();

var listenUrl = builder.Configuration["Host:Url"] ?? "http://0.0.0.0:5000";
app.Run(listenUrl);