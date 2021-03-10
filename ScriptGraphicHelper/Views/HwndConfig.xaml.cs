using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ScriptGraphicHelper.ViewModels;

namespace ScriptGraphicHelper.Views
{
    public class HwndConfig : Window
    {
        public HwndConfig()
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
            DataContext = new HwndConfigViewModel(this);
        }
    }
}
