# ============================================================
# Multi-stage build — WhatsApp Error Assistant
# ============================================================

# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["WhatsAppErrorAssistant.csproj", "."]
RUN dotnet restore "./WhatsAppErrorAssistant.csproj"

COPY . .
RUN dotnet build "WhatsAppErrorAssistant.csproj" -c Release -o /app/build

# --- Publish stage ---
FROM build AS publish
RUN dotnet publish "WhatsAppErrorAssistant.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Non-root user for security
RUN adduser --disabled-password --gecos "" appuser
WORKDIR /app

COPY --from=publish /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "WhatsAppErrorAssistant.dll"]
