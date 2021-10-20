using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;

namespace ScriptGraphicHelper.Views
{
    public class AJConfig : Window
    {
        public AJConfig()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

        }

        private List<string> Addresses;

        public AJConfig(List<string> addresses)
        {
            this.Addresses = addresses;
            InitializeComponent();
        }

        private void WindowOpened(object sender, EventArgs e)
        {
            var comboBox = this.FindControl<ComboBox>("AddressList");
            comboBox.Items = Addresses;

            foreach (var address in Addresses)
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
            var remoteAddress = this.FindControl<TextBox>("RemoteAddress").Text.Trim();
            Close((address, remoteAddress));
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            switch (key)
            {
                case Key.Enter:
                    var address = (string)this.FindControl<ComboBox>("Address").SelectedItem;
                    var remoteAddress = this.FindControl<TextBox>("RemoteAddress").Text.Trim();
                    Close((address, remoteAddress));
                    break;

                case Key.Escape: Close(); break;

                default: return;
            }
            e.Handled = true;
        }

    }
}
