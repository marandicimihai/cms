FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# Ensure the data protection keys directory exists and is writable before switching to non-root user
USER root
RUN mkdir -p /var/DataProtection-Keys \
    && chmod -R 777 /var/DataProtection-Keys
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Install Node.js for Tailwind CSS build
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get install -y nodejs

# Copy package files and install Node dependencies
COPY ["package.json", "package-lock.json*", "./"]
RUN npm ci --omit=dev

# Copy project file and restore .NET dependencies
COPY ["src/CMS.Main/CMS.Main.csproj", "src/CMS.Main/"]
RUN dotnet restore "src/CMS.Main/CMS.Main.csproj"

# Copy all source files
COPY . .

# Build Tailwind CSS
WORKDIR "/src"
RUN npx tailwindcss -i src/CMS.Main/wwwroot/css/app.css -o src/CMS.Main/wwwroot/css/tailwind.css --minify

# Build .NET application
WORKDIR "/src/src/CMS.Main"
RUN dotnet build "./CMS.Main.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CMS.Main.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy Email Templates directory to /app/Emails/Templates
COPY src/CMS.Main/Emails/Templates /app/Emails/Templates

ENTRYPOINT ["dotnet", "CMS.Main.dll"]
