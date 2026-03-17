# 📱 WhatsApp Error Assistant

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com)
[![Claude Haiku](https://img.shields.io/badge/Claude-Haiku_4.5-orange?logo=anthropic)](https://www.anthropic.com)
[![Docker](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker)](https://www.docker.com)
[![CI](https://github.com/YOUR_USERNAME/whatsapp-error-assistant/actions/workflows/ci.yml/badge.svg)](https://github.com/YOUR_USERNAME/whatsapp-error-assistant/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

WhatsApp hata mesajlarını ve log çıktılarını **Claude Haiku AI** ile analiz eden .NET 8 Minimal API servisi.

---

## Özellikler

- `POST /analyze` — WhatsApp hata mesajını gönder, Türkçe analiz al
- **Swagger UI** root'ta (`/`) açılır
- **Rate limiting** — IP başına dakikada 10 istek
- Claude Haiku (`claude-haiku-4-5`) entegrasyonu
- Docker ve docker-compose desteği
- GitHub Actions CI pipeline

---

## Hızlı Başlangıç

### Gereksinimler

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Claude API anahtarı](https://console.anthropic.com/settings/keys)

### 1. Ortam değişkenini ayarla

```bash
cp .env.example .env
# .env dosyasını açıp CLAUDE_API_KEY değerini girin
```

### 2. Çalıştır

```bash
# Ortam değişkenini export et
export CLAUDE_API_KEY=sk-ant-...

# Bağımlılıkları yükle ve çalıştır
dotnet restore
dotnet run
```

Tarayıcıda `http://localhost:5000` adresine gidin — Swagger UI açılır.

---

## Docker ile Çalıştırma

```bash
# .env dosyasını oluştur
cp .env.example .env
# CLAUDE_API_KEY değerini düzenle

# Derle ve çalıştır
docker-compose up --build

# Arka planda çalıştır
docker-compose up -d --build
```

Servis `http://localhost:8080` adresinde çalışır.

---

## API Kullanımı

### `POST /analyze`

**Request:**

```json
{
  "message": "Error 401: Unauthorized. Connection to WhatsApp servers failed.",
  "context": "iPhone 15, WhatsApp 24.x"
}
```

**Response (200):**

```json
{
  "analysis": "**Hata Açıklaması**\n401 Unauthorized hatası...\n\n**Olası Nedenler**\n1. ...\n\n**Çözüm Önerileri**\n...\n\n**Önem Derecesi:** Orta",
  "model": "claude-haiku-4-5",
  "inputMessage": "Error 401: Unauthorized...",
  "timestamp": "2026-03-17T10:00:00Z"
}
```

**Rate limit aşıldığında (429):**

```json
{
  "error": "Rate limit aşıldı. Dakikada en fazla 10 istek gönderebilirsiniz.",
  "retryAfterSeconds": 60
}
```

### `GET /health`

```json
{ "status": "healthy", "timestamp": "2026-03-17T10:00:00Z" }
```

---

## Ortam Değişkenleri

| Değişken | Zorunlu | Açıklama |
|---|---|---|
| `CLAUDE_API_KEY` | Evet | Anthropic API anahtarı |
| `ASPNETCORE_ENVIRONMENT` | Hayır | `Development` / `Production` (varsayılan: `Production`) |
| `ASPNETCORE_URLS` | Hayır | Dinlenecek adres (varsayılan: `http://+:8080`) |

---

## Proje Yapısı

```
whatsapp-error-assistant/
├── .github/workflows/ci.yml   # GitHub Actions CI
├── Models/
│   ├── AnalyzeRequest.cs
│   └── AnalyzeResponse.cs
├── Services/
│   └── ClaudeService.cs       # Claude Haiku entegrasyonu
├── Program.cs                 # Minimal API + Middleware
├── WhatsAppErrorAssistant.csproj
├── Dockerfile
├── docker-compose.yml
└── .env.example
```

---

## Geliştirme

```bash
# Sıcak yeniden yükleme ile çalıştır
dotnet watch run
```

---

## Lisans

[MIT](LICENSE)
