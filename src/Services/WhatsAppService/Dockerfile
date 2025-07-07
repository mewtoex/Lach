FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects
COPY ["Shared/Lach.Shared.Common/Lach.Shared.Common.csproj", "Shared/Lach.Shared.Common/"]
COPY ["Shared/Lach.Shared.Messaging/Lach.Shared.Messaging.csproj", "Shared/Lach.Shared.Messaging/"]

# Copy service project
COPY ["Services/WhatsAppService/WhatsAppService.csproj", "Services/WhatsAppService/"]

# Restore dependencies
RUN dotnet restore "Services/WhatsAppService/WhatsAppService.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/Services/WhatsAppService"
RUN dotnet build "WhatsAppService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WhatsAppService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WhatsAppService.dll"] 