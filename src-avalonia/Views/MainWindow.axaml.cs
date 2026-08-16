using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using LinuxCmdHelper.Models;
using LinuxCmdHelper.Services;

namespace LinuxCmdHelper.Views
{
    public partial class MainWindow : Window
    {
        private readonly CommandRepository _repo = new();
        private readonly LocalizationService _i18n = LocalizationService.Instance;
        private readonly ThemeService _theme = ThemeService.Instance;

        private CommandItem? _selectedCommand;
        private readonly Dictionary<string, object> _paramValues = new(StringComparer.OrdinalIgnoreCase);
        private readonly DispatcherTimer _toastTimer = new() { Interval = TimeSpan.FromSeconds(2.2) };

        public MainWindow()
        {
            InitializeComponent();
            _toastTimer.Tick += (s, e) =>
            {
                ToastBorder.IsVisible = false;
                _toastTimer.Stop();
            };

            _i18n.LanguageChanged += OnLanguageChanged;
            _theme.ThemeChanged += OnThemeChanged;

            BindEvents();
            LoadData();
        }

        private void LoadData()
        {
            _repo.Load();
            ApplyLanguageTexts();
            UpdateThemeButtonText();
            InitCategories();
            SelectInitialCommand();
        }

        private void BindEvents()
        {
            TxtSearch.TextChanged += (s, e) => RefreshCommandList();
            LstCategories.SelectionChanged += (s, e) => RefreshCommandList();
            LstCommands.SelectionChanged += (s, e) =>
            {
                if (LstCommands.SelectedItem is CommandItem cmd)
                {
                    DisplayCommandDetail(cmd);
                }
            };

            BtnLangToggle.Click += (s, e) =>
            {
                string nextLang = _i18n.CurrentLanguage == "zh-CN" ? "en-US" : "zh-CN";
                _i18n.SetLanguage(nextLang);
                _theme.SaveSettings();
            };

            BtnThemeToggle.Click += (s, e) =>
            {
                _theme.ToggleTheme();
            };

            BtnExportHtml.Click += async (s, e) => await ExportHtmlAsync();
            BtnQuickSync.Click += async (s, e) => await QuickSyncAsync();
            BtnDataSources.Click += async (s, e) => await OpenDataSourcesAsync();
            BtnReset.Click += (s, e) =>
            {
                if (_selectedCommand != null) DisplayCommandDetail(_selectedCommand);
            };

            BtnCopyCommand.Click += async (s, e) => await CopyCommandToClipboardAsync();
            TxtFinalCommand.PointerPressed += async (s, e) => await CopyCommandToClipboardAsync();

            KeyDown += (s, e) =>
            {
                if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.F)
                {
                    TxtSearch.Focus();
                    TxtSearch.SelectAll();
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    if (!string.IsNullOrEmpty(TxtSearch.Text))
                    {
                        TxtSearch.Text = string.Empty;
                        e.Handled = true;
                    }
                }
            };
        }

        private void OnLanguageChanged()
        {
            ApplyLanguageTexts();
            InitCategories();
            RefreshCommandList();
            if (_selectedCommand != null)
            {
                DisplayCommandDetail(_selectedCommand);
            }
        }

        private void OnThemeChanged()
        {
            UpdateThemeButtonText();
        }

        private void ApplyLanguageTexts()
        {
            Title = _i18n.Get("AppTitle") + " - Linux / SQL / Docker / K8s / Git";
            TxtBrandTitle.Text = _i18n.Get("AppTitle");
            TxtSearch.Watermark = _i18n.Get("SearchPlaceholder");
            BtnExportHtml.Content = _i18n.Get("BtnExportWeb");
            BtnQuickSync.Content = _i18n.Get("BtnSync");
            BtnDataSources.Content = _i18n.Get("BtnDataSources");
            BtnReset.Content = _i18n.Get("BtnResetParams");
            BtnLangToggle.Content = _i18n.CurrentLanguage == "zh-CN" ? "🌐 中文 / EN" : "🌐 EN / 中文";
            ToolTip.SetTip(BtnLangToggle, _i18n.CurrentLanguage == "zh-CN" ? "点击切换为英文 (Switch to English)" : "Click to switch to Chinese (切换为中文)");
            TxtCategoryHeader.Text = _i18n.Get("CategorySectionTitle");
            TxtRealtimeTitle.Text = _i18n.Get("RealtimeCommandTitle");
            BtnCopyCommand.Content = _i18n.Get("BtnCopyCommand");
            TxtParamConfigTitle.Text = _i18n.Get("ParamConfigTitle");
            TxtExampleTitle.Text = _i18n.Get("ExampleTitle");
            TxtToastContent.Text = _i18n.Get("ToastCopied");
        }

        private void UpdateThemeButtonText()
        {
            BtnThemeToggle.Content = _theme.CurrentTheme == "Dark" ? "🌙 深色" : "☀️ 浅色";
        }

        private void InitCategories()
        {
            var list = new List<CategoryItemVM>
            {
                new()
                {
                    Name = "全部场景",
                    DisplayName = _i18n.GetCategoryDisplayName("全部场景"),
                    Count = _repo.AllCommands.Count
                }
            };

            var groups = _repo.AllCommands
                .GroupBy(c => c.Category)
                .Select(g => new CategoryItemVM
                {
                    Name = g.Key,
                    DisplayName = _i18n.GetCategoryDisplayName(g.Key),
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count);

            list.AddRange(groups);

            LstCategories.ItemsSource = list;
            if (list.Count > 0)
            {
                LstCategories.SelectedIndex = 0;
            }
        }

        private void SelectInitialCommand()
        {
            RefreshCommandList();
            if (LstCommands.Items.Count > 0)
            {
                LstCommands.SelectedIndex = 0;
            }
        }

        private void RefreshCommandList()
        {
            string cat = (LstCategories.SelectedItem as CategoryItemVM)?.Name ?? "全部场景";
            string search = TxtSearch.Text ?? "";

            var filtered = _repo.Search(cat, search);
            LstCommands.ItemsSource = filtered;

            string displayCat = (LstCategories.SelectedItem as CategoryItemVM)?.DisplayName ?? _i18n.Get("AllScenariosTitle");
            TxtCategoryTitle.Text = displayCat;
            TxtCategoryCount.Text = _i18n.Get("CountSuffix", filtered.Count);

            if (filtered.Count > 0)
            {
                if (_selectedCommand == null || !filtered.Any(c => c.Id == _selectedCommand.Id))
                {
                    LstCommands.SelectedIndex = 0;
                }
                else
                {
                    LstCommands.SelectedItem = filtered.First(c => c.Id == _selectedCommand.Id);
                }
            }
            else
            {
                _selectedCommand = null;
                ClearDetailView();
            }
        }

        private void DisplayCommandDetail(CommandItem cmd)
        {
            _selectedCommand = cmd;
            _paramValues.Clear();

            if (cmd.Params != null)
            {
                foreach (var p in cmd.Params)
                {
                    if (p.Type == "checkbox")
                    {
                        _paramValues[p.Key] = p.GetDefaultBool();
                    }
                    else
                    {
                        _paramValues[p.Key] = p.GetDefaultString();
                    }
                }
            }

            TxtDetailCategory.Text = _i18n.GetCategoryDisplayName(cmd.Category);
            TxtDetailTitle.Text = cmd.Title;
            TxtDetailDesc.Text = cmd.Desc;

            if (cmd.IsDanger)
            {
                BorderDangerWarning.IsVisible = true;
                TxtDangerWarning.Text = _i18n.Get("DangerBadge");
            }
            else if (cmd.IsWarning)
            {
                BorderDangerWarning.IsVisible = true;
                TxtDangerWarning.Text = _i18n.Get("WarningBadge");
            }
            else
            {
                BorderDangerWarning.IsVisible = false;
            }

            TxtExampleBox.Text = !string.IsNullOrEmpty(cmd.Example) ? cmd.Example : cmd.Template;

            BuildDynamicParamControls(cmd);
            UpdateRealtimeCommand();
        }

        private void ClearDetailView()
        {
            TxtDetailCategory.Text = "--";
            TxtDetailTitle.Text = _i18n.Get("EmptyState");
            TxtDetailDesc.Text = "";
            BorderDangerWarning.IsVisible = false;
            TxtFinalCommand.Text = "";
            PnlParamControls.Children.Clear();
            TxtExampleBox.Text = "--";
        }

        private void BuildDynamicParamControls(CommandItem cmd)
        {
            PnlParamControls.Children.Clear();

            if (cmd.Params == null || cmd.Params.Count == 0)
            {
                var noParam = new TextBlock
                {
                    Text = _i18n.Get("NoParamHint"),
                    Classes = { "mutedText" }
                };
                PnlParamControls.Children.Add(noParam);
                return;
            }

            foreach (var p in cmd.Params)
            {
                var container = new StackPanel { Spacing = 6 };

                if (p.Type == "checkbox")
                {
                    var chk = new CheckBox
                    {
                        Content = p.Label,
                        IsChecked = p.GetDefaultBool(),
                        FontSize = 13,
                        Cursor = new Cursor(StandardCursorType.Hand)
                    };
                    chk.IsCheckedChanged += (s, e) =>
                    {
                        _paramValues[p.Key] = chk.IsChecked ?? false;
                        UpdateRealtimeCommand();
                    };
                    container.Children.Add(chk);
                }
                else if (p.Type == "select" && p.Options != null && p.Options.Count > 0)
                {
                    var lbl = new TextBlock
                    {
                        Text = p.Label,
                        Classes = { "paramLabel" }
                    };
                    container.Children.Add(lbl);

                    var cmb = new ComboBox
                    {
                        ItemsSource = p.Options,
                        Height = 40,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                    string defVal = p.GetDefaultString();
                    var matchOpt = p.Options.FirstOrDefault(o => o.Value == defVal) ?? p.Options[0];
                    cmb.SelectedItem = matchOpt;

                    cmb.SelectionChanged += (s, e) =>
                    {
                        if (cmb.SelectedItem is ParamOption opt)
                        {
                            _paramValues[p.Key] = opt.Value;
                            UpdateRealtimeCommand();
                        }
                    };
                    container.Children.Add(cmb);
                }
                else
                {
                    var lbl = new TextBlock
                    {
                        Text = p.Label,
                        Classes = { "paramLabel" }
                    };
                    container.Children.Add(lbl);

                    var txt = new TextBox
                    {
                        Text = p.GetDefaultString(),
                        Watermark = p.Placeholder,
                        Height = 40,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Padding = new Thickness(10, 0),
                        FontFamily = new FontFamily("Cascadia Code, Consolas, Courier New")
                    };

                    txt.TextChanged += (s, e) =>
                    {
                        _paramValues[p.Key] = txt.Text ?? "";
                        UpdateRealtimeCommand();
                    };
                    container.Children.Add(txt);
                }

                PnlParamControls.Children.Add(container);
            }
        }

        private void UpdateRealtimeCommand()
        {
            if (_selectedCommand == null) return;
            string command = _repo.InterpolateCommand(_selectedCommand, _paramValues);
            TxtFinalCommand.Text = command;
        }

        private async Task CopyCommandToClipboardAsync()
        {
            string text = TxtFinalCommand.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(text) || text == "--") return;

            if (Clipboard != null)
            {
                await Clipboard.SetTextAsync(text);
                ShowToast();
            }
        }

        private void ShowToast()
        {
            ToastBorder.IsVisible = true;
            _toastTimer.Stop();
            _toastTimer.Start();
        }

        private async Task ExportHtmlAsync()
        {
            var topLevel = GetTopLevel(this);
            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = _i18n.Get("BtnExportWeb"),
                DefaultExtension = ".html",
                SuggestedFileName = "CmdHelper_Web.html",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("HTML Page") { Patterns = new[] { "*.html" } }
                }
            });

            if (file != null)
            {
                string localPath = file.Path.LocalPath;
                bool ok = _repo.ExportToSingleHtml(localPath, out string err);
                if (ok)
                {
                    ShowToast();
                }
            }
        }

        private async Task QuickSyncAsync()
        {
            BtnQuickSync.IsEnabled = false;
            try
            {
                var res = await _repo.SyncAllDataSourcesAsync();
                InitCategories();
                RefreshCommandList();
                ShowToast();
            }
            finally
            {
                BtnQuickSync.IsEnabled = true;
            }
        }

        private async Task OpenDataSourcesAsync()
        {
            var dialog = new DataSourceWindow(_repo);
            await dialog.ShowDialog(this);
            InitCategories();
            RefreshCommandList();
        }
    }
}
