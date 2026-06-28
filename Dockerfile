# See https://aka.ms/customizecontainer to learn how to customize your debug container.
# Build context is the solution root (hobby-app-backend/).

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore as distinct layers for better caching.
COPY ["src/HobbyApp.Domain/HobbyApp.Domain.csproj", "src/HobbyApp.Domain/"]
COPY ["src/HobbyApp.Application/HobbyApp.Application.csproj", "src/HobbyApp.Application/"]
COPY ["src/HobbyApp.Infrastructure/HobbyApp.Infrastructure.csproj", "src/HobbyApp.Infrastructure/"]
COPY ["src/HobbyApp.Api/HobbyApp.Api.csproj", "src/HobbyApp.Api/"]
RUN dotnet restore "src/HobbyApp.Api/HobbyApp.Api.csproj"

# Copy the rest of the source and build.
COPY . .
WORKDIR "/src/src/HobbyApp.Api"
RUN dotnet build "HobbyApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "HobbyApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HobbyApp.Api.dll"]
