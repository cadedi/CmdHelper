using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LinuxCmdHelper.Models
{
    public class CommandItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("category")]
        public string Category { get; set; } = "常规";

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("desc")]
        public string Desc { get; set; } = "";

        [JsonPropertyName("template")]
        public string Template { get; set; } = "";

        [JsonPropertyName("params")]
        public List<CommandParam> Params { get; set; } = new List<CommandParam>();

        [JsonPropertyName("computed")]
        public Dictionary<string, string>? Computed { get; set; }

        [JsonPropertyName("example")]
        public string Example { get; set; } = "";

        [JsonPropertyName("dangerLevel")]
        public string DangerLevel { get; set; } = "normal";

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new List<string>();

        // VM 辅助属性
        [JsonIgnore]
        public bool IsDanger => DangerLevel == "danger";

        [JsonIgnore]
        public bool IsWarning => DangerLevel == "warning";

        [JsonIgnore]
        public string DisplayPreview => !string.IsNullOrEmpty(Example) ? Example : Template;
    }

    public class CommandParam
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = "";

        [JsonPropertyName("label")]
        public string Label { get; set; } = "";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "text"; // "text", "select", "checkbox"

        [JsonPropertyName("default")]
        public object? Default { get; set; }

        [JsonPropertyName("placeholder")]
        public string Placeholder { get; set; } = "";

        [JsonPropertyName("options")]
        public List<ParamOption>? Options { get; set; }

        public string GetDefaultString()
        {
            if (Default == null) return "";
            if (Default is System.Text.Json.JsonElement element)
            {
                if (element.ValueKind == System.Text.Json.JsonValueKind.String) return element.GetString() ?? "";
                if (element.ValueKind == System.Text.Json.JsonValueKind.Number) return element.GetRawText();
                if (element.ValueKind == System.Text.Json.JsonValueKind.True) return "true";
                if (element.ValueKind == System.Text.Json.JsonValueKind.False) return "false";
                return element.ToString();
            }
            return Default.ToString() ?? "";
        }

        public bool GetDefaultBool()
        {
            if (Default == null) return false;
            if (Default is bool b) return b;
            if (Default is System.Text.Json.JsonElement element)
            {
                if (element.ValueKind == System.Text.Json.JsonValueKind.True) return true;
                if (element.ValueKind == System.Text.Json.JsonValueKind.False) return false;
            }
            return string.Equals(Default.ToString(), "true", System.StringComparison.OrdinalIgnoreCase);
        }
    }

    public class ParamOption
    {
        [JsonPropertyName("label")]
        public string Label { get; set; } = "";

        [JsonPropertyName("value")]
        public string Value { get; set; } = "";

        public override string ToString() => Label;
    }

    public class CategoryItemVM
    {
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public int Count { get; set; }
    }
}
