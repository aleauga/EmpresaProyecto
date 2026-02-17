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

var paymentGatewayBaseUrl = builder.Configuration["PaymentGateway:BaseUrl"] ?? "http://httpbin:80/";
builder.Services.AddHttpClient<IPaymentGateway, PaymentGatewayClient>(client =>
{
    client.BaseAddress = new Uri(paymentGatewayBaseUrl);
});

// Configuración manual de pipeline de resiliencia con Polly
// Estrategia de reintentos con backoff exponencial
var retry = new RetryStrategyOptions
{
    MaxRetryAttempts = 3,
    DelayGenerator = args =>
    {
        var delay = TimeSpan.FromSeconds(Math.Pow(2, args.AttemptNumber)); // 2^n segundos
        return new ValueTask<TimeSpan?>(delay);
    },
    OnRetry = args =>
    {
        Console.WriteLine($"Reintento {args.AttemptNumber} después de {args.RetryDelay.TotalSeconds} segundos");
        return default;
    }
};

// Circuit breaker: abre el circuito si falla más del 50% de las llamadas
var circuitBreaker = new CircuitBreakerStrategyOptions
{
    FailureRatio = 0.5,
    MinimumThroughput = 2,
    BreakDuration = TimeSpan.FromSeconds(30),
    OnOpened = args => { Console.WriteLine("Circuito abierto"); return default; },
    OnClosed = args => { Console.WriteLine("Circuito cerrado"); return default; },
    OnHalfOpened = args => { Console.WriteLine("Circuito en estado Half-Open"); return default; }
};

// Timeout: corta operaciones que tarden más de 10 segundos
var timeout = new TimeoutStrategyOptions
{
    Timeout = TimeSpan.FromSeconds(10),
    OnTimeout = args =>
    {
        Console.WriteLine("Operación expirada");
        return default;
    }
};

// Construcción del pipeline de resiliencia
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


builder.Services.AddHostedService<Worker>(); // Servicio en segundo plano que consume eventos
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
            errorNumbersToAdd: null        // códigos de error adicionales opcionales
        )
    ));

var app = builder.Build();

app.UseCors();

app.MapHub<NotificationHub>("/notificaciones");

app.Run();