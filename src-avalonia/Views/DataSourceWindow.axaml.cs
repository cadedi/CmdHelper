using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using LinuxCmdHelper.Models;
using LinuxCmdHelper.Services;

namespace LinuxCmdHelper.Views
{
    public partial class DataSourceWindow : Window
    {
        private readonly CommandRepository _repo;
        private readonly LocalizationService _i18n = LocalizationService.Instance;

        public DataSourceWindow()
        {
            InitializeComponent();
            _repo = new CommandRepository();
        }

        public DataSourceWindow(CommandRepository repo)
        {
            InitializeComponent();
            _repo = repo;
            ApplyI18n();
            RefreshList();
            BindEvents();
        }

        private void ApplyI18n()
        {
            Title = _i18n.Get("DsDialogTitle");
            TxtDlgTitle.Text = "📡 " + _i18n.Get("DsDialogTitle");
            TxtDlgDesc.Text = _i18n.Get("DsDialogDesc");
            TxtListTitle.Text = _i18n.Get("DsListTitle");
            TxtAddHeader.Text = _i18n.Get("DsAddTitle");
            TxtNewName.Watermark = _i18n.Get("DsNamePlaceholder");
            TxtNewUrl.Watermark = _i18n.Get("DsUrlPlaceholder");
            BtnAddSource.Content = _i18n.Get("DsBtnAdd");
            BtnSyncNow.Content = _i18n.Get("DsBtnSyncAll");
            BtnClose.Content = _i18n.Get("DsBtnClose");
        }

        private void BindEvents()
        {
            BtnAddSource.Click += (s, e) =>
            {
                string name = TxtNewName.Text?.Trim() ?? "";
                string url = TxtNewUrl.Text?.Trim() ?? "";
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(url))
                {
                    return;
                }

                string mode = "merge";
                if (CmbMergeMode.SelectedItem is ComboBoxItem item && item.Tag != null)
                {
                    mode = item.Tag.ToString() ?? "merge";
                }

                var ds = new DataSourceConfig
                {
                    Name = name,
                    Url = url,
                    Enabled = true,
                    MergeMode = mode,
                    LastStatus = "就绪"
                };

                _repo.AddDataSource(ds);
                RefreshList();
                TxtNewUrl.Text = "";
            };

            BtnSyncNow.Click += async (s, e) =>
            {
                BtnSyncNow.IsEnabled = false;
                TxtSyncStatus.Text = _i18n.Get("DsStatusSyncing");

                try
                {
                    var result = await _repo.SyncAllDataSourcesAsync();
                    RefreshList();
                    TxtSyncStatus.Text = result.Message;
                }
                catch (Exception ex)
                {
                    TxtSyncStatus.Text = $"Error: {ex.Message}";
                }
                finally
                {
                    BtnSyncNow.IsEnabled = true;
                }
            };

            BtnClose.Click += (s, e) =>
            {
                Close();
            };
        }

        private void RefreshList()
        {
            LstDataSources.ItemsSource = null;
            LstDataSources.ItemsSource = _repo.DataSources.ToList();
            TxtSourceCount.Text = _i18n.Get("DsListCount", _repo.DataSources.Count);
        }

        private void BtnDeleteSource_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string id)
            {
                _repo.RemoveDataSource(id);
                RefreshList();
            }
        }
    }
}
