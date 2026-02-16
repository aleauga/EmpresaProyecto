using EmpresaProyecto.API.Subscriptions.Services.Contracts;
using EmpresaProyecto.API.Subscriptions.Services.Implementations;
using EmpresaProyecto.Core.Entities;
using EmpresaProyecto.Core.Messaging.Contracts;
using EmpresaProyecto.Core.Models;
using EmpresaProyecto.Core.Repository.Contracts;
using EmpresaProyecto.Infrastructure.Messaging;
using EmpresaProyecto.Infrastructure.Persistence.Context;
using EmpresaProyecto.Infrastructure.Persistence.Repository.Implementations;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddValidatorsFromAssemblyContaining<ValidatorClient>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IEventPublisher, RabbitPublisher>();
builder.Services.Configure<RabbitSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddDbContext<SubscriptionContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 0))
    ));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(builder.Configuration["Frontend:Origin"] ?? "http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

app.MapControllers();

app.Run();