FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects
COPY ["Shared/Lach.Shared.Common/Lach.Shared.Common.csproj", "Shared/Lach.Shared.Common/"]
COPY ["Shared/Lach.Shared.Messaging/Lach.Shared.Messaging.csproj", "Shared/Lach.Shared.Messaging/"]

# Copy service project
COPY ["Services/AuthService/AuthService.csproj", "Services/AuthService/"]

# Restore dependencies
RUN dotnet restore "Services/AuthService/AuthService.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/Services/AuthService"
RUN dotnet build "AuthService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthService.dll"] 