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
        }

        private void Window_Opened(object sender, EventArgs e)
        {
            var addRange = this.FindControl<ToggleSwitch>("AddRange");
            var isOffset = this.FindControl<ToggleSwitch>("IsOffset");
            var diySim = this.FindControl<TextBox>("DiySim");
            var ysPath = this.FindControl<TextBox>("YsPath");
            var xyPath = this.FindControl<TextBox>("XyPath");
            var dmRegcode = this.FindControl<TextBox>("DmRegcode");
            addRange.IsChecked = Setting.Instance.AddRange;
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
                var isOffset = this.FindControl<ToggleSwitch>("IsOffset");
                var diySim = this.FindControl<TextBox>("DiySim");
                var ysPath = this.FindControl<TextBox>("YsPath");
                var xyPath = this.FindControl<TextBox>("XyPath");
                var dmRegcode = this.FindControl<TextBox>("DmRegcode");
                Setting.Instance.AddRange = (bool)addRange.IsChecked;
                Setting.Instance.IsOffset = (bool)isOffset.IsChecked;
                int result = 95;
                int.TryParse(diySim.Text.Trim(), out result);
                Setting.Instance.DiySim = result;
                Setting.Instance.YsPath = ysPath.Text ?? string.Empty;
                Setting.Instance.XyPath = xyPath.Text ?? string.Empty;
                Setting.Instance.DmRegcode = dmRegcode.Text ?? string.Empty;

                string settingStr = JsonConvert.SerializeObject(Setting.Instance, Formatting.Indented);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "setting.json", settingStr);
                this.Close();
            }
            catch { }

        }

    }
}
