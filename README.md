# 🚀 CMS - Headless Content Management System

A modern, headless content management system built with ASP.NET Core 9.0, Blazor, and PostgreSQL. Create custom content schemas, manage entries via REST API, and integrate seamlessly with any frontend framework.

## 🎥 Demo

[![CMS Showcase](https://img.youtube.com/vi/13ACYDFdx0s/0.jpg)](https://youtu.be/13ACYDFdx0s)

Watch the full demo: [https://youtu.be/13ACYDFdx0s](https://youtu.be/13ACYDFdx0s)

## ✨ Features

- **Custom Schemas** - Define your own content types with flexible field properties
- **REST API** - Full CRUD operations for content entries with filtering and pagination
- **API Key Authentication** - Secure access control for your content
- **Blazor UI** - Modern, responsive web interface for content management
- **PostgreSQL** - Robust, scalable database backend
- **Email Integration** - Built-in email notifications with FluentEmail
- **Docker Support** - One-command deployment with Docker Compose

## 🛠️ Tech Stack

- **Backend**: ASP.NET Core 9.0, FastEndpoints
- **Frontend**: Blazor Server, TailwindCSS
- **Database**: PostgreSQL with Entity Framework Core
- **Auth**: ASP.NET Core Identity with API Key authentication
- **Email**: FluentEmail with SMTP support

## 🚦 Quick Start

### Prerequisites

- [Docker](https://www.docker.com/get-started) (v20.10+) and Docker Compose (v2.0+)

### Run with Docker

1. **Clone the repository**
   ```bash
   git clone https://github.com/marandicimihai/cms.git
   cd cms
   ```

2. **Start all services**
   ```bash
   docker-compose up -d --build
   ```

3. **Access the application**
   - **CMS Web App**: http://localhost:12354
   - **Email Testing (smtp4dev)**: http://localhost:5001

4. **Create an account and start building!**
   - Register a new user account
   - Create your first project
   - Define a schema with custom fields
   - Generate an API key
   - Start creating content via the API

### Local Development

This project includes a complete dev container configuration with all dependencies pre-installed.

1. **Prerequisites**
   - [VS Code](https://code.visualstudio.com/) with [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)
   - [Docker Desktop](https://www.docker.com/products/docker-desktop)

2. **Open in Dev Container**
   - Clone the repository
   - Open the folder in VS Code
   - Click "Reopen in Container" when prompted (or use Command Palette: "Dev Containers: Reopen in Container")
   - The container includes .NET 9.0 SDK, PostgreSQL, and all necessary tools

3. **Run the application**
   ```bash
   dotnet run --project src/CMS.Main
   ```

## 📚 API Documentation

Access the full API documentation at `/documentation` when running the application, or visit http://localhost:12354/documentation

### Quick API Example

```bash
# 1. Create an entry
curl -X POST http://localhost:12354/api/{schemaId}/entries \
  -H "X-API-Key: your-api-key" \
  -H "Content-Type: application/json" \
  -d '{
    "fields": {
      "title": "My First Post",
      "content": "Hello World!"
    }
  }'

# 2. Get all entries
curl -X GET http://localhost:12354/api/{schemaId}/entries \
  -H "X-API-Key: your-api-key"
```

## 🔐 Security Notes

**⚠️ For Production Use:**

- Change default PostgreSQL password in `docker-compose.yml`
- Configure HTTPS with proper certificates
- Use a real SMTP service (SendGrid, AWS SES, etc.)
- Store secrets securely (Azure Key Vault, AWS Secrets Manager)
- Enable rate limiting and API throttling
- Review and configure CORS policies

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Feel free to open issues or submit pull requests.

## 📧 Support

For questions or issues, please open an issue on GitHub.
