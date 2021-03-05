using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ScriptGraphicHelper.Models;
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
            PropertyChanged += Window_PropertyChanged;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            POINT point = new();
            Win32Api.GetCursorPos(out point);
            switch (key)
            {
                case Key.Left: Win32Api.SetCursorPos(point.X - 1, point.Y); break;
                case Key.Up: Win32Api.SetCursorPos(point.X, point.Y - 1); break;
                case Key.Right: Win32Api.SetCursorPos(point.X + 1, point.Y); break;
                case Key.Down: Win32Api.SetCursorPos(point.X, point.Y + 1); break;
                default: break;
            }
        }

        private double OldWidth = 0;
        private double OldHeight = 0;
        private void Window_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (Width != OldWidth || Height != OldHeight)
            {
                OldWidth = Width;
                OldHeight = Height;

                Grid grid = this.FindControl<Grid>("Grid_Img");
                grid.MaxWidth = Width - 200;
                grid.MaxHeight = Height - 50;
            }
        }
    }
}
