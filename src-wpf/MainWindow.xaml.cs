using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LinuxCmdHelper.Models;
using LinuxCmdHelper.Services;

namespace LinuxCmdHelper
{
    public class CategoryItemVM
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public partial class MainWindow : Window
    {
        private readonly CommandRepository _repo = new CommandRepository();
        private CommandItem? _selectedCommand;
        private readonly Dictionary<string, object> _paramValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private readonly DispatcherTimer _toastTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            _toastTimer.Interval = TimeSpan.FromSeconds(2);
            _toastTimer.Tick += (s, e) =>
            {
                _toastTimer.Stop();
                ToastBorder.Visibility = Visibility.Collapsed;
            };

            Loaded += MainWindow_Loaded;
            KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _repo.Load();
            InitCategories();
            SelectInitialCommand();
        }

        private void InitCategories()
        {
            var list = new List<CategoryItemVM>
            {
                new CategoryItemVM { Name = "全部场景", Count = _repo.AllCommands.Count }
            };

            var groups = _repo.AllCommands
                .GroupBy(c => c.Category)
                .Select(g => new CategoryItemVM { Name = g.Key, Count = g.Count() })
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
            string search = TxtSearch.Text;

            var filtered = _repo.Search(cat, search);
            LstCommands.ItemsSource = filtered;

            TxtCategoryTitle.Text = cat == "全部场景" ? "全部需求场景" : cat;
            TxtCategoryCount.Text = $"共 {filtered.Count} 条";

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

        private void LstCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshCommandList();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            TxtSearchPlaceholder.Visibility = string.IsNullOrEmpty(TxtSearch.Text) ? Visibility.Visible : Visibility.Collapsed;
            RefreshCommandList();
        }

