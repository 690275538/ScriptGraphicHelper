using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;

namespace ScriptGraphicHelper.Panels
{
    public class Controls : UserControl
    {
        public Controls()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ColorInfos_DataContextChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("222ee2");
        }
    }
}
