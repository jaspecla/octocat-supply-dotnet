FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/OctocatSupply.Web/OctocatSupply.Web.csproj", "src/OctocatSupply.Web/"]
COPY ["src/OctocatSupply.Observability/OctocatSupply.Observability.csproj", "src/OctocatSupply.Observability/"]
RUN dotnet restore "src/OctocatSupply.Web/OctocatSupply.Web.csproj"

COPY src/OctocatSupply.Web/ src/OctocatSupply.Web/
COPY src/OctocatSupply.Observability/ src/OctocatSupply.Observability/
RUN dotnet publish "src/OctocatSupply.Web/OctocatSupply.Web.csproj" \
    --configuration Release --no-restore --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish .
USER $APP_UID

ENTRYPOINT ["dotnet", "OctocatSupply.Web.dll"]
