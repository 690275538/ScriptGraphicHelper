using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

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
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        public static string Address { get; set; } = string.Empty;
        public static int Port { get; set; } = -1;

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
