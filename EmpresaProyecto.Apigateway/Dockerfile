FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY EmpresaProyecto.Apigateway/Apigateway.csproj ./EmpresaProyecto.Apigateway/
COPY EmpresaProyecto.Apigateway ./EmpresaProyecto.Apigateway
WORKDIR /src/EmpresaProyecto.Apigateway
RUN dotnet restore "Apigateway.csproj"
RUN dotnet publish "Apigateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Apigateway.dll"]