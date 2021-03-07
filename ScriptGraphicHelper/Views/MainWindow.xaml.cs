using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ScriptGraphicHelper.Models.UnmanagedMethods;
using System;

namespace ScriptGraphicHelper.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
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
            this.FontWeight = Avalonia.Media.FontWeight.Medium;
            DataGrid d = this.FindControl<DataGrid>("ColorInfos");
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

            Key key = e.Key;
            POINT point = Win32Api.GetCursorPos();
            switch (key)
            {
                case Key.Left: Win32Api.SetCursorPos(point.X - 1, point.Y); break;
                case Key.Up: Win32Api.SetCursorPos(point.X, point.Y - 1); break;
                case Key.Right: Win32Api.SetCursorPos(point.X + 1, point.Y); break;
                case Key.Down: Win32Api.SetCursorPos(point.X, point.Y + 1); break;
                default: break;
            }
        }
    }
}
