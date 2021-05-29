using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace ScriptGraphicHelper.Views
{
    public class TcpConfig : Window
    {
        public TcpConfig()
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

        public static string Address { get; set; } = string.Empty;
        public static int Port { get; set; } = 5678;
        public bool IsTapped { get; set; } = false;

        private void WindowOpened(object sender, EventArgs e)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            var address = this.FindControl<TextBox>("Address");
            address.Text = Address != string.Empty ? Address : "192.168.0.";
            var port = this.FindControl<TextBox>("Port");
            port.Text = Port.ToString();
            if (Title == "tcp配置")
            {
                address.IsReadOnly = true;
            }
            else if (Title == "AJ配置")
            {
                port.IsVisible = false;
                var tb = this.FindControl<TextBlock>("Description");
                tb.Text = "需要开启autojs的悬浮窗!";
                tb.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
                Height -= 20;
            }
            else if (Title == "Adb无线调试")
            {
                var btn = this.FindControl<Button>("Skip");
                btn.IsVisible = true;
                Height += 50;
            }
        }

        private void Ok_Tapped(object sender, TappedEventArgs e)
        {
            IsTapped = true;
            var address = this.FindControl<TextBox>("Address");
            Address = address.Text.Trim();
            var port = this.FindControl<TextBox>("Port");
            Port = int.Parse(port.Text.Trim());
            this.Close();
        }

        private void Skip_Tapped(object sender, TappedEventArgs e)
        {
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            switch (key)
            {
                case Key.Enter:
                    IsTapped = true;
                    var address = this.FindControl<TextBox>("Address");
                    Address = address.Text.Trim();
                    var port = this.FindControl<TextBox>("Port");
                    Port = int.Parse(port.Text.Trim());
                    this.Close();
                    break;

                case Key.Escape: this.Close(); break;

                default: return;
            }
            e.Handled = true;
        }

    }
}
