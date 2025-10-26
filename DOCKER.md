# 🐳 Docker Deployment Guide

This guide explains how to run the CMS application using Docker Compose.

## 📋 Prerequisites

- Docker (version 20.10 or higher)
- Docker Compose (version 2.0 or higher)

## 🚀 Quick Start

### 1. Build and Start All Services

```bash
docker-compose up -d --build
```

This will start:
- **PostgreSQL** database on port `5432`
- **CMS Application** on port `8080`
- **smtp4dev** email testing server on port `5001` (Web UI) and `2525` (SMTP)

### 2. Access the Application

- **CMS Web App**: http://localhost:12354
- **smtp4dev Web UI**: http://localhost:5001
- **PostgreSQL**: localhost:5432
  - Username: `postgres`
  - Password: `postgres`
  - Database: `CMS`

## 📧 Email Testing with smtp4dev

smtp4dev captures all outgoing emails from the CMS application. To view them:

1. Open http://localhost:5001 in your browser
2. All emails sent by the application will appear here
3. You can inspect email content, headers, and attachments

## 📁 Volumes

The following volumes persist data:

- `postgres-data`: PostgreSQL database files
- `dataprotection-keys`: ASP.NET Core data protection keys

## 🔧 Configuration

### Connection Strings

The application uses these connection strings (configured in `docker-compose.yml`):

- **Database**: `Host=postgres;Port=5432;Database=CMS;Username=postgres;Password=postgres`
- **SMTP**: `Host=smtp4dev;Port=25`

## 🔒 Security Notes

**⚠️ Important for Production:**

1. **Change default passwords** in `docker-compose.yml`
2. **Use secrets management** instead of environment variables
3. **Configure HTTPS** and update `ASPNETCORE_URLS`
4. **Restrict network access** to PostgreSQL and smtp4dev
5. **Use proper data protection key storage** (not file system)

## 📦 Production Deployment

For production, consider:

1. Using a managed PostgreSQL service (AWS RDS, Azure Database, etc.)
2. Using a real SMTP service (SendGrid, AWS SES, etc.)
3. Configuring reverse proxy (nginx, Traefik) for HTTPS
4. Setting up proper logging and monitoring
5. Using Docker Swarm or Kubernetes for orchestration