using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LinuxCmdHelper.Models
{
    public class ParamOption
    {
        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class CommandParam
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = "text"; // "text", "select", "checkbox"

        [JsonPropertyName("default")]
        public object? Default { get; set; }

        [JsonPropertyName("placeholder")]
        public string Placeholder { get; set; } = string.Empty;

        [JsonPropertyName("options")]
        public List<ParamOption>? Options { get; set; }

        // 获取字符串默认值
        public string GetDefaultString()
        {
            if (Default == null) return string.Empty;
            return Default.ToString() ?? string.Empty;
        }

        // 获取布尔默认值
        public bool GetDefaultBool()
        {
            if (Default == null) return false;
            if (Default is bool b) return b;
            if (bool.TryParse(Default.ToString(), out bool parsed)) return parsed;
            return false;
        }
    }

    public class CommandItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("category")]
        public string Category { get; set; } = "常用操作";

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("desc")]
        public string Desc { get; set; } = string.Empty;

        [JsonPropertyName("template")]
        public string Template { get; set; } = string.Empty;

        [JsonPropertyName("params")]
        public List<CommandParam> Params { get; set; } = new List<CommandParam>();

        [JsonPropertyName("computed")]
        public Dictionary<string, string>? Computed { get; set; }

        [JsonPropertyName("example")]
        public string Example { get; set; } = string.Empty;

        [JsonPropertyName("dangerLevel")]
        public string DangerLevel { get; set; } = "normal"; // "normal", "warning", "danger"

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new List<string>();

        // 用于 UI 绑定的辅助属性
        [JsonIgnore]
        public string DangerBadgeText => DangerLevel switch
        {
            "danger" => "高危",
            "warning" => "注意",
            _ => ""
        };

        [JsonIgnore]
        public bool HasDangerBadge => !string.IsNullOrEmpty(DangerBadgeText);
    }
}
