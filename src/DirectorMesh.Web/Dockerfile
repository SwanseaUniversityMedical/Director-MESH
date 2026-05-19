FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/DirectorMesh.Web/DirectorMesh.Web.csproj", "src/DirectorMesh.Web/"]
RUN dotnet restore "src/DirectorMesh.Web/DirectorMesh.Web.csproj"
COPY . .
WORKDIR "/src/src/DirectorMesh.Web"
RUN dotnet build "DirectorMesh.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DirectorMesh.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DirectorMesh.Web.dll"]
