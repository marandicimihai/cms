FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/CMS.App/CMS.App/CMS.App.csproj", "src/CMS.App/CMS.App/"]
COPY ["src/CMS.App/CMS.App.Client/CMS.App.Client.csproj", "src/CMS.App/CMS.App.Client/"]
RUN dotnet restore "src/CMS.App/CMS.App/CMS.App.csproj"
COPY . .
WORKDIR "/src/src/CMS.App/CMS.App"
RUN dotnet build "./CMS.App.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CMS.App.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CMS.App.dll"]
