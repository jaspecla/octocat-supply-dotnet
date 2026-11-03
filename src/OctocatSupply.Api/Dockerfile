FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/OctocatSupply.Api/OctocatSupply.Api.csproj", "src/OctocatSupply.Api/"]
COPY ["src/OctocatSupply.Observability/OctocatSupply.Observability.csproj", "src/OctocatSupply.Observability/"]
RUN dotnet restore "src/OctocatSupply.Api/OctocatSupply.Api.csproj"

COPY src/OctocatSupply.Api/ src/OctocatSupply.Api/
COPY src/OctocatSupply.Observability/ src/OctocatSupply.Observability/
RUN dotnet publish "src/OctocatSupply.Api/OctocatSupply.Api.csproj" \
    --configuration Release --no-restore --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DbFile=/app/data/app.db
EXPOSE 8080

COPY --from=build /app/publish .
RUN mkdir -p /app/data && chown "$APP_UID" /app/data
USER $APP_UID

ENTRYPOINT ["dotnet", "OctocatSupply.Api.dll"]
