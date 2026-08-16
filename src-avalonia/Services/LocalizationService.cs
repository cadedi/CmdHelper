using System;
using System.Collections.Generic;

namespace LinuxCmdHelper.Services
{
    public class LocalizationService
    {
        public static LocalizationService Instance { get; } = new LocalizationService();

        public event Action? LanguageChanged;

        public string CurrentLanguage { get; private set; } = "zh-CN";

        private readonly Dictionary<string, Dictionary<string, string>> _strings = new()
        {
            ["zh-CN"] = new Dictionary<string, string>
            {
                ["AppTitle"] = "极速命令助手",
                ["SearchPlaceholder"] = "搜索需求 (如: 归档压缩, 慢查询, k8s pod, 端口占用, 索引优化...)",
                ["BtnExportWeb"] = "📤 导出 Web 版",
                ["BtnSync"] = "🔄 刷新/同步",
                ["BtnDataSources"] = "📡 数据源配置",
                ["BtnResetParams"] = "重置参数",
                ["CategorySectionTitle"] = "场景分类",
                ["AllCategories"] = "全部场景",
                ["AllScenariosTitle"] = "全部需求场景",
                ["CountSuffix"] = "共 {0} 条",
                ["EmptyState"] = "没有匹配到相关需求命令",
                ["RealtimeCommandTitle"] = "实时拼接命令 (点击一键复制)",
                ["BtnCopyCommand"] = "📋 复制完整命令",
                ["ParamConfigTitle"] = "参数动态设置 (修改后实时更新上方命令)",
                ["ExampleTitle"] = "标准示例 / 场景说明",
                ["NoParamHint"] = "该命令无需额外参数，直接复制即可使用。",
                ["ToastCopied"] = "✔ 已成功复制命令到剪贴板！",
                ["DangerBadge"] = "危险操作",
                ["WarningBadge"] = "修改操作",
                ["ThemeLight"] = "浅色",
                ["ThemeDark"] = "深色",
                ["LangZh"] = "中文",
                ["LangEn"] = "English",

                // 分类名称映射
                ["Cat_Linux 运维"] = "Linux 运维",
                ["Cat_Java 性能与 JVM"] = "Java 性能与 JVM",
                ["Cat_SQL 数据库"] = "SQL 数据库",
                ["Cat_Docker 容器"] = "Docker 容器",
                ["Cat_Kubernetes (K8s)"] = "Kubernetes (K8s)",
                ["Cat_Git 版本控制"] = "Git 版本控制",
                ["Cat_Redis 缓存"] = "Redis 缓存",
                ["Cat_Nginx & 安全证书"] = "Nginx & 安全证书",
                ["Cat_Windows & PowerShell"] = "Windows & PowerShell",

                // 数据源弹窗
                ["DsDialogTitle"] = "数据源管理与远程接口同步",
                ["DsDialogDesc"] = "支持配置多个远程 HTTP/HTTPS 接口或本地 JSON 文件。可选用【智能去重合并 (按 ID 覆盖旧项)】或【完全替代】，支持离线断网自动缓存。",
                ["DsListTitle"] = "已配置的数据源列表",
                ["DsListCount"] = "共 {0} 个数据源",
                ["DsAddTitle"] = "添加新的数据源 / 接口",
                ["DsNamePlaceholder"] = "数据源名称",
                ["DsUrlPlaceholder"] = "支持 HTTP/HTTPS 接口 URL 或 本地 .json 文件绝对路径",
                ["DsBtnAdd"] = "➕ 添加数据源",
                ["DsBtnSyncAll"] = "🔄 立即测试并同步全部数据源",
                ["DsBtnClose"] = "完成并关闭",
                ["DsDelete"] = "删除",
                ["DsMergeMode"] = "智能去重合并 (merge)",
                ["DsReplaceMode"] = "完全覆盖替代 (replace)",
                ["DsStatusSyncing"] = "正在同步拉取所有启用的数据源，请稍候...",
                ["ExportSuccessTitle"] = "导出成功",
                ["ExportSuccessMsg"] = "已成功导出单文件 Web 版到：\n{0}\n\n该 HTML 可直接在 Linux/macOS 任意浏览器中离线打开。\n\n是否立即在默认浏览器中打开预览？"
            },
            ["en-US"] = new Dictionary<string, string>
            {
                ["AppTitle"] = "Command Helper",
                ["SearchPlaceholder"] = "Search scenarios (e.g. compress, slow query, k8s, port...)",
                ["BtnExportWeb"] = "📤 Export Web",
                ["BtnSync"] = "🔄 Sync",
                ["BtnDataSources"] = "📡 Sources",
                ["BtnResetParams"] = "Reset",
                ["CategorySectionTitle"] = "Categories",
                ["AllCategories"] = "All Scenarios",
                ["AllScenariosTitle"] = "All Command Scenarios",
                ["CountSuffix"] = "{0} items",
                ["EmptyState"] = "No matching command scenarios found",
                ["RealtimeCommandTitle"] = "Live Interpolated Command (Click to copy)",
                ["BtnCopyCommand"] = "📋 Copy Full Command",
                ["ParamConfigTitle"] = "Dynamic Parameter Settings (Live update)",
                ["ExampleTitle"] = "Standard Example / Notes",
                ["NoParamHint"] = "This command requires no additional parameters, ready to copy.",
                ["ToastCopied"] = "✔ Command copied to clipboard successfully!",
                ["DangerBadge"] = "Danger",
                ["WarningBadge"] = "Notice",
                ["ThemeLight"] = "Light",
                ["ThemeDark"] = "Dark",
                ["LangZh"] = "中文",
                ["LangEn"] = "English",

                // 分类名称映射
                ["Cat_Linux 运维"] = "Linux Operations",
                ["Cat_Java 性能与 JVM"] = "Java & JVM Profiling",
                ["Cat_SQL 数据库"] = "SQL Databases",
                ["Cat_Docker 容器"] = "Docker Containers",
                ["Cat_Kubernetes (K8s)"] = "Kubernetes (K8s)",
                ["Cat_Git 版本控制"] = "Git VCS",
                ["Cat_Redis 缓存"] = "Redis Caching",
                ["Cat_Nginx & 安全证书"] = "Nginx & SSL/TLS",
                ["Cat_Windows & PowerShell"] = "Windows & PowerShell",

                // 数据源弹窗
                ["DsDialogTitle"] = "Data Sources & Remote API Sync",
                ["DsDialogDesc"] = "Configure multiple remote HTTP/HTTPS API endpoints or local JSON files. Supports intelligent merge (deduplicate by ID) or full override, with offline auto-caching.",
                ["DsListTitle"] = "Configured Data Sources",
                ["DsListCount"] = "{0} data source(s)",
                ["DsAddTitle"] = "Add New Data Source / Endpoint",
                ["DsNamePlaceholder"] = "Data source name",
                ["DsUrlPlaceholder"] = "HTTP/HTTPS API URL or local .json file path",
                ["DsBtnAdd"] = "➕ Add Source",
                ["DsBtnSyncAll"] = "🔄 Test & Sync All Sources Now",
                ["DsBtnClose"] = "Done & Close",
                ["DsDelete"] = "Delete",
                ["DsMergeMode"] = "Smart Merge (by ID)",
                ["DsReplaceMode"] = "Full Replace (replace)",
                ["DsStatusSyncing"] = "Syncing all enabled data sources, please wait...",
                ["ExportSuccessTitle"] = "Export Successful",
                ["ExportSuccessMsg"] = "Successfully exported single-file Web version to:\n{0}\n\nThis HTML can be opened offline in any browser on Linux/macOS.\n\nOpen in browser now?"
            }
        };

        public void SetLanguage(string lang)
        {
            if (lang != "zh-CN" && lang != "en-US") lang = "zh-CN";
            CurrentLanguage = lang;
            LanguageChanged?.Invoke();
        }

        public string Get(string key, params object[] args)
        {
            if (_strings.TryGetValue(CurrentLanguage, out var dict) && dict.TryGetValue(key, out var val))
            {
                return args.Length > 0 ? string.Format(val, args) : val;
            }
            if (_strings["zh-CN"].TryGetValue(key, out var fallbackVal))
            {
                return args.Length > 0 ? string.Format(fallbackVal, args) : fallbackVal;
            }
            return key;
        }

        public string GetCategoryDisplayName(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName)) return "";
            if (categoryName == "全部场景" || categoryName == "All Scenarios")
            {
                return Get("AllCategories");
            }
            return Get($"Cat_{categoryName}");
        }
    }
}
