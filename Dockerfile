FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution and project files first for better layer caching
COPY em2devs-apps-building-management.sln .
COPY src/EM2Devs.BuildingManagement.Application/EM2Devs.BuildingManagement.Application.csproj src/EM2Devs.BuildingManagement.Application/
COPY tests/EM2Devs.BuildingManagement.Application.Unit.Tests/EM2Devs.BuildingManagement.Application.Unit.Tests.csproj tests/EM2Devs.BuildingManagement.Application.Unit.Tests/
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet build src/EM2Devs.BuildingManagement.Application/EM2Devs.BuildingManagement.Application.csproj \
    -c $BUILD_CONFIGURATION \
    --no-restore

FROM build AS test
RUN dotnet test tests/EM2Devs.BuildingManagement.Application.Unit.Tests/EM2Devs.BuildingManagement.Application.Unit.Tests.csproj \
    -c $BUILD_CONFIGURATION \
    --no-build \
    --no-restore \
    --verbosity normal

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish src/EM2Devs.BuildingManagement.Application/EM2Devs.BuildingManagement.Application.csproj \
    -c $BUILD_CONFIGURATION \
    --no-build \
    -o /app/publish

FROM base AS final
WORKDIR /app

# Use the built-in non-root user from the .NET base image
USER $APP_UID

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EM2Devs.BuildingManagement.Application.dll"]
