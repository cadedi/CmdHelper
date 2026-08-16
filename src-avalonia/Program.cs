using System;
using Avalonia;

namespace LinuxCmdHelper
{
    internal sealed class Program
    {
        // 应用程序跨平台主入口点
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia 配置构建器
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
