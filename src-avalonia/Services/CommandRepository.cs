using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LinuxCmdHelper.Models;

namespace LinuxCmdHelper.Services
{
    public class CommandRepository
    {
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            WriteIndented = true
        };

        private static readonly HttpClient HttpClientInstance;

        static CommandRepository()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            HttpClientInstance = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(8)
            };
            HttpClientInstance.DefaultRequestHeaders.Add("User-Agent", "CmdHelper/2.0 (Cross-Platform; Avalonia)");
        }

        private readonly List<CommandItem> _commands = new();
        private readonly List<DataSourceConfig> _dataSources = new();

        public IReadOnlyList<CommandItem> AllCommands => _commands;
        public IReadOnlyList<DataSourceConfig> DataSources => _dataSources;

        public void Load()
        {
            _commands.Clear();
            LoadDataSourcesConfig();

            // 1. 加载内置/本地基础命令
            var baseCommands = LoadBaseCommands();
            MergeCommands(baseCommands, "merge", out _, out _);

            // 2. 加载本地 custom_commands.json
            string customJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "custom_commands.json");
            if (File.Exists(customJsonPath))
            {
                try
                {
                    string customJson = File.ReadAllText(customJsonPath);
                    var customList = ParseCommandsFromJson(customJson);
                    if (customList != null && customList.Count > 0)
                    {
                        MergeCommands(customList, "merge", out _, out _);
                    }
                }
                catch { }
            }

            // 3. 加载启用的各数据源离线缓存
            foreach (var ds in _dataSources.Where(d => d.Enabled))
            {
                string cacheFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"cache_{ds.Id}.json");
                if (File.Exists(cacheFile))
                {
                    try
                    {
                        string cachedJson = File.ReadAllText(cacheFile);
                        var cachedList = ParseCommandsFromJson(cachedJson);
                        if (cachedList != null && cachedList.Count > 0)
                        {
                            MergeCommands(cachedList, ds.MergeMode, out _, out _);
                            ds.LoadedCount = cachedList.Count;
                        }
                    }
                    catch { }
                }
            }
        }

        public List<CommandItem> Search(string category, string query)
        {
            var list = _commands.AsEnumerable();

            if (!string.IsNullOrEmpty(category) && category != "全部场景" && category != "All Scenarios")
            {
                list = list.Where(c => string.Equals(c.Category, category, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                string q = query.Trim().ToLowerInvariant();
                list = list.Where(c =>
                    (c.Title != null && c.Title.ToLowerInvariant().Contains(q)) ||
                    (c.Desc != null && c.Desc.ToLowerInvariant().Contains(q)) ||
                    (c.Template != null && c.Template.ToLowerInvariant().Contains(q)) ||
                    (c.Tags != null && c.Tags.Any(t => t.ToLowerInvariant().Contains(q)))
                );
            }

            return list.ToList();
        }

        public async Task<SyncResult> SyncAllDataSourcesAsync()
        {
            int totalNew = 0;
            int totalUpdated = 0;
            int successCount = 0;
            var errors = new List<string>();

            // 先重置回基础命令
            _commands.Clear();
            var baseCommands = LoadBaseCommands();
            MergeCommands(baseCommands, "merge", out _, out _);

            foreach (var ds in _dataSources.Where(d => d.Enabled && !string.IsNullOrWhiteSpace(d.Url)))
            {
                try
                {
                    string json = "";
                    if (ds.Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        ds.Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        var response = await HttpClientInstance.GetAsync(ds.Url);
                        response.EnsureSuccessStatusCode();
                        json = await response.Content.ReadAsStringAsync();
                    }
                    else if (File.Exists(ds.Url))
                    {
                        json = File.ReadAllText(ds.Url);
                    }
                    else
                    {
                        throw new FileNotFoundException($"找不到本地数据源文件: {ds.Url}");
                    }

                    var incoming = ParseCommandsFromJson(json);
                    if (incoming != null && incoming.Count > 0)
                    {
                        MergeCommands(incoming, ds.MergeMode, out int added, out int updated);
                        totalNew += added;
                        totalUpdated += updated;
                        ds.LoadedCount = incoming.Count;
                        ds.LastSyncTime = DateTime.Now;
                        ds.LastStatus = $"成功加载 {incoming.Count} 条";
                        successCount++;

                        try
                        {
                            string cacheFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"cache_{ds.Id}.json");
                            File.WriteAllText(cacheFile, json);
                        }
                        catch { }
                    }
                    else
                    {
                        ds.LastStatus = "接口返回内容未解析到有效命令";
                        errors.Add($"{ds.Name}: 未解析到命令数据");
                    }
                }
                catch (Exception ex)
                {
                    ds.LastStatus = $"失败: {ex.Message}";
                    errors.Add($"{ds.Name}: {ex.Message}");
                }
            }

            SaveDataSourcesConfig();

            return new SyncResult
            {
                Success = errors.Count == 0,
                TotalCommands = _commands.Count,
                NewAdded = totalNew,
                Updated = totalUpdated,
                Message = errors.Count == 0
                    ? $"全部同步完成！当前库共 {_commands.Count} 条命令（新增 {totalNew} 条，更新覆盖 {totalUpdated} 条）。"
                    : $"部分数据源同步完成（成功 {successCount} 个，失败 {errors.Count} 个）。\n" + string.Join("\n", errors)
            };
        }

        public void MergeCommands(List<CommandItem> incoming, string mergeMode, out int newAdded, out int updated)
        {
            newAdded = 0;
            updated = 0;

            if (incoming == null || incoming.Count == 0) return;

            if (mergeMode == "replace")
            {
                _commands.Clear();
                _commands.AddRange(incoming);
                newAdded = incoming.Count;
                return;
            }

            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < _commands.Count; i++)
            {
                if (!string.IsNullOrEmpty(_commands[i].Id))
                {
                    map[_commands[i].Id] = i;
                }
            }

            foreach (var item in incoming)
            {
                if (string.IsNullOrEmpty(item.Id))
                {
                    item.Id = Guid.NewGuid().ToString("N");
                }

                if (map.TryGetValue(item.Id, out int existingIndex))
                {
                    _commands[existingIndex] = item;
                    updated++;
                }
                else
                {
                    _commands.Add(item);
                    map[item.Id] = _commands.Count - 1;
                    newAdded++;
                }
            }
        }

        public List<CommandItem> ParseCommandsFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<CommandItem>();

            try
            {
                var directList = JsonSerializer.Deserialize<List<CommandItem>>(json, JsonOpts);
                if (directList != null && directList.Count > 0) return directList;
            }
            catch { }

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<List<CommandItem>>(json, JsonOpts) ?? new List<CommandItem>();
                }
                else if (root.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in new[] { "data", "list", "commands", "items", "result", "rows" })
                    {
                        if (root.TryGetProperty(prop, out var propElem))
                        {
                            if (propElem.ValueKind == JsonValueKind.Array)
                            {
                                return JsonSerializer.Deserialize<List<CommandItem>>(propElem.GetRawText(), JsonOpts) ?? new List<CommandItem>();
                            }
                            else if (propElem.ValueKind == JsonValueKind.Object)
                            {
                                foreach (var innerProp in new[] { "list", "items", "commands", "rows" })
                                {
                                    if (propElem.TryGetProperty(innerProp, out var innerElem) && innerElem.ValueKind == JsonValueKind.Array)
                                    {
                                        return JsonSerializer.Deserialize<List<CommandItem>>(innerElem.GetRawText(), JsonOpts) ?? new List<CommandItem>();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            return new List<CommandItem>();
        }

        private List<CommandItem> LoadBaseCommands()
        {
            // 1. 同目录 commands.json
            string localJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "commands.json");
            if (File.Exists(localJsonPath))
            {
                try
                {
                    string json = File.ReadAllText(localJsonPath);
                    var parsed = ParseCommandsFromJson(json);
                    if (parsed != null && parsed.Count > 0) return parsed;
                }
                catch { }
            }

            // 2. 嵌入资源
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("commands.json", StringComparison.OrdinalIgnoreCase)) ?? "";
                if (!string.IsNullOrEmpty(resourceName))
                {
                    using Stream? stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        using StreamReader reader = new StreamReader(stream);
                        string json = reader.ReadToEnd();
                        var parsed = ParseCommandsFromJson(json);
                        if (parsed != null && parsed.Count > 0) return parsed;
                    }
                }
            }
            catch { }

            return GetFallbackCommands();
        }

        public void LoadDataSourcesConfig()
        {
            _dataSources.Clear();
            string cfgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data_sources.json");
            if (File.Exists(cfgPath))
            {
                try
                {
                    string json = File.ReadAllText(cfgPath);
                    var list = JsonSerializer.Deserialize<List<DataSourceConfig>>(json, JsonOpts);
                    if (list != null && list.Count > 0)
                    {
                        _dataSources.AddRange(list);
                    }
                }
                catch { }
            }
        }

        public void SaveDataSourcesConfig()
        {
            try
            {
                string cfgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data_sources.json");
                string json = JsonSerializer.Serialize(_dataSources, JsonOpts);
                File.WriteAllText(cfgPath, json);
            }
            catch { }
        }

        public void AddDataSource(DataSourceConfig ds)
        {
            _dataSources.Add(ds);
            SaveDataSourcesConfig();
        }

        public void RemoveDataSource(string id)
        {
            _dataSources.RemoveAll(d => d.Id == id);
            try
            {
                string cacheFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"cache_{id}.json");
                if (File.Exists(cacheFile)) File.Delete(cacheFile);
            }
            catch { }
            SaveDataSourcesConfig();
        }

        public string InterpolateCommand(CommandItem cmd, Dictionary<string, object> userParams)
        {
            if (cmd == null || string.IsNullOrEmpty(cmd.Template)) return "";

            string result = cmd.Template;

            var computedContext = new Dictionary<string, object>(userParams);
            if (cmd.Computed != null)
            {
                foreach (var kv in cmd.Computed)
                {
                    string expr = kv.Value;
                    string evaluatedVal = EvaluateSimpleExpression(expr, userParams);
                    computedContext[kv.Key] = evaluatedVal;
                }
            }

            result = Regex.Replace(result, @"\{([a-zA-Z0-9_]+)\}", match =>
            {
                string key = match.Groups[1].Value;
                if (computedContext.TryGetValue(key, out var val) && val != null)
                {
                    return val.ToString() ?? "";
                }
                if (userParams.TryGetValue(key, out var rawVal) && rawVal != null)
                {
                    return rawVal.ToString() ?? "";
                }
                return match.Value;
            });

            return result;
        }

        private string EvaluateSimpleExpression(string expr, Dictionary<string, object> paramValues)
        {
            try
            {
                string clean = expr.Trim().Trim('{', '}').Trim();
                var ternaryMatch = Regex.Match(clean, @"^([a-zA-Z0-9_]+)\s*\?\s*'([^']*)'\s*:\s*'([^']*)'$");
                if (ternaryMatch.Success)
                {
                    string condKey = ternaryMatch.Groups[1].Value;
                    string trueVal = ternaryMatch.Groups[2].Value;
                    string falseVal = ternaryMatch.Groups[3].Value;

                    bool isTrue = false;
                    if (paramValues.TryGetValue(condKey, out var val))
                    {
                        if (val is bool b) isTrue = b;
                        else if (val is string s) isTrue = !string.IsNullOrEmpty(s) && s != "false" && s != "0";
                    }
                    return isTrue ? trueVal : falseVal;
                }
            }
            catch { }

            return "";
        }

        public bool ExportToSingleHtml(string targetFilePath, out string error)
        {
            error = "";
            try
            {
                string templateContent = "";
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("template.html", StringComparison.OrdinalIgnoreCase)) ?? "";
                if (!string.IsNullOrEmpty(resourceName))
                {
                    using Stream? stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        using StreamReader reader = new StreamReader(stream);
                        templateContent = reader.ReadToEnd();
                    }
                }

                if (string.IsNullOrEmpty(templateContent))
                {
                    string webFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CmdHelper_Web.html");
                    if (File.Exists(webFile))
                    {
                        templateContent = File.ReadAllText(webFile);
                    }
                }

                if (string.IsNullOrEmpty(templateContent))
                {
                    error = "未找到 HTML 网页模板资源。";
                    return false;
                }

                string json = JsonSerializer.Serialize(_commands, JsonOpts);
                string pattern = @"const COMMANDS_DATABASE = \[[\s\S]*?\];";
                string replacement = "const COMMANDS_DATABASE = " + json + ";";
                string exportHtml = Regex.Replace(templateContent, pattern, replacement);

                File.WriteAllText(targetFilePath, exportHtml, System.Text.Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private List<CommandItem> GetFallbackCommands()
        {
            return new List<CommandItem>
            {
                new CommandItem
                {
                    Id = "tar_compress",
                    Category = "Linux 运维",
                    Title = "归档并压缩指定文件或目录",
                    Desc = "使用 tar 打包并选用 gzip/bzip2/xz 压缩算法生成压缩归档文件",
                    Template = "tar -c{compress_opt}vf {archive_name} {target_path}",
                    Params = new List<CommandParam>
                    {
                        new CommandParam { Key = "compress_opt", Label = "压缩算法", Type = "select", Default = "z", Options = new List<ParamOption>{ new ParamOption{ Label="gzip (.tar.gz)", Value="z" }, new ParamOption{ Label="bzip2 (.tar.bz2)", Value="j" }, new ParamOption{ Label="xz (.tar.xz)", Value="J" } } },
                        new CommandParam { Key = "archive_name", Label = "输出归档文件名", Type = "text", Default = "app-service.log.tar.gz" },
                        new CommandParam { Key = "target_path", Label = "待压缩的文件或目录", Type = "text", Default = "app-service.log" }
                    },
                    Example = "tar -czvf app-service.log.tar.gz app-service.log"
                }
            };
        }
    }
}
