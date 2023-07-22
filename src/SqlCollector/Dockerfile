FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["SqlCollector/SqlCollector.csproj", "SqlCollector/"]
RUN dotnet restore "SqlCollector/SqlCollector.csproj"
COPY . .
WORKDIR "/src/SqlCollector"
RUN dotnet build "SqlCollector.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SqlCollector.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SqlCollector.dll"]