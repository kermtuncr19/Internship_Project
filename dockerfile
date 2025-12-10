# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["StoreApp/StoreApp.csproj", "StoreApp/"]
COPY ["Entities/Entities.csproj", "Entities/"]
COPY ["Repositories/Repositories.csproj", "Repositories/"]
COPY ["Services/Services.csproj", "Services/"]

# Restore dependencies
RUN dotnet restore "StoreApp/StoreApp.csproj"

# Copy all source files
COPY . .

# Build
WORKDIR "/src/StoreApp"
RUN dotnet build "StoreApp.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "StoreApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Expose port
EXPOSE 8080

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Copy published files from publish stage
COPY --from=publish /app/publish .

# Entry point - dosya ismi doğru olmalı!
ENTRYPOINT ["dotnet", "StoreApp.dll"]