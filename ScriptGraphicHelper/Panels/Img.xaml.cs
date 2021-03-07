using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ScriptGraphicHelper.Panels
{
    public class Img : UserControl
    {
        public Img()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
