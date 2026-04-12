    # Dockerfile optimizado multi-stage para Reservas.API
    FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
    WORKDIR /app
    EXPOSE 5141

    FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
    WORKDIR /src

    # Copiar archivos de proyecto
    COPY ["Reservas/Reservas.API/Reservas.API.csproj", "Reservas/Reservas.API/"]
    COPY ["Reservas/Reservas.Application/Reservas.Application.csproj", "Reservas/Reservas.Application/"]
    COPY ["Reservas/Reservas.Domain/Reservas.Domain.csproj", "Reservas/Reservas.Domain/"]
    COPY ["Reservas/Reservas.Infrastructure/Reservas.Infrastructure.csproj", "Reservas/Reservas.Infrastructure/"]
    COPY ["Reservas/Reservas.Persistence/Reservas.Persistence.csproj", "Reservas/Reservas.Persistence/"]
    COPY ["Reservas.Email/Reservas.Email.csproj", "Reservas.Email/"]
    COPY ["Grpc.Contracts/Grpc.Contracts.csproj", "Grpc.Contracts/"]
    COPY ["Notification.Domain/Notification.Domain.csproj", "Notification.Domain/"]

    # Restaurar dependencias
    RUN dotnet restore "Reservas/Reservas.API/Reservas.API.csproj"

    # Copiar código fuente
    COPY Reservas/ Reservas/
    COPY Reservas.Email/ Reservas.Email/
    COPY Grpc.Contracts/ Grpc.Contracts/
    COPY Notification.Domain/ Notification.Domain/

    # Build
    WORKDIR "/src/Reservas/Reservas.API"
    RUN dotnet build "Reservas.API.csproj" -c Release -o /app/build --no-restore

    FROM build AS publish
    RUN dotnet publish "Reservas.API.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

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

    ENTRYPOINT ["dotnet", "Reservas.API.dll"]
