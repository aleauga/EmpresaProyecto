FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copiar los proyectos dependientes y el proyecto WorkerService
COPY EmpresaProyecto.Core ./EmpresaProyecto.Core
COPY EmpresaProyecto.Infrastructure ./EmpresaProyecto.Infrastructure
COPY EmpresaProyecto.WorkerService/EmpresaProyecto.WorkerService.csproj ./EmpresaProyecto.WorkerService/
COPY EmpresaProyecto.WorkerService ./EmpresaProyecto.WorkerService
WORKDIR /src/EmpresaProyecto.WorkerService
RUN dotnet restore "EmpresaProyecto.WorkerService.csproj"
RUN dotnet publish "EmpresaProyecto.WorkerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EmpresaProyecto.WorkerService.dll"]