# ===========================
# BUILD STAGE
# ===========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY . .

WORKDIR /src/SalesManagementSystem.API/SalesManagementSystem.API

RUN dotnet restore SalesManagementSystem.API.csproj

RUN dotnet publish SalesManagementSystem.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ===========================
# RUNTIME STAGE
# ===========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet","SalesManagementSystem.API.dll"]