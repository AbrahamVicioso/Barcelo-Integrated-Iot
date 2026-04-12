# Dockerfile optimizado multi-stage para Dispositivos.API
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5185

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["Dispositivos.API/Dispositivos.API.csproj", "Dispositivos.API/"]
COPY ["Dispositivos.Application/Dispositivos.Application.csproj", "Dispositivos.Application/"]
COPY ["Dispositivos.Domain/Dispositivos.Domain.csproj", "Dispositivos.Domain/"]
COPY ["Dispositivos.Infrastructure/Dispositivos.Infrastructure.csproj", "Dispositivos.Infrastructure/"]
COPY ["Dispositivos.Persistence/Dispositivos.Persistence.csproj", "Dispositivos.Persistence/"]
COPY ["Notification.Domain/Notification.Domain.csproj", "Notification.Domain/"]

# Restaurar dependencias
RUN dotnet restore "Dispositivos.API/Dispositivos.API.csproj"

# Copiar código fuente
COPY Dispositivos.API/ Dispositivos.API/
COPY Dispositivos.Application/ Dispositivos.Application/
COPY Dispositivos.Domain/ Dispositivos.Domain/
COPY Dispositivos.Infrastructure/ Dispositivos.Infrastructure/
COPY Dispositivos.Persistence/ Dispositivos.Persistence/
COPY Notification.Domain/ Notification.Domain/

# Build
WORKDIR "/src/Dispositivos.API"
RUN dotnet build "Dispositivos.API.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "Dispositivos.API.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

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

ENTRYPOINT ["dotnet", "Dispositivos.API.dll"]
