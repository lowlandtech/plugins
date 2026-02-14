FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy Directory.Build.props first for layer caching
COPY src/Directory.Build.props src/
COPY tests/Directory.Build.props tests/

# Copy all project files for restore
COPY src/BaGet/BaGet.csproj src/BaGet/
COPY src/BaGet.Core/BaGet.Core.csproj src/BaGet.Core/
COPY src/BaGet.Protocol/BaGet.Protocol.csproj src/BaGet.Protocol/
COPY src/BaGet.Web/BaGet.Web.csproj src/BaGet.Web/
COPY src/BaGet.Components/BaGet.Components.csproj src/BaGet.Components/
COPY src/BaGet.Database.PostgreSql/BaGet.Database.PostgreSql.csproj src/BaGet.Database.PostgreSql/
COPY src/BaGet.Database.Sqlite/BaGet.Database.Sqlite.csproj src/BaGet.Database.Sqlite/
COPY src/BaGet.Azure/BaGet.Azure.csproj src/BaGet.Azure/
COPY src/BaGet.Aws/BaGet.Aws.csproj src/BaGet.Aws/
COPY src/BaGet.Gcp/BaGet.Gcp.csproj src/BaGet.Gcp/
COPY src/BaGet.Aliyun/BaGet.Aliyun.csproj src/BaGet.Aliyun/

# Restore dependencies
RUN dotnet restore src/BaGet/BaGet.csproj

# Copy all source code
COPY src/ src/

# Build the application
WORKDIR /src/src/BaGet
RUN dotnet build -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
LABEL org.opencontainers.image.source="https://github.com/loic-sharma/BaGet"
WORKDIR /app

# Create data directory for packages
RUN mkdir -p /data/packages

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BaGet.dll"]
