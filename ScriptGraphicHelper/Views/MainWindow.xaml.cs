using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
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
        public static MainWindow Instance { get; private set; }
        public IntPtr Handle { get; private set; }


        public MainWindow()
        {
            ExtendClientAreaToDecorationsHint = true;
            ExtendClientAreaTitleBarHeightHint = -1;
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;

            Instance = this;
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

        private DispatcherTimer Timer = new DispatcherTimer();

        private void Window_Opened(object sender, EventArgs e)
        {
            this.Handle = this.PlatformImpl.Handle.Handle;
            this.ClientSize = new Size(Setting.Instance.Width, Setting.Instance.Height);
            Timer.Tick += new EventHandler(HintMessage_Closed);
            Timer.Interval = new TimeSpan(0, 0, 8);
            Timer.Start();
        }

        private void HintMessage_Closed(object? sender, EventArgs e)
        {
            var hint = this.FindControl<Border>("HintMessage");
            hint.IsVisible = false;
            Timer.IsEnabled = false;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Setting.Instance.Width = Width;
            Setting.Instance.Height = Height;

            string settingStr = JsonConvert.SerializeObject(Setting.Instance, Formatting.Indented);
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"Assets\setting.json", settingStr);

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
            e.Handled = true;
        }

        private void TitleBar_DragMove(object sender, PointerPressedEventArgs e)
        {
            this.BeginMoveDrag(e);
        }

        private void Minsize_Tapped(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Fullscreen_Tapped(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.FullScreen)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.FullScreen;
            }
        }

        private void Close_Tapped(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
