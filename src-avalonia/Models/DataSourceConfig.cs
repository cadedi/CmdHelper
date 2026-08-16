using System;

namespace LinuxCmdHelper.Models
{
    public class DataSourceConfig
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = "新数据源";
        public string Url { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public string MergeMode { get; set; } = "merge"; // "merge" | "replace"
        public DateTime? LastSyncTime { get; set; }
        public int LoadedCount { get; set; }
        public string LastStatus { get; set; } = "就绪";
    }

    public class SyncResult
    {
        public bool Success { get; set; }
        public int TotalCommands { get; set; }
        public int NewAdded { get; set; }
        public int Updated { get; set; }
        public string Message { get; set; } = "";
    }
}
