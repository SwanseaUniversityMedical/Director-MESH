FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/DirectorMesh.SiteAgent/DirectorMesh.SiteAgent.csproj", "src/DirectorMesh.SiteAgent/"]
RUN dotnet restore "src/DirectorMesh.SiteAgent/DirectorMesh.SiteAgent.csproj"
COPY . .
WORKDIR "/src/src/DirectorMesh.SiteAgent"
RUN dotnet build "DirectorMesh.SiteAgent.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DirectorMesh.SiteAgent.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DirectorMesh.SiteAgent.dll"]
