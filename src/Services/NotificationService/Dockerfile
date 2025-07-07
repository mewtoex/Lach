FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects
COPY ["Shared/Lach.Shared.Common/Lach.Shared.Common.csproj", "Shared/Lach.Shared.Common/"]
COPY ["Shared/Lach.Shared.Messaging/Lach.Shared.Messaging.csproj", "Shared/Lach.Shared.Messaging/"]

# Copy service project
COPY ["Services/NotificationService/NotificationService.csproj", "Services/NotificationService/"]

# Restore dependencies
RUN dotnet restore "Services/NotificationService/NotificationService.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/Services/NotificationService"
RUN dotnet build "NotificationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.dll"] 