# CmdHelper (Command QuickCopy Helper)

<p align="center">
  <a href="https://github.com/cadedi/CmdHelper/actions"><img src="https://img.shields.io/badge/build-passing-brightgreen?style=flat-square&logo=github-actions" alt="Build Status"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue?style=flat-square" alt="License"></a>
  <img src="https://img.shields.io/badge/platform-Linux%20%7C%20Windows%20%7C%20Web-orange?style=flat-square" alt="Platform">
  <img src="https://img.shields.io/badge/.NET-8.0%20%7C%204.6.2-purple?style=flat-square&logo=dotnet" alt=".NET">
  <img src="https://img.shields.io/badge/UI-Avalonia%2011%20%7C%20WPF-blueviolet?style=flat-square" alt="UI Framework">
  <img src="https://img.shields.io/badge/offline-100%25%20Air--Gapped-success?style=flat-square" alt="Air Gapped">
</p>

<p align="center">
  <b>Scenario-Driven · Realtime Parameter Interpolation · One-Click Instant Copy · Cross-Platform Native ELF & EXE · 100% Offline & Air-Gapped</b>
</p>

<p align="center">
  <a href="README.md"><b>English</b></a> | <a href="README_zh.md"><b>简体中文</b></a>
</p>

---

## 💡 Why CmdHelper?

Traditional cheat-sheets force you to memorize arcane command flags (`tar -czvf`, `find . -mtime +7 -exec ...`, `pg_dump -Fc -h ...`, `docker exec -it ...`). 

**CmdHelper** flips the workflow: **search by what problem you want to solve**. Simply adjust dynamic parameter fields in real time, and get the accurate, production-ready command instantly copied to your clipboard.

Comprehensive scenarios covered:
- 🐧 **Linux Operations & Troubleshooting** (Disk, CPU, Memory, Network, Port Listening, Process Tracking)
- 🗄️ **SQL & Databases** (PostgreSQL, MySQL, Redis, Dump/Restore, Slow Query Log Analysis, Deadlock Inspection)
- 🐳 **Docker Containers & Kubernetes (K8s)** (Pod diagnostics, Log tailing, Resource metrics, Volume backups)
- 🔄 **Git Version Control** (Branch cleanup, Rollbacks, Stash management, Submodule fixes)
- 🌐 **Nginx & SSL/TLS** (Cert inspection, Performance tuning, Upstream proxying, Rewrite rules)
- 💻 **Windows & PowerShell** (Service control, Event log inspection, Network socket probing)

---

## 🌟 Key Features

### 1. Scenario-Driven Dynamic Interpolation
- Search commands by real-world intents (e.g., *“compress tar archive”*, *“check port 8080”*, *“dump postgres table”*).
- Live reactive parameter engine: text boxes, dropdown selections, and toggles calculate and assemble commands in milliseconds with zero latency.
- Built-in danger tags for destructive commands (e.g., `rm -rf`, force push, kill process).

### 2. True Cross-Platform Native Binaries
- **Linux Native Single-File ELF (`CmdHelper-linux-x64`)**:
  - Built with **.NET 8 + Avalonia UI 11**.
  - **Self-contained**: target Linux machines (Ubuntu, Debian, CentOS, RHEL, Kylin, UOS) **do not need .NET runtime installed**. Just `chmod +x` and run!
- **Windows Ultra-Lightweight Native (`CmdHelper-wpf.exe`)**:
  - Only **1.1 MB** single-file EXE based on .NET Framework 4.6.2 (pre-installed on Windows Server 2016+).
- **Offline Single-File Web App (`CmdHelper_Web.html`)**:
  - 100% self-contained Vanilla HTML5/CSS3/JS with **zero external CDN dependencies**, built for air-gapped server environments and jump boxes.

### 3. i18n & Multi-Language Support
- Instant bilingual toggle between **English (`en-US`)** and **Simplified Chinese (`zh-CN`)**.
- Fully localized UI: navigation bars, category mappings, badges, parameter hints, and dialogs.

### 4. Dark & Light Theme Modes
- **Light Theme**: Clean, eye-friendly `#EEEEEE` background tailored for long hours of DevOps work.
- **Dark Theme**: Modern obsidian dark palette for terminal purists.
- Preferences automatically persisted across sessions.

