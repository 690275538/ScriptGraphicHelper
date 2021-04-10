using Avalonia;
using Avalonia.Controls;
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

        private void WindowOpened(object sender, EventArgs e)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            var address = this.FindControl<TextBox>("Address");
            address.Text = Address != string.Empty ? Address : "192.168.0.";
            var port = this.FindControl<TextBox>("Port");
            port.Text = Port.ToString();
            if (Title == "AJ配置")
            {
                port.IsVisible = false;
                var tb = this.FindControl<TextBlock>("Description");
                tb.Text = "需要开启autojs的悬浮窗!";
                tb.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
                Height = Height - 20;
            }
        }

        private void Btn_Tapped(object sender, RoutedEventArgs e)
        {
            var address = this.FindControl<TextBox>("Address");
            Address = address.Text.Trim();
            var port = this.FindControl<TextBox>("Port");
            Port = int.Parse(port.Text.Trim());
            this.Close();
        }
    }
}
