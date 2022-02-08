using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;

namespace ScriptGraphicHelper.Views
{
    public class TcpConfig : Window
    {
        public TcpConfig()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

        }

        private List<string> Addresses;

        public TcpConfig(List<string> addresses)
        {
            this.Addresses = addresses;
            InitializeComponent();
        }

        private void WindowOpened(object sender, EventArgs e)
        {
            var comboBox = this.FindControl<ComboBox>("AddressList");
            comboBox.Items = this.Addresses;
            foreach (var address in this.Addresses)
            {
                if (address.StartsWith("192.168"))
                {
                    comboBox.SelectedItem = address;
                }
            }
        }

        private void Ok_Tapped(object sender, RoutedEventArgs e)
        {
            var address = (string)this.FindControl<ComboBox>("AddressList").SelectedItem;
            var port = int.Parse(this.FindControl<TextBox>("Port").Text.Trim());
            Close((address, port));
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
                    var address = (string)this.FindControl<ComboBox>("AddressList").SelectedItem;
                    var port = int.Parse(this.FindControl<TextBox>("Port").Text.Trim());
                    Close((address, port));
                    break;

                case Key.Escape: Close(); break;

                default: return;
            }
            e.Handled = true;
        }

    }
}
