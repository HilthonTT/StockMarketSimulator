# See https://aka.ms/customizecontainer to learn how to customize your debug container.

# Runtime base image (also used by Visual Studio fast mode)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# Shared build settings and central package versions must be present before restore.
COPY ["global.json", "Directory.Build.props", "Directory.Packages.props", ".editorconfig", "./"]
COPY ["src/", "src/"]
RUN dotnet restore "src/Yarp.Proxy/Yarp.Proxy.csproj"
WORKDIR "/src/src/Yarp.Proxy"
RUN dotnet build "./Yarp.Proxy.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Yarp.Proxy.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Yarp.Proxy.dll"]