        private void LstCommands_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstCommands.SelectedItem is CommandItem cmd)
            {
                DisplayCommandDetail(cmd);
            }
        }

        private void DisplayCommandDetail(CommandItem cmd)
        {
            _selectedCommand = cmd;
            _paramValues.Clear();

            // 填充默认值
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

            // 更新头部
            TxtDetailCategory.Text = cmd.Category;
            TxtDetailTitle.Text = cmd.Title;
            TxtDetailDesc.Text = cmd.Desc;

            if (cmd.DangerLevel == "danger")
            {
                BorderDangerWarning.Visibility = Visibility.Visible;
                TxtDangerWarning.Text = "高危操作：请务必核对参数后再执行";
            }
            else if (cmd.DangerLevel == "warning")
            {
                BorderDangerWarning.Visibility = Visibility.Visible;
                TxtDangerWarning.Text = "注意：涉及文件修改或服务变更";
            }
            else
            {
                BorderDangerWarning.Visibility = Visibility.Collapsed;
            }

            TxtExampleBox.Text = string.IsNullOrEmpty(cmd.Example) ? cmd.Template : cmd.Example;

            // 动态生成参数输入控件
            GenerateParamControls(cmd);

            // 实时计算命令
            UpdateFinalCommandText();
        }

        private void GenerateParamControls(CommandItem cmd)
        {
            PnlParamControls.Children.Clear();

            if (cmd.Params == null || cmd.Params.Count == 0)
            {
                var emptyTip = new TextBlock
                {
                    Text = "该需求命令无需额外参数，可直接一键复制使用。",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280")),
                    FontSize = 13,
                    Margin = new Thickness(0, 4, 0, 4)
                };
                PnlParamControls.Children.Add(emptyTip);
                return;
            }

            foreach (var p in cmd.Params)
            {
                var rowContainer = new StackPanel
                {
                    Margin = new Thickness(0, 0, 0, 14)
                };

                if (p.Type == "select")
                {
                    var lbl = new TextBlock
                    {
                        Text = p.Label,
                        FontSize = 12,
                        FontWeight = FontWeights.Medium,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4B5563")),
                        Margin = new Thickness(0, 0, 0, 6)
                    };
                    rowContainer.Children.Add(lbl);

                    var cmb = new ComboBox
                    {
                        Height = 40,
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")),
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111827")),
                        BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1D5DB")),
                        Padding = new Thickness(10, 0, 10, 0),
                        VerticalContentAlignment = VerticalAlignment.Center,
                        FontSize = 13
                    };

                    if (p.Options != null)
                    {
                        foreach (var opt in p.Options)
                        {
                            var item = new ComboBoxItem
                            {
                                Content = opt.Label,
                                Tag = opt.Value
                            };
                            cmb.Items.Add(item);

                            if (opt.Value == p.GetDefaultString())
                            {
                                cmb.SelectedItem = item;
                            }
                        }

                        if (cmb.SelectedIndex == -1 && cmb.Items.Count > 0)
                        {
                            cmb.SelectedIndex = 0;
                        }
                    }

                    string pKey = p.Key;
                    cmb.SelectionChanged += (s, e) =>
                    {
                        if (cmb.SelectedItem is ComboBoxItem selectedItem)
                        {
                            _paramValues[pKey] = selectedItem.Tag?.ToString() ?? "";
                            UpdateFinalCommandText();
                        }
                    };

                    rowContainer.Children.Add(cmb);
                }
                else if (p.Type == "checkbox")
                {
                    var chk = new CheckBox
                    {
                        Content = p.Label,
                        FontSize = 13,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2937")),
                        IsChecked = p.GetDefaultBool(),
                        Margin = new Thickness(0, 4, 0, 4)
                    };

                    string pKey = p.Key;
                    chk.Checked += (s, e) =>
                    {
                        _paramValues[pKey] = true;
                        UpdateFinalCommandText();
                    };
                    chk.Unchecked += (s, e) =>
                    {
                        _paramValues[pKey] = false;
                        UpdateFinalCommandText();
                    };

                    rowContainer.Children.Add(chk);
                }
                else // text
                {
                    var lbl = new TextBlock
                    {
                        Text = p.Label,
                        FontSize = 12,
                        FontWeight = FontWeights.Medium,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4B5563")),
                        Margin = new Thickness(0, 0, 0, 6)
                    };
                    rowContainer.Children.Add(lbl);

                    var txt = new TextBox
                    {
                        Text = p.GetDefaultString(),
                        Style = (Style)FindResource("AppTextBoxStyle"),
                        Height = 40,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                    string pKey = p.Key;
                    txt.TextChanged += (s, e) =>
                    {
                        _paramValues[pKey] = txt.Text;
                        UpdateFinalCommandText();
                    };

                    rowContainer.Children.Add(txt);
                }

                PnlParamControls.Children.Add(rowContainer);
            }
        }

        private void UpdateFinalCommandText()
        {
            if (_selectedCommand == null) return;
            string finalCmd = _repo.InterpolateCommand(_selectedCommand, _paramValues);
            TxtFinalCommand.Text = finalCmd;
        }

        private void ClearDetailView()
        {
            TxtDetailCategory.Text = "--";
            TxtDetailTitle.Text = "未找到匹配的需求命令";
            TxtDetailDesc.Text = "请调整左侧分类或更改顶部搜索关键词。";
            TxtFinalCommand.Text = "--";
            TxtExampleBox.Text = "--";
            BorderDangerWarning.Visibility = Visibility.Collapsed;
            PnlParamControls.Children.Clear();
        }

        private void BtnCopyCommand_Click(object sender, RoutedEventArgs e)
        {
            PerformCopy();
        }

        private void TxtFinalCommand_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PerformCopy();
        }

        private void PerformCopy()
        {
            string text = TxtFinalCommand.Text;
            if (string.IsNullOrWhiteSpace(text) || text == "--") return;

            try
            {
                Clipboard.SetText(text);
                ShowToast();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"复制到剪贴板失败: {ex.Message}", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ShowToast()
        {
            ToastBorder.Visibility = Visibility.Visible;
            _toastTimer.Stop();
            _toastTimer.Start();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCommand != null)
            {
                DisplayCommandDetail(_selectedCommand);
            }
        }

        private void BtnDataSources_Click(object sender, RoutedEventArgs e)
        {
            var win = new DataSourceWindow(_repo)
            {
                Owner = this
            };
            if (win.ShowDialog() == true)
            {
                InitCategories();
                RefreshCommandList();
            }
        }

        private async void BtnQuickSync_Click(object sender, RoutedEventArgs e)
        {
            BtnQuickSync.IsEnabled = false;
            try
            {
                var res = await _repo.SyncAllDataSourcesAsync();
                InitCategories();
                RefreshCommandList();
                ShowToast();
                MessageBox.Show(this, res.Message, res.Success ? "同步成功" : "同步完成(含警告)", MessageBoxButton.OK, res.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"同步失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnQuickSync.IsEnabled = true;
            }
        }

        private void BtnExportHtml_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "导出单文件 Web 版",
                Filter = "HTML 网页 (*.html)|*.html",
                FileName = "CmdHelper_Web.html",
                DefaultExt = ".html"
            };

            if (dialog.ShowDialog() == true)
            {
                bool success = _repo.ExportToSingleHtml(dialog.FileName, out string err);
                if (success)
                {
                    ShowToast();
                    var openRes = MessageBox.Show(this, $"已成功导出单文件 Web 版到：\n{dialog.FileName}\n\n该 HTML 可直接发送给 Linux/macOS 同事或在任何浏览器中离线打开。\n\n是否立即在默认浏览器中打开预览？", "导出成功", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (openRes == MessageBoxResult.Yes)
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = dialog.FileName,
                                UseShellExecute = true
                            });
                        }
                        catch { }
                    }
                }
                else
                {
                    MessageBox.Show(this, $"导出失败:\n{err}", "导出错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.F)
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
        }
    }
}