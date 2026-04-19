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
COPY ["Shared/Barcelo.Authorization.Shared/Barcelo.Authorization.Shared.csproj", "Shared/Barcelo.Authorization.Shared/"]
COPY NuGet.config .

# Restaurar dependencias (retry por conexión inestable)
RUN for i in 1 2 3 4 5; do \
      dotnet restore "Usuarios/Usuarios.API/Usuarios.API.csproj" --disable-parallel && break; \
      echo "Retry $i/5..."; sleep 15; \
    done

# Copiar código fuente
COPY Usuarios/ Usuarios/
COPY Grpc.Contracts/ Grpc.Contracts/
COPY Notification.Domain/ Notification.Domain/
COPY Usuarios.ExternalService/ Usuarios.ExternalService/
COPY Shared/Barcelo.Authorization.Shared/ Shared/Barcelo.Authorization.Shared/

# Build
WORKDIR "/src/Usuarios/Usuarios.API"
RUN dotnet build "Usuarios.API.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "Usuarios.API.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

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

ENTRYPOINT ["dotnet", "Usuarios.API.dll"]
