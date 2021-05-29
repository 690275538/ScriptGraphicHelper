using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace ScriptGraphicHelper.Views
{
    public partial class MessageBox : Window
    {
        public MessageBox()
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

        private string Title { get; set; } = string.Empty;
        private string Message { get; set; } = string.Empty;

        public MessageBox(string title, string msg) : this()
        {
            Title = title;
            Message = msg;
        }

        public MessageBox(string msg) : this("ÌבÊ¾", msg) { }


        private void Window_Opened(object sender, EventArgs e)
        {
            var title = this.FindControl<TextBlock>("Title");
            title.Text = Title;
            var tb = this.FindControl<TextBlock>("Message");
            tb.Text = Message;

        }

        private async void Close_Tapped(object sender, RoutedEventArgs e)
        {
            await Application.Current.Clipboard.SetTextAsync(Message);
            this.Close();
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await Application.Current.Clipboard.SetTextAsync(Message);
                this.Close();
            }
        }

        private void Window_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Width" || e.Property.Name == "Height")
            {
                Position = new PixelPoint((int)(Screens.Primary.WorkingArea.Width / 2 - Width / 2), (int)(Screens.Primary.WorkingArea.Height / 2 - Height / 2));
            }
        }
    }
}
