# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 10000

# Build image
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Sandoohouse.csproj", "./"]
RUN dotnet restore "Sandoohouse.csproj"
COPY . .
RUN dotnet build "Sandoohouse.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish "Sandoohouse.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Sandoohouse.dll"]