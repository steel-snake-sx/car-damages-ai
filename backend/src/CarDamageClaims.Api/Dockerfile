FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/CarDamageClaims.sln backend/
COPY backend/src/CarDamageClaims.Api/CarDamageClaims.Api.csproj backend/src/CarDamageClaims.Api/
RUN dotnet restore backend/CarDamageClaims.sln

COPY backend/ backend/
RUN dotnet publish backend/src/CarDamageClaims.Api/CarDamageClaims.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CarDamageClaims.Api.dll"]
