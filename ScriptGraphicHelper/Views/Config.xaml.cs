using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Newtonsoft.Json;
using ScriptGraphicHelper.Models;
using System;
using System.IO;

namespace ScriptGraphicHelper.Views
{
    public class Config : Window
    {
        public Config()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void Window_Opened(object sender, EventArgs e)
        {
            var addRange = this.FindControl<ToggleSwitch>("AddRange");
            var addInfo = this.FindControl<ToggleSwitch>("AddInfo");
            var isOffset = this.FindControl<ToggleSwitch>("IsOffset");
            var diySim = this.FindControl<TextBox>("DiySim");
            var ysPath = this.FindControl<TextBox>("YsPath");
            var xyPath = this.FindControl<TextBox>("XyPath");
            var dmRegcode = this.FindControl<TextBox>("DmRegcode");
            addRange.IsChecked = Setting.Instance.AddRange;
            addInfo.IsChecked = Setting.Instance.AddInfo;
            isOffset.IsChecked = Setting.Instance.IsOffset;
            diySim.Text = Setting.Instance.DiySim.ToString();
            ysPath.Text = Setting.Instance.YsPath;
            xyPath.Text = Setting.Instance.XyPath;
            dmRegcode.Text = Setting.Instance.DmRegcode;
        }

        private void Ok_Tapped(object sender, RoutedEventArgs e)
        {
            try
            {
                var addRange = this.FindControl<ToggleSwitch>("AddRange");
                var addInfo = this.FindControl<ToggleSwitch>("AddInfo");
                var isOffset = this.FindControl<ToggleSwitch>("IsOffset");
                var diySim = this.FindControl<TextBox>("DiySim");
                var ysPath = this.FindControl<TextBox>("YsPath");
                var xyPath = this.FindControl<TextBox>("XyPath");
                var dmRegcode = this.FindControl<TextBox>("DmRegcode");
                Setting.Instance.AddRange = addRange.IsChecked ?? false;
                Setting.Instance.AddInfo = addInfo.IsChecked ?? false;
                Setting.Instance.IsOffset = isOffset.IsChecked ?? false;

                if (int.TryParse(diySim.Text.Trim(), out var sim))
                {
                    Setting.Instance.DiySim = sim;
                }

                Setting.Instance.YsPath = ysPath.Text ?? string.Empty;
                Setting.Instance.XyPath = xyPath.Text ?? string.Empty;
                Setting.Instance.DmRegcode = dmRegcode.Text ?? string.Empty;
                var settingStr = JsonConvert.SerializeObject(Setting.Instance, Formatting.Indented);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"Assets\setting.json", settingStr);
                Close();
            }
            catch { }

        }

    }
}
