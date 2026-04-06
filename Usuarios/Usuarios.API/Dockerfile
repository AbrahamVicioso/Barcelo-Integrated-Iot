# Dockerfile optimizado multi-stage para Usuarios.API
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5284
EXPOSE 5285

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto para aprovechar caché de Docker
COPY ["Usuarios/Usuarios.API/Usuarios.API.csproj", "Usuarios/Usuarios.API/"]
COPY ["Usuarios/Usuarios.Application/Usuarios.Application.csproj", "Usuarios/Usuarios.Application/"]
COPY ["Usuarios/Usuarios.Domain/Usuarios.Domain.csproj", "Usuarios/Usuarios.Domain/"]
COPY ["Usuarios/Usuarios.Persistence/Usuarios.Persistence.csproj", "Usuarios/Usuarios.Persistence/"]
COPY ["Grpc.Contracts/Grpc.Contracts.csproj", "Grpc.Contracts/"]
COPY ["Notification.Domain/Notification.Domain.csproj", "Notification.Domain/"]
COPY ["Usuarios.ExternalService/Usuarios.ExternalService.csproj", "Usuarios.ExternalService/"]

# Restaurar dependencias
RUN dotnet restore "Usuarios/Usuarios.API/Usuarios.API.csproj"

# Copiar código fuente
COPY Usuarios/ Usuarios/
COPY Grpc.Contracts/ Grpc.Contracts/
COPY Notification.Domain/ Notification.Domain/
COPY Usuarios.ExternalService/ Usuarios.ExternalService/

# Build
WORKDIR "/src/Usuarios/Usuarios.API"
RUN dotnet build "Usuarios.API.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "Usuarios.API.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configuración optimizada
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "Usuarios.API.dll"]
