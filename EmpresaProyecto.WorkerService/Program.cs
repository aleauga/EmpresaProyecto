using EmpresaProyecto.Core.Messaging.Contracts;
using EmpresaProyecto.Core.Models;
using EmpresaProyecto.Core.Repository.Contracts;
using EmpresaProyecto.Core.Rest.Contracts;
using EmpresaProyecto.Infrastructure.Communication;
using EmpresaProyecto.Infrastructure.Messaging;
using EmpresaProyecto.Infrastructure.Persistence.Context;
using EmpresaProyecto.Infrastructure.Persistence.Repository.Implementations;
using EmpresaProyecto.Infrastructure.Rest;
using EmpresaProyecto.WorkerService;
using EmpresaProyecto.WorkerService.Services.Contracts;
using EmpresaProyecto.WorkerService.Services.Implementations;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

// Registramos HttpClient normal, sin AddResilienceHandler
var paymentGatewayBaseUrl = builder.Configuration["PaymentGateway:BaseUrl"] ?? "http://httpbin:80/";
builder.Services.AddHttpClient<IPaymentGateway, PaymentGatewayClient>(client =>
{
    client.BaseAddress = new Uri(paymentGatewayBaseUrl);
});

// Creamos el pipeline de resiliencia manualmente
var retry = new RetryStrategyOptions
{
    MaxRetryAttempts = 3,
    DelayGenerator = args =>
    {
        // args.AttemptNumber empieza en 1
        var delay = TimeSpan.FromSeconds(Math.Pow(2, args.AttemptNumber));
        return new ValueTask<TimeSpan?>(delay);
    },
    OnRetry = args =>
    {
        Console.WriteLine($"Reintento {args.AttemptNumber} después de {args.RetryDelay.TotalSeconds} segundos");
        return default;
    }
};


var circuitBreaker = new CircuitBreakerStrategyOptions
{
    FailureRatio = 0.5,
    MinimumThroughput = 2,
    BreakDuration = TimeSpan.FromSeconds(30),
    OnOpened = args => { Console.WriteLine("Circuito abierto"); return default; },
    OnClosed = args => { Console.WriteLine("Circuito cerrado"); return default; },
    OnHalfOpened = args => { Console.WriteLine("Circuito en estado Half-Open"); return default; }
};

var timeout = new TimeoutStrategyOptions
{
    Timeout = TimeSpan.FromSeconds(10),
    OnTimeout = args =>
    {
        Console.WriteLine("Operación expirada");
        return default;
    }
};

// Construimos el pipeline
var pipeline = new ResiliencePipelineBuilder()
    .AddRetry(retry)
    .AddCircuitBreaker(circuitBreaker)
    .AddTimeout(timeout)
    .Build();

builder.Services.AddSingleton(pipeline);
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithOrigins(builder.Configuration["Frontend:Origin"] ?? "http://localhost:3000");
    });
});



builder.Services.AddHostedService<Worker>();
builder.Services.Configure<RabbitSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<IEventConsumer, RabbitConsumer>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddDbContext<SubscriptionContext>(options =>
    options.UseMySql(
          builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 0)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,              // número máximo de reintentos
            maxRetryDelay: TimeSpan.FromSeconds(10), // tiempo máximo entre reintentos
            errorNumbersToAdd: null        // puedes especificar códigos de error adicionales
        )
    ));


var app = builder.Build();
app.UseCors();
app.MapHub<NotificationHub>("/notificaciones");

app.Run();