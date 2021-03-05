using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.ViewModels.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace ScriptGraphicHelper.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
       
        private int emulatorSelected;
        public int EmulatorSelected
        {
            get => emulatorSelected;
            set => this.RaiseAndSetIfChanged(ref emulatorSelected, value);
        }

        private int simSelected;
        public int SimSelected
        {
            get => simSelected;
            set => this.RaiseAndSetIfChanged(ref simSelected, value);
        }

        private FormatMode formatSelected;
        public FormatMode FormatSelected
        {
            get => formatSelected;
            set => this.RaiseAndSetIfChanged(ref formatSelected, value);
        }

        private string rect;
        public string Rect
        {
            get => rect;
            set => this.RaiseAndSetIfChanged(ref rect, value);
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
            set => this.RaiseAndSetIfChanged(ref img, value);
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


        public MainWindowViewModel()
        {
            LoupeWriteBmp = LoupeWriteBitmap.Init(241, 241);
            Loupe_IsVisible = false;
            Rect_IsVisible = false;
            EmulatorInfo = new();
            EmulatorInfo.Add("雷电");
            EmulatorInfo.Add("雷电64");
            EmulatorInfo.Add("夜神");
            EmulatorInfo.Add("逍遥");
            EmulatorInfo.Add("UDP");
        }

        private Point StartPoint;

        [Obsolete]
        public ICommand Img_PointerPressed => new Command((param) =>
        {
            if (param != null)
            {
                CommandParameters parameters = (CommandParameters)param;
                var eventArgs = (PointerPressedEventArgs)parameters.EventArgs;
                if (eventArgs.MouseButton == MouseButton.Left)
                {
                    Loupe_IsVisible = false;
                    StartPoint = eventArgs.GetPosition(null);
                    RectMargin = new Thickness(StartPoint.X, StartPoint.Y, 0, 0);
                    Rect_IsVisible = true;
                }

            }
        });

        public ICommand Img_PointerMoved => new Command((param) =>
        {
            if (param != null)
            {
                CommandParameters parameters = (CommandParameters)param;
                var eventArgs = (PointerEventArgs)parameters.EventArgs;
                var point = eventArgs.GetPosition(null);
                if (Rect_IsVisible)
                {
                    double width = point.X - StartPoint.X - 1;
                    double height = point.Y - StartPoint.Y - 1;
                    if (width > 0 && height > 0)
                    {
                        RectWidth = width;
                        RectHeight = height;
                    }
                }
                else
                {
                    if (point.Y > 600)
                        LoupeMargin = new Thickness(point.X + 20, point.Y - 261, 0, 0);
                    else
                        LoupeMargin = new Thickness(point.X + 20, point.Y + 20, 0, 0);

                    var imgPoint = eventArgs.GetPosition((Image)parameters.Sender);


                    PointX = Math.Floor(imgPoint.X);
                    PointY = Math.Floor(imgPoint.Y);

                    int sx = (int)PointX - 7;
                    int sy = (int)PointY - 7;
                    List<byte[]> colors = new();
                    for (int j = 0; j < 15; j++)
                    {
                        for (int i = 0; i < 15; i++)
                        {
                            int x = sx + i;
                            int y = sy + j;

                            if (x >= 0 && y >= 0 && x < ImgWidth && y < ImgHeight)
                            {
                                colors.Add(GraphicHelper.GetPixel(x, y));
                            }
                            else
                            {
                                colors.Add(new byte[] { 0, 0, 0 });
                            }
                        }
                    }
                    LoupeWriteBmp.WriteColor(colors);

                }


            }
        });

        public ICommand Img_PointerReleased => new Command((param) =>
        {
            if (param != null)
            {
                CommandParameters parameters = (CommandParameters)param;
                if (Rect_IsVisible)
                {
                    var img = (Image)parameters.Sender;
                    var eventArgs = (PointerEventArgs)parameters.EventArgs;
                    var point = eventArgs.GetPosition(img);
                    Rect = string.Format("[{0},{1},{2},{3}]", point.X-RectWidth, point.Y - rectHeight, point.X, point.Y);
                }
                Rect_IsVisible = false;
                Loupe_IsVisible = true;
                RectWidth = 0;
                RectHeight = 0;
                RectMargin = new Thickness(0, 0, 0, 0);
            }
        });

        public ICommand Img_PointerEnter => new Command((param) => Loupe_IsVisible = true);

        public ICommand Img_PointerLeave => new Command((param) => Loupe_IsVisible = false);



        public async void Load_Click()
        {
            try
            {
                //OpenFileDialog fileDialog = new()
                //{
                //    AllowMultiple = false,
                //    Filters = new List<FileDialogFilter>()
                //    {
                //        new FileDialogFilter
                //        {
                //            Name = "位图文件",
                //            Extensions = new List<string>() { "jpg", "png", "bmp" }
                //        }
                //    },
                //    Title = "请选择文件",
                //};
                //string[] result = await fileDialog.ShowAsync(new Window());
                string[] result = new string[] { @"C:\Users\PC\Documents\leidian\Pictures\test.png" };
                if (result.Length > 0)
                {
                    string fileName = result[0];
                    var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    var position = fileStream.Position;

                    Img = new Bitmap(fileStream);
                    ImgWidth = Img.Size.Width;
                    ImgHeight = Img.Size.Height;
                    fileStream.Position = position;
                    SKBitmap sKBitmap = SKBitmap.Decode(fileStream);
                    GraphicHelper.KeepScreen(sKBitmap);
                    fileStream.Close();
                    fileStream.Dispose();
                }

            }
            catch (Exception e)
            {
                //var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("title", e.Message);
                //await messageBoxStandardWindow.Show();
            }
        }
    }

}
