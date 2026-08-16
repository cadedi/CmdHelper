# Contributing to CmdHelper

Thank you for your interest in contributing to **CmdHelper**! We welcome bug reports, feature requests, command additions, and pull requests.

---

## 🧭 Code of Conduct

Please maintain a respectful, welcoming, and inclusive atmosphere when participating in discussions, reporting issues, or submitting PRs.

---

## 🛠️ How to Contribute

### 1. Adding New Commands / Scenarios (`commands.json`)

To add or update command scenarios in `data/commands.json`, ensure each entry follows the standard JSON schema:

```json
{
  "id": "unique_command_id",
  "category": "Linux 运维",
  "title": "Clear description of the problem solved",
  "desc": "Detailed explanation of what the command does and tips",
  "template": "command_name --flag={param_key} {another_param}",
  "params": [
    {
      "key": "param_key",
      "label": "Parameter Name",
      "type": "text",
      "default": "example_value",
      "placeholder": "e.g., 192.168.1.100"
    },
    {
      "key": "another_param",
      "label": "Option Type",
      "type": "select",
      "default": "fast",
      "options": [
        { "label": "Fast Mode", "value": "fast" },
        { "label": "Deep Mode", "value": "deep" }
      ]
    }
  ],
  "example": "command_name --flag=example_value fast",
  "dangerLevel": "normal",
  "tags": ["network", "tcp", "port"]
}
```

> **Important**: Never include sensitive company data, internal IPs, private tokens, or proprietary project names. Use sanitized generic placeholders such as `192.168.1.100`, `demo_db`, or `app-service.log`.

---

### 2. Development & Building

#### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher
- PowerShell 7+ (pwsh) or Windows PowerShell

#### Build All Targets
```powershell
# Build WPF Lightweight Client & Offline Web Single File
.\build.ps1

# Build Avalonia Cross-Platform Single-File Binaries (Win / Linux ELF)
.\build-avalonia.ps1
```

---

## 🔀 Submitting Pull Requests

1. **Fork** the repository and create your branch from `main` (e.g., `feat/add-redis-cluster-cmds`).
2. Make changes and verify both English and Chinese localizations if adding UI features.
3. Test compilation with `build.ps1` and `build-avalonia.ps1`.
4. Submit a **Pull Request** describing the motivation and changes made.
