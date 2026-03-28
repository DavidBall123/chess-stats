FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY api/ChessMonitor.Api.csproj api/
COPY shared/ChessMonitor.Shared.csproj shared/
RUN dotnet restore api/ChessMonitor.Api.csproj

COPY api/ api/
COPY shared/ shared/
RUN dotnet publish api/ChessMonitor.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ChessMonitor.Api.dll"]
