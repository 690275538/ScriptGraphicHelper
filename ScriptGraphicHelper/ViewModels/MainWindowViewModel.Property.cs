using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.ViewModels.Core;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace ScriptGraphicHelper.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private Cursor windowCursor = new Cursor(StandardCursorType.Arrow);
        public Cursor WindowCursor
        {
            get => windowCursor;
            set => this.RaiseAndSetIfChanged(ref windowCursor, value);
        }

        private int emulatorSelectedIndex = 0;
        public int EmulatorSelectedIndex
        {
            get => emulatorSelectedIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref emulatorSelectedIndex, value);
                Emulator_Selected(value);
            }
        }

        private int simSelectedIndex = 0;
        public int SimSelectedIndex
        {
            get => simSelectedIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref simSelectedIndex, value);
                Setting.Instance.SimSelectedIndex = value;
            }
        }

        private FormatMode formatSelectedIndex = FormatMode.compareStr;
        public FormatMode FormatSelectedIndex
        {
            get => formatSelectedIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref formatSelectedIndex, value);
                Setting.Instance.FormatSelectedIndex = (int)value;
                if (value == FormatMode.anchorsFindStr || value == FormatMode.anchorsCompareStr)
                {
                    DataGrid_IsVisible = false;
                    ImgMargin = new Thickness(170, 50, 340, 20);
                    ColorInfo.Width = ImgWidth;
                    ColorInfo.Height = imgHeight;
                }
                else
                {
                    DataGrid_IsVisible = true;
                    ImgMargin = new Thickness(170, 50, 280, 20);
                }
            }
        }

        private string testResult = string.Empty;
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

        private string createStr = string.Empty;
        public string CreateStr
        {
            get => createStr;
            set => this.RaiseAndSetIfChanged(ref createStr, value);
        }

        private ObservableCollection<string> emulatorInfo;
        public ObservableCollection<string> EmulatorInfo
        {
            get => emulatorInfo;
            set => this.RaiseAndSetIfChanged(ref emulatorInfo, value);
        }

        private int titleBarWidth;
        public int TitleBarWidth
        {
            get => titleBarWidth;
            set => this.RaiseAndSetIfChanged(ref titleBarWidth, value);
        }

        private TabItems<TabItem> tabItems = new TabItems<TabItem>();
        public TabItems<TabItem> TabItems
        {
            get => tabItems;
            set => this.RaiseAndSetIfChanged(ref tabItems, value);
        }

        private int tabControlSelectedIndex;
        public int TabControlSelectedIndex
        {
            get => tabControlSelectedIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref tabControlSelectedIndex, value);
                if (value != -1)
                {
                    Img = TabItems[value].Img;
                    MemoryStream stream = new MemoryStream();
                    Img.Save(stream);
                    stream.Position = 0;
                    SKBitmap sKBitmap = SKBitmap.Decode(stream);
                    GraphicHelper.KeepScreen(sKBitmap);
                    sKBitmap.Dispose();
                    stream.Dispose();
                }
                else
                {
                    SKBitmap sKBitmap = new SKBitmap(1, 1);
                    GraphicHelper.KeepScreen(sKBitmap);
                    Img = new Bitmap(GraphicHelper.PxFormat, AlphaFormat.Opaque, sKBitmap.GetPixels(), new PixelSize(1, 1), new Vector(96, 96), sKBitmap.RowBytes);
                    sKBitmap.Dispose();
                }
            }
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

        private Thickness imgMargin = new Thickness(170, 50, 280, 20);
        public Thickness ImgMargin
        {
            get => imgMargin;
            set => this.RaiseAndSetIfChanged(ref imgMargin, value);
        }

        private double imgWidth = 0;
        private double ImgWidth
        {
            get => imgWidth;
            set
            {
                imgWidth = value;
                ImgDrawWidth = Math.Floor(value * ScaleFactor);
            }
        }

        private double imgHeight = 0;
        private double ImgHeight
        {
            get => imgHeight;
            set
            {
                imgHeight = value;
                ImgDrawHeight = Math.Floor(value * ScaleFactor);
            }
        }

        private double imgDrawWidth = 0;
        public double ImgDrawWidth
        {
            get => imgDrawWidth;
            set => this.RaiseAndSetIfChanged(ref imgDrawWidth, value);
        }

        private double imgDrawHeight = 0;
        public double ImgDrawHeight
        {
            get => imgDrawHeight;
            set => this.RaiseAndSetIfChanged(ref imgDrawHeight, value);
        }

        private double scaleFactor = 1.0;
        public double ScaleFactor
        {
            get => scaleFactor;
            set
            {
                ImgDrawWidth = Math.Floor(ImgWidth * value);
                ImgDrawHeight = Math.Floor(ImgHeight * value);
                this.RaiseAndSetIfChanged(ref scaleFactor, value);
            }
        }

        private WriteableBitmap loupeWriteBmp;
        public WriteableBitmap LoupeWriteBmp
        {
            get => loupeWriteBmp;
            set => this.RaiseAndSetIfChanged(ref loupeWriteBmp, value);
        }

        private bool loupe_IsVisible = false;
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

        private int pointX = 0;
        public int PointX
        {
            get => pointX;
            set => this.RaiseAndSetIfChanged(ref pointX, value);
        }

        private int pointY = 0;
        public int PointY
        {
            get => pointY;
            set => this.RaiseAndSetIfChanged(ref pointY, value);
        }

        private double rectWidth = 0;
        public double RectWidth
        {
            get => rectWidth;
            set => this.RaiseAndSetIfChanged(ref rectWidth, value);
        }

        private double rectHeight = 0;
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

        private bool rect_IsVisible = false;
        public bool Rect_IsVisible
        {
            get => rect_IsVisible;
            set => this.RaiseAndSetIfChanged(ref rect_IsVisible, value);
        }

        private Thickness findedPoint_Margin;
        public Thickness FindedPoint_Margin
        {
            get => findedPoint_Margin;
            set => this.RaiseAndSetIfChanged(ref findedPoint_Margin, value);
        }

        private bool findedPoint_IsVisible = false;
        public bool FindedPoint_IsVisible
        {
            get => findedPoint_IsVisible;
            set => this.RaiseAndSetIfChanged(ref findedPoint_IsVisible, value);
        }

        private ObservableCollection<ColorInfo> colorInfos;
        public ObservableCollection<ColorInfo> ColorInfos
        {
            get => colorInfos;
            set
            {
                this.RaiseAndSetIfChanged(ref colorInfos, value);
            }
        }

        private int dataGridSelectedIndex;
        public int DataGridSelectedIndex
        {
            get => dataGridSelectedIndex;
            set => this.RaiseAndSetIfChanged(ref dataGridSelectedIndex, value);
        }

        private int dataGridHeight;
        public int DataGridHeight
        {
            get => dataGridHeight;
            set
            {
                if (value > 1000)
                {
                    value = 1000;
                }
                this.RaiseAndSetIfChanged(ref dataGridHeight, value);
            }
        }

        private bool dataGrid_IsVisible = true;
        public bool DataGrid_IsVisible
        {
            get => dataGrid_IsVisible;
            set => this.RaiseAndSetIfChanged(ref dataGrid_IsVisible, value);
        }

    }

}
