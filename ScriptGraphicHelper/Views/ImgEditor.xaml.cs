using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.ViewModels;

namespace ScriptGraphicHelper.Views
{
    public class ImgEditor : Window
    {
        public ImgEditor()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }
        public ImgEditor(Range range, byte[] data)
        {
            InitializeComponent();
            this.DataContext = new ImgEditorViewModel(range, data);
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
