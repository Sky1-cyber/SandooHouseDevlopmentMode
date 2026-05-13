# -----------------------------
# Build stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["Sandoohouse.csproj", "./"]
RUN dotnet restore "Sandoohouse.csproj"

# Copy the rest of the code
COPY . .

# Build and publish
RUN dotnet publish "Sandoohouse.csproj" -c Release -o /app/publish /p:UseAppHost=false


# -----------------------------
# Runtime stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Render uses port 10000
EXPOSE 10000

# Start application
ENTRYPOINT ["dotnet", "Sandoohouse.dll"]