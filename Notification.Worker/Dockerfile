# Dockerfile optimizado multi-stage para Notification.Worker
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["Notification.Worker/Notification.Worker.csproj", "Notification.Worker/"]
COPY ["EmailService.Infrastructure/Notification.Email.csproj", "EmailService.Infrastructure/"]
COPY ["Notification.Kafka/Notification.Kafka.csproj", "Notification.Kafka/"]
COPY ["Notification.Push/Notification.Push.csproj", "Notification.Push/"]
COPY ["Notification.Domain/Notification.Domain.csproj", "Notification.Domain/"]

# Restaurar dependencias
RUN dotnet restore "Notification.Worker/Notification.Worker.csproj"

# Copiar código fuente
COPY Notification.Worker/ Notification.Worker/
COPY EmailService.Infrastructure/ EmailService.Infrastructure/
COPY Notification.Kafka/ Notification.Kafka/
COPY Notification.Push/ Notification.Push/
COPY Notification.Domain/ Notification.Domain/

# Build
WORKDIR "/src/Notification.Worker"
RUN dotnet build "Notification.Worker.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "Notification.Worker.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configuración optimizada
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "Notification.Worker.dll"]
