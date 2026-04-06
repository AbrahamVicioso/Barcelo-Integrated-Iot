# Dockerfile optimizado multi-stage para Authentication.Api
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5117
EXPOSE 5118

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar solo los archivos de proyecto primero para aprovechar la caché de Docker
COPY ["Authenticate/Authentication.Api/Authentication.Api.csproj", "Authenticate/Authentication.Api/"]
COPY ["Authenticate/Authentication.Application/Authentication.Application.csproj", "Authenticate/Authentication.Application/"]
COPY ["Authenticate/Authentication.Domain/Authentication.Domain.csproj", "Authenticate/Authentication.Domain/"]
COPY ["Authenticate/Authentication.Persistence/Authentication.Persistence.csproj", "Authenticate/Authentication.Persistence/"]
COPY ["Grpc.Contracts/Grpc.Contracts.csproj", "Grpc.Contracts/"]
COPY ["Notification.Domain/Notification.Domain.csproj", "Notification.Domain/"]
COPY ["Notification.Push/Notification.Push.csproj", "Notification.Push/"]

# Restaurar dependencias
RUN dotnet restore "Authenticate/Authentication.Api/Authentication.Api.csproj"

# Copiar el resto del código fuente
COPY Authenticate/ Authenticate/
COPY Grpc.Contracts/ Grpc.Contracts/
COPY Notification.Domain/ Notification.Domain/
COPY Notification.Push/ Notification.Push/

# Build
WORKDIR "/src/Authenticate/Authentication.Api"
RUN dotnet build "Authentication.Api.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "Authentication.Api.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configuración optimizada
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "Authentication.Api.dll"]
