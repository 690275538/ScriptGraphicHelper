using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace ScriptGraphicHelper.Views
{
    public class DataImport : Window
    {
        public string ImportString { get; private set; } = string.Empty;
        public DataImport()
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

        private void Ok_Tapped(object sender, RoutedEventArgs e)
        {
            var tb = this.FindControl<TextBox>("ImportString");
            if (tb.Text != "")
            {
                ImportString = tb.Text.Trim();
                this.Close();
            }
        }
    }
}