### 5. Multi-Source Remote Sync & Intelligent Deduplication
- Pull command libraries dynamically from internal company APIs, GitHub/GitLab Raw endpoints, or local JSON files.
- **Smart Deduplication (`merge`)**: Existing entries with the same `id` are updated automatically, while new entries are appended.
- **Offline Fault Tolerance**: Local caching guarantees full functionality even during network outages.

### 6. Client-Side Single-File Web Export
- Export the current live in-memory command collection directly to a portable, single-file HTML page for distribution to non-Windows team members.

---

## 📦 Deliverables Matrix (`release/` Directory)

| Deliverable File | Target Platform | Size | Description |
| :--- | :--- | :--- | :--- |
| **`CmdHelper-linux-x64`** | **Linux (x64)**<br>(Ubuntu / Debian / CentOS / Kylin / UOS) | ~44 MB | **Linux Native Single-File ELF**<br>Self-contained runtime; zero pre-requisites; supports i18n & Dark/Light themes. |
| **`CmdHelper-win-x64.exe`** | **Windows 10 / 11 / Server (x64)** | ~45 MB | **Avalonia Native Windows Edition**<br>Self-contained runtime with full theme and multi-language support. |
| **`CmdHelper-wpf.exe`** | **Windows Server 2016+ / 10 / 11** | **1.1 MB** | **Ultra-Lightweight WPF Edition**<br>Zero installation, instant cold start (<200ms), 25MB RAM usage. |
| **`CmdHelper_Web.html`** | **All Platforms / Any Browser** | **~79 KB** | **Pure Offline Single-File HTML**<br>Zero CDN, zero network requests, instant startup in air-gapped environments. |

---

## 🚀 Quick Start

### Linux (Ubuntu / Debian / CentOS / Kylin / UOS)
```bash
# Grant execution permissions and run
chmod +x ./CmdHelper-linux-x64
./CmdHelper-linux-x64
```

### Windows
- **Modern Windows (Recommended)**: Run `release/CmdHelper-win-x64.exe`
- **Windows Server 2016+ (Zero-Dep Portable)**: Run `release/CmdHelper-wpf.exe`

### Offline Browser
Open `release/CmdHelper_Web.html` directly in Edge, Chrome, Safari, or Firefox.

---

## 📡 Remote API & Data Source Specification

CmdHelper supports syncing with external endpoints. The API response can be a direct array or wrapped in common backend envelope formats:

```json
[
  {
    "id": "pg_custom_dump",
    "category": "SQL 数据库",
    "title": "PostgreSQL custom format dump",
    "desc": "Backup database or tables with custom compressed format",
    "template": "pg_dump -U {user} -h {host} -p {port} -Fc -d {dbname} -f {output_file}",
    "params": [
      { "key": "user", "label": "Username", "type": "text", "default": "postgres" },
      { "key": "host", "label": "Host", "type": "text", "default": "127.0.0.1" },
      { "key": "port", "label": "Port", "type": "text", "default": "5432" },
      { "key": "dbname", "label": "Database", "type": "text", "default": "demo_db" },
      { "key": "output_file", "label": "Output File", "type": "text", "default": "backup.dump" }
    ],
    "example": "pg_dump -U postgres -h 127.0.0.1 -p 5432 -Fc -d demo_db -f backup.dump",
    "dangerLevel": "normal",
    "tags": ["postgres", "pg_dump", "backup"]
  }
]
```

---

## 🔨 Build & Compile

Ensure you have [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher installed:

```powershell
# Build WPF Lightweight Client & Offline Web Single File
.\build.ps1

# Build Avalonia Cross-Platform Single-File Binaries (Windows EXE & Linux ELF)
.\build-avalonia.ps1
```

---

## ❓ Frequently Asked Questions (FAQ)

#### Q1: Do I need to carry `commands.json` alongside the executable?
**No.** All command databases and templates are embedded into the standalone executable. A single binary file is all you need.

#### Q2: Can I update commands without recompiling the program?
**Yes.** You can:
1. Drop an external `commands.json` or `custom_commands.json` in the same directory.
2. Click **`📡 Sources`** in the UI to configure your internal JSON API endpoint.

#### Q3: Does the single-file HTML version require internet access?
**No.** The HTML file contains zero CDN external references, inline SVG icons, and system native fonts. It operates 100% offline in isolated air-gapped networks.

---

## 🤝 Contributing

Contributions are welcome! Please check out [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on adding new command scenarios and submitting PRs.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
