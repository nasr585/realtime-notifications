# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY RealtimeNotifications.sln ./
COPY src/RealtimeNotifications.Domain/RealtimeNotifications.Domain.csproj src/RealtimeNotifications.Domain/
COPY src/RealtimeNotifications.Application/RealtimeNotifications.Application.csproj src/RealtimeNotifications.Application/
COPY src/RealtimeNotifications.Infrastructure/RealtimeNotifications.Infrastructure.csproj src/RealtimeNotifications.Infrastructure/
COPY src/RealtimeNotifications.Api/RealtimeNotifications.Api.csproj src/RealtimeNotifications.Api/
RUN dotnet restore src/RealtimeNotifications.Api/RealtimeNotifications.Api.csproj

COPY src/ src/
RUN dotnet publish src/RealtimeNotifications.Api/RealtimeNotifications.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RealtimeNotifications.Api.dll"]
