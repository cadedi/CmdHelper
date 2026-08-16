using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LinuxCmdHelper.Models;
using LinuxCmdHelper.Services;

namespace LinuxCmdHelper
{
    public partial class DataSourceWindow : Window
    {
        private readonly CommandRepository _repo;
        public bool DataChanged { get; private set; } = false;

        public DataSourceWindow(CommandRepository repo)
        {
            InitializeComponent();
            _repo = repo;
            RefreshList();
        }

        private void RefreshList()
        {
            LstDataSources.ItemsSource = null;
            LstDataSources.ItemsSource = _repo.DataSources.ToList();
            TxtSourceCount.Text = $"共 {_repo.DataSources.Count} 个数据源";
        }

        private void BtnAddSource_Click(object sender, RoutedEventArgs e)
        {
            string name = TxtNewName.Text.Trim();
            string url = TxtNewUrl.Text.Trim();
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(url))
            {
                MessageBox.Show(this, "请输入数据源名称和接口 URL/文件路径。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string mode = "merge";
            if (CmbMergeMode.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                mode = item.Tag.ToString();
            }

            var ds = new DataSourceConfig
            {
                Name = name,
                Url = url,
                Enabled = true,
                MergeMode = mode,
                LastStatus = "未同步"
            };

            _repo.AddDataSource(ds);
            DataChanged = true;
            RefreshList();

            TxtNewName.Text = "新数据源";
            TxtNewUrl.Text = "";
        }

        private void BtnDeleteSource_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string id)
            {
                var found = _repo.DataSources.FirstOrDefault(d => d.Id == id);
                if (found != null)
                {
                    var res = MessageBox.Show(this, $"确定要删除数据源【{found.Name}】吗？", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        _repo.RemoveDataSource(id);
                        DataChanged = true;
                        RefreshList();
                    }
                }
            }
        }

        private void ChkSourceEnabled_Changed(object sender, RoutedEventArgs e)
        {
            _repo.SaveDataSourcesConfig();
            DataChanged = true;
        }

        private async void BtnSyncNow_Click(object sender, RoutedEventArgs e)
        {
            BtnSyncNow.IsEnabled = false;
            TxtSyncStatus.Text = "正在同步拉取所有启用的数据源，请稍候...";

            try
            {
                var result = await _repo.SyncAllDataSourcesAsync();
                DataChanged = true;
                RefreshList();
                TxtSyncStatus.Text = result.Message;
                MessageBox.Show(this, result.Message, result.Success ? "同步成功" : "同步完成(含错误)", MessageBoxButton.OK, result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                TxtSyncStatus.Text = $"同步异常: {ex.Message}";
                MessageBox.Show(this, $"同步过程中发生异常:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnSyncNow.IsEnabled = true;
            }
        }

        private void LstDataSources_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = DataChanged;
            Close();
        }
    }
}
