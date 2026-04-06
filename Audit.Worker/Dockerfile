# Dockerfile optimizado multi-stage para Audit.Worker
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5250
EXPOSE 5251

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["Audit.Worker/Audit.Worker.csproj", "Audit.Worker/"]
COPY ["Notification.Domain/Notification.Domain.csproj", "Notification.Domain/"]

# Restaurar dependencias
RUN dotnet restore "Audit.Worker/Audit.Worker.csproj"

# Copiar código fuente
COPY Audit.Worker/ Audit.Worker/
COPY Notification.Domain/ Notification.Domain/

# Build
WORKDIR "/src/Audit.Worker"
RUN dotnet build "Audit.Worker.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "Audit.Worker.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configuración optimizada
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "Audit.Worker.dll"]
