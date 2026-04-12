# Dockerfile optimizado multi-stage para ApiGateway
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5019
EXPOSE 5020

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["ApiGateway/ApiGateway.csproj", "ApiGateway/"]

# Restaurar dependencias
RUN dotnet restore "ApiGateway/ApiGateway.csproj"

# Copiar código fuente
COPY ApiGateway/ ApiGateway/

# Build
WORKDIR "/src/ApiGateway"
RUN dotnet build "ApiGateway.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app

RUN apt-get update && apt-get install -y tzdata
    
ENV TZ=America/Santo_Domingo

COPY --from=publish /app/publish .

# Configuración optimizada
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "ApiGateway.dll"]
