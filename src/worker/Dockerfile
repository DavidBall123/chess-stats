FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY worker/ChessMonitor.Worker.csproj worker/
COPY shared/ChessMonitor.Shared.csproj shared/
RUN dotnet restore worker/ChessMonitor.Worker.csproj

COPY worker/ worker/
COPY shared/ shared/
RUN dotnet publish worker/ChessMonitor.Worker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ChessMonitor.Worker.dll"]
