using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Diagnostics;

namespace ScriptGraphicHelper.Panels
{
    public class ColorInfos : UserControl
    {
        public ColorInfos()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var dataGrid = (DataGrid)sender;
            e.Row.PointerEnter += (object? sender, PointerEventArgs _) =>
            {
                int index = e.Row.GetIndex();
                dataGrid.SelectedIndex = index;
            };
        }
    }
}
