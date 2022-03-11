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
            var diyFormatMode = this.FindControl<ComboBox>("DiyFormatMode");

            addRange.IsChecked = Settings.Instance.AddRange;
            addInfo.IsChecked = Settings.Instance.AddInfo;
            isOffset.IsChecked = Settings.Instance.IsOffset;
            diySim.Text = Settings.Instance.DiySim.ToString();
            ysPath.Text = Settings.Instance.YsPath;
            xyPath.Text = Settings.Instance.XyPath;
            dmRegcode.Text = Settings.Instance.DmRegcode;
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
                var diyFormatMode = this.FindControl<ComboBox>("DiyFormatMode");

                Settings.Instance.AddRange = addRange.IsChecked ?? false;
                Settings.Instance.AddInfo = addInfo.IsChecked ?? false;
                Settings.Instance.IsOffset = isOffset.IsChecked ?? false;

                if (int.TryParse(diySim.Text.Trim(), out var sim))
                {
                    Settings.Instance.DiySim = sim;
                }

                Settings.Instance.YsPath = ysPath.Text ?? string.Empty;
                Settings.Instance.XyPath = xyPath.Text ?? string.Empty;
                Settings.Instance.DmRegcode = dmRegcode.Text ?? string.Empty;

                var settingStr = JsonConvert.SerializeObject(Settings.Instance, Formatting.Indented);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"assets\settings.json", settingStr);
                Close();
            }
            catch { }

        }

    }
}
