using System;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using LinuxCmdHelper.Models;

namespace LinuxCmdHelper.Services
{
    public class ThemeService
    {
        public static ThemeService Instance { get; } = new ThemeService();

        public event Action? ThemeChanged;

        public string CurrentTheme { get; private set; } = "Light"; // "Light" | "Dark"

        private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

        public void Initialize()
        {
            var settings = LoadSettings();
            CurrentTheme = settings.Theme == "Dark" ? "Dark" : "Light";
            LocalizationService.Instance.SetLanguage(settings.Language);
            ApplyTheme(CurrentTheme, false);
        }

        public void ToggleTheme()
        {
            SetTheme(CurrentTheme == "Light" ? "Dark" : "Light");
        }

        public void SetTheme(string theme)
        {
            CurrentTheme = theme == "Dark" ? "Dark" : "Light";
            ApplyTheme(CurrentTheme, true);
            ThemeChanged?.Invoke();
            SaveSettings();
        }

        private void ApplyTheme(string theme, bool notify)
        {
            if (Application.Current != null)
            {
                Application.Current.RequestedThemeVariant = theme == "Dark" 
                    ? ThemeVariant.Dark 
                    : ThemeVariant.Light;
            }
        }

        public AppSettings LoadSettings()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_settings.json");
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null) return settings;
                }
            }
            catch { }
            return new AppSettings { Language = "zh-CN", Theme = "Light" };
        }

        public void SaveSettings()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_settings.json");
                var settings = new AppSettings
                {
                    Language = LocalizationService.Instance.CurrentLanguage,
                    Theme = CurrentTheme
                };
                string json = JsonSerializer.Serialize(settings, JsonOpts);
                File.WriteAllText(path, json);
            }
            catch { }
        }
    }
}
