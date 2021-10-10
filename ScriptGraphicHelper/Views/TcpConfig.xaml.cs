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
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

        }

        public static string Address { get; set; } = string.Empty;
        public static int Port { get; set; } = 5678;
        public bool IsTapped { get; set; } = false;

        private void WindowOpened(object sender, EventArgs e)
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            if (this.Title == "tcp≈‰÷√")
            {
                this.FindControl<Border>("TBBorder").IsVisible = false;
                this.FindControl<Border>("CBBorder").IsVisible = true;
                var addressList = this.FindControl<ComboBox>("AddressList");
                var array = Address.Split("|");
                addressList.Items = array;
                addressList.SelectedIndex = 0;

                for (var i = 0; i < array.Length; i++)
                {
                    if (array[i].StartsWith("192.168"))
                    {
                        addressList.SelectedIndex = i;
                    }
                }

            }

            var address = this.FindControl<TextBox>("Address");
            address.Text = Address != string.Empty ? Address : "192.168.0.";
            var port = this.FindControl<TextBox>("Port");
            port.Text = Port.ToString();
            if (this.Title == "AJ≈‰÷√")
            {
                port.IsVisible = false;
                var tb = this.FindControl<TextBlock>("Description");
                tb.Text = "–Ë“™ø™∆Ùautojsµƒ–¸∏°¥∞!";
                tb.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
                this.Height -= 20;
            }
            else if (this.Title == "AdbŒﬁœﬂµ˜ ‘")
            {
                var btn = this.FindControl<Button>("Skip");
                btn.IsVisible = true;
                this.Height += 50;
            }

        }

        private void Ok_Tapped(object sender, RoutedEventArgs e)
        {
            this.IsTapped = true;
            var address = this.FindControl<TextBox>("Address");
            Address = address.Text.Trim();

            if (this.Title == "tcp≈‰÷√")
            {
                var addressList = this.FindControl<ComboBox>("AddressList");
                Address = addressList.SelectedItem as string ?? "null";
            }

            var port = this.FindControl<TextBox>("Port");
            Port = int.Parse(port.Text.Trim());
            Close();
        }

        private void Skip_Tapped(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            switch (key)
            {
                case Key.Enter:
                    this.IsTapped = true;
                    var address = this.FindControl<TextBox>("Address");
                    Address = address.Text.Trim();

                    if (this.Title == "tcp≈‰÷√")
                    {
                        var addressList = this.FindControl<ComboBox>("AddressList");
                        Address = addressList.SelectedItem as string ?? "null";
                    }

                    var port = this.FindControl<TextBox>("Port");
                    Port = int.Parse(port.Text.Trim());
                    Close();
                    break;

                case Key.Escape: Close(); break;

                default: return;
            }
            e.Handled = true;
        }

    }
}
