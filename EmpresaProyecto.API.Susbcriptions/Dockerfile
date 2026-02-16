FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copiar los proyectos dependientes y el proyecto API
COPY EmpresaProyecto.API.Susbcriptions/EmpresaProyecto.API.Subscriptions.csproj ./EmpresaProyecto.API.Susbcriptions/
COPY EmpresaProyecto.Core ./EmpresaProyecto.Core
COPY EmpresaProyecto.Infrastructure ./EmpresaProyecto.Infrastructure
COPY EmpresaProyecto.API.Susbcriptions ./EmpresaProyecto.API.Susbcriptions
WORKDIR /src/EmpresaProyecto.API.Susbcriptions
RUN dotnet restore "EmpresaProyecto.API.Subscriptions.csproj"
RUN dotnet publish "EmpresaProyecto.API.Subscriptions.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EmpresaProyecto.API.Subscriptions.dll"]