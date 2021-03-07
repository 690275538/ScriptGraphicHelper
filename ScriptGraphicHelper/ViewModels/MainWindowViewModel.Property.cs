using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.ViewModels.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScriptGraphicHelper.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private Cursor windowCursor;
        public Cursor WindowCursor
        {
            get => windowCursor;
            set => this.RaiseAndSetIfChanged(ref windowCursor, value);
        }

        private int emulatorSelectedIndex;
        public int EmulatorSelectedIndex
        {
            get => emulatorSelectedIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref emulatorSelectedIndex, value);
                Emulator_Selected(value);
            }
        }

        private int simSelectedIndex;
        public int SimSelectedIndex
        {
            get => simSelectedIndex;
            set => this.RaiseAndSetIfChanged(ref simSelectedIndex, value);
        }

        private FormatMode formatSelectedIndex;
        public FormatMode FormatSelectedIndex
        {
            get => formatSelectedIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref formatSelectedIndex, value);
                if (value ==FormatMode.anchorsFindStr || value == FormatMode.anchorsCompareStr)
                {
                    DataGrid_IsVisible = false;
                    ImgMargin = new Thickness(170, 20, 340, 20);
                    ColorInfo.Width = ImgWidth;
                    ColorInfo.Height = ImgHeight;
                }
                else
                {
                    DataGrid_IsVisible = true;
                    ImgMargin = new Thickness(170, 20, 280, 20);
                }
            }
        }

        private string testResult;
        public string TestResult
        {
            get => testResult;
            set => this.RaiseAndSetIfChanged(ref testResult, value);
        }

        private string rect = string.Empty;
        public string Rect
        {
            get => rect;
            set => this.RaiseAndSetIfChanged(ref rect, value);
        }

        private string createString;
        public string CreateString
        {
            get => createString;
            set => this.RaiseAndSetIfChanged(ref createString, value);
        }

        private ObservableCollection<string> emulatorInfo;
        public ObservableCollection<string> EmulatorInfo
        {
            get => emulatorInfo;
            set => this.RaiseAndSetIfChanged(ref emulatorInfo, value);
        }

        private Bitmap img;
        public Bitmap Img
        {
            get => img;
            set
            {
                this.RaiseAndSetIfChanged(ref img, value);
                ImgWidth = value.Size.Width;
                ImgHeight = value.Size.Height;
            }
        }

        private Thickness imgMargin;
        public Thickness ImgMargin
        {
            get => imgMargin;
            set => this.RaiseAndSetIfChanged(ref imgMargin, value);
        }

        private double imgWidth;
        public double ImgWidth
        {
            get => imgWidth;
            set => this.RaiseAndSetIfChanged(ref imgWidth, value);
        }

        private double imgHeight;
        public double ImgHeight
        {
            get => imgHeight;
            set => this.RaiseAndSetIfChanged(ref imgHeight, value);
        }

        private WriteableBitmap loupeWriteBmp;
        public WriteableBitmap LoupeWriteBmp
        {
            get => loupeWriteBmp;
            set => this.RaiseAndSetIfChanged(ref loupeWriteBmp, value);
        }

        private bool loupe_IsVisible;
        public bool Loupe_IsVisible
        {
            get => loupe_IsVisible;
            set => this.RaiseAndSetIfChanged(ref loupe_IsVisible, value);
        }

        private Thickness loupeMargin;
        public Thickness LoupeMargin
        {
            get => loupeMargin;
            set => this.RaiseAndSetIfChanged(ref loupeMargin, value);
        }

        private double pointX;
        public double PointX
        {
            get => pointX;
            set => this.RaiseAndSetIfChanged(ref pointX, value);
        }

        private double pointY;
        public double PointY
        {
            get => pointY;
            set => this.RaiseAndSetIfChanged(ref pointY, value);
        }

        private double rectWidth;
        public double RectWidth
        {
            get => rectWidth;
            set => this.RaiseAndSetIfChanged(ref rectWidth, value);
        }

        private double rectHeight;
        public double RectHeight
        {
            get => rectHeight;
            set => this.RaiseAndSetIfChanged(ref rectHeight, value);
        }

        private Thickness rectMargin;
        public Thickness RectMargin
        {
            get => rectMargin;
            set => this.RaiseAndSetIfChanged(ref rectMargin, value);
        }

        private bool rect_IsVisible;
        public bool Rect_IsVisible
        {
            get => rect_IsVisible;
            set => this.RaiseAndSetIfChanged(ref rect_IsVisible, value);
        }

        private ObservableCollection<ColorInfo> colorInfos;
        public ObservableCollection<ColorInfo> ColorInfos
        {
            get => colorInfos;
            set => this.RaiseAndSetIfChanged(ref colorInfos, value);
        }

        private int dataGridHeight;
        public int DataGridHeight
        {
            get => dataGridHeight;
            set => this.RaiseAndSetIfChanged(ref dataGridHeight, value);
        }

        private bool dataGrid_IsVisible = true;
        public bool DataGrid_IsVisible
        {
            get => dataGrid_IsVisible;
            set => this.RaiseAndSetIfChanged(ref dataGrid_IsVisible, value);
        }

    }
}
