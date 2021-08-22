using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Newtonsoft.Json;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.Models.UnmanagedMethods;
using ScriptGraphicHelper.ViewModels;
using System.ComponentModel;

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

        private DispatcherTimer Timer = new();

        private void Window_Opened(object sender, EventArgs e)
        {
            AddHandler(DragDrop.DropEvent, (this.DataContext as MainWindowViewModel).DropImage_Event);
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

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            switch (key)
            {
                case Key.Left: NativeApi.Move2Left(); break;
                case Key.Up: NativeApi.Move2Top(); break;
                case Key.Right: NativeApi.Move2Right(); break;
                case Key.Down: NativeApi.Move2Bottom(); break;
                default: return;
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

        private double defaultWidth;
        private double defaultHeight;
        private void WindowStateChange_Tapped(object sender, RoutedEventArgs e)
        {
            this.CanResize = true;
            Button default_btn = this.FindControl<Button>("Default_btn");
            Button fullScreen_btn = this.FindControl<Button>("FullScreen_btn");
            if (WindowState == WindowState.FullScreen)
            {
                default_btn.IsVisible = false;
                fullScreen_btn.IsVisible = true;
                WindowState = WindowState.Normal;

                this.Width = defaultWidth;
                this.Height = defaultHeight;
                var workingAreaSize = Screens.Primary.WorkingArea.Size;
                this.Position = new PixelPoint((int)((workingAreaSize.Width - this.Width) / 2), (int)((workingAreaSize.Height - this.Height) / 2));
            }
            else
            {
                defaultWidth = this.Width;
                defaultHeight = this.Height;
                default_btn.IsVisible = true;
                fullScreen_btn.IsVisible = false;
                WindowState = WindowState.FullScreen;
            }
        }

        private void Close_Tapped(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
