using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Newtonsoft.Json;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.Models.UnmanagedMethods;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

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
            FontWeight = Avalonia.Media.FontWeight.Medium;
        }

        private void Window_Opened(object sender, EventArgs e)
        {
            Width = Setting.Instance.Width;
            Height = Setting.Instance.Height;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Setting.Instance.Width = Width;
            Setting.Instance.Height = Height;

            string settingStr = JsonConvert.SerializeObject(Setting.Instance, Formatting.Indented);
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "setting.json", settingStr);

            try
            {
                Process[] processes = Process.GetProcessesByName("DmServer");
                if (processes.Length > 0)
                {
                    for (int i = 0; i < processes.Length; i++)
                    {
                        processes[i].Kill();
                    }
                }
            }
            catch { }
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
