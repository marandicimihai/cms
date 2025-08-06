FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/CMS.Main/CMS.Main.csproj", "src/CMS.Main/"]
COPY ["src/CMS.Main.Client/CMS.Main.Client.csproj", "src/CMS.Main.Client/"]
RUN dotnet restore "src/CMS.Main/CMS.Main.csproj"
COPY . .
WORKDIR "/src/src/CMS.Main"
RUN dotnet build "./CMS.Main.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CMS.Main.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CMS.Main.dll"]
