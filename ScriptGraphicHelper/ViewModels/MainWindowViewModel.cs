using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.Models.UnmanagedMethods;
using ScriptGraphicHelper.ViewModels.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Image = Avalonia.Controls.Image;
using Point = Avalonia.Point;
using SystemBitmap = System.Drawing.Bitmap;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;
using ScriptGraphicHelper.Converters;
using Range = ScriptGraphicHelper.Models.Range;

namespace ScriptGraphicHelper.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            ColorInfos = new ObservableCollection<ColorInfo>();


            LoupeWriteBmp = LoupeWriteBitmap.Init(241, 241);
            DataGridHeight = 40;
            Loupe_IsVisible = false;
            Rect_IsVisible = false;
            EmulatorSelectedIndex = -1;
            EmulatorInfo = EmulatorHelper.Init();
            ImgMargin = new Thickness(170, 20, 280, 20);
            //EmulatorInfo.Add("雷电模拟器");
            //EmulatorInfo.Add("雷电模拟器64");
            //EmulatorInfo.Add("夜神模拟器");
            //EmulatorInfo.Add("逍遥模拟器");
            //EmulatorInfo.Add("UDP通信");
        }

        private Point StartPoint;

        public ICommand Img_PointerPressed => new Command((param) =>
        {
            if (param != null)
            {
                CommandParameters parameters = (CommandParameters)param;
                var eventArgs = (PointerPressedEventArgs)parameters.EventArgs;
                if (eventArgs.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
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
                    List<byte[]> colors = new List<byte[]>();
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
                    var eventArgs = (PointerEventArgs)parameters.EventArgs;
                    var point = eventArgs.GetPosition((Image)parameters.Sender);
                    int sx = (int)(point.X - RectWidth);
                    int sy = (int)(point.Y - rectHeight);
                    if (RectWidth > 10 && rectHeight > 10)
                    {
                        Rect = string.Format("[{0},{1},{2},{3}]", sx, sy, point.X, point.Y);
                    }
                    else
                    {
                        byte[] color = GraphicHelper.GetPixel(sx, sy);

                        if (FormatSelectedIndex == FormatMode.anchorsFindStr || FormatSelectedIndex == FormatMode.anchorsCompareStr)
                        {
                            AnchorType anchor = AnchorType.None;
                            double quarterWidth = imgWidth / 4;
                            if (sx > quarterWidth * 3)
                            {
                                anchor = AnchorType.Right;
                            }
                            else if (sx > quarterWidth)
                            {
                                anchor = AnchorType.Center;
                            }
                            else
                            {
                                anchor = AnchorType.Left;
                            }
                            if (ColorInfos.Count == 0)
                            {
                                ColorInfo.Width = ImgWidth;
                                ColorInfo.Height = ImgHeight;
                            }
                            ColorInfos.Add(new ColorInfo(ColorInfos.Count, anchor, sx, sy, color));
                        }
                        else
                        {
                            ColorInfos.Add(new ColorInfo(ColorInfos.Count, sx, sy, color));
                        }

                        DataGridHeight = (ColorInfos.Count + 1) * 40;
                    }
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

        public async void Emulator_Selected(int value)
        {
            if (EmulatorHelper.IsInit == EmlatorState.success)
            {
                EmulatorHelper.Index = EmulatorSelectedIndex;
            }
            else if (EmulatorHelper.IsInit == EmlatorState.Waiting)
            {
                WindowCursor = new Cursor(StandardCursorType.Wait);
                EmulatorHelper.Changed(EmulatorSelectedIndex);
                EmulatorInfo = await EmulatorHelper.GetAll();
                EmulatorSelectedIndex = -1;
                WindowCursor = new Cursor(StandardCursorType.Arrow);

            }
            else if (EmulatorHelper.IsInit == EmlatorState.Starting)
            {
                EmulatorHelper.IsInit = EmlatorState.success;
            }
        }

        public async void ScreenShot_Click()
        {
            WindowCursor = new Cursor(StandardCursorType.Wait);
            if (EmulatorHelper.Select == -1 || EmulatorHelper.Index == -1)
            {
                Win32Api.MessageBox("请先配置 -> (模拟器/tcp/句柄)");
                WindowCursor = new Cursor(StandardCursorType.Arrow);
                return;
            }
            Bitmap bitmap = await EmulatorHelper.ScreenShot();
            if (bitmap.Width != 1)
            {
                LoadBitmap(bitmap);
                bitmap.Dispose();
            }
            WindowCursor = new Cursor(StandardCursorType.Arrow);
        }

        public void ResetEmulatorOptions_Click()
        {
            if (EmulatorHelper.IsInit == EmlatorState.success)
            {
                EmulatorHelper.Dispose();
                EmulatorInfo.Clear();
                EmulatorInfo = EmulatorHelper.Init();
            }
        }

        public async void TurnRight_Click()
        {
            if (Img == null)
            {
                return;
            }
            SystemBitmap bitmap = await GraphicHelper.GetBitmap(null);
            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
            LoadBitmap(bitmap);
            bitmap.Dispose();
        }

        public void Load_Click()
        {
            try
            {
                string[] result = new string[] { @"C:\Users\PC\Documents\leidian\Pictures\test.png" };

                OpenFileName ofn = new OpenFileName();

                ofn.structSize = Marshal.SizeOf(ofn);
                ofn.filter = "位图文件 (*.png;*.bmp;*.jpg)\0*.png;*.bmp;*.jpg\0";
                ofn.file = new string(new char[256]);
                ofn.maxFile = ofn.file.Length;
                ofn.fileTitle = new string(new char[64]);
                ofn.maxFileTitle = ofn.fileTitle.Length;
                ofn.initialDir = "C:\\";
                ofn.title = "请选择文件";

                if (Win32Api.GetOpenFileName(ofn))
                {

                    Debug.WriteLine("Selected file with full path: {0}", ofn.file);

                    string fileName = ofn.file;

                    var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    SystemBitmap bitmap = new SystemBitmap(stream);
                    LoadBitmap(bitmap);
                    bitmap.Dispose();
                    stream.Dispose();
                }
            }
            catch (Exception e)
            {
                Win32Api.MessageBox(e.Message, uType: 48);
            }
        }

        public void Test_Click()
        {
            if (Img!=null && ColorInfos.Count>0)
            {
                int[] sims = new int[] { 100, 95, 90, 85, 80, 0 };
                if (FormatSelectedIndex == FormatMode.compareStr || FormatSelectedIndex == FormatMode.ajCompareStr || FormatSelectedIndex == FormatMode.cdCompareStr || FormatSelectedIndex == FormatMode.diyCompareStr)
                {
                    string str = CreateColorStrHelper.Create(0, ColorInfos);
                    TestResult = GraphicHelper.CompareColorEx(str.Trim('"'), sims[SimSelectedIndex]).ToString();
                }
                else if (FormatSelectedIndex == FormatMode.anchorsCompareStr)
                {
                    double width = ColorInfo.Width;
                    double height = ColorInfo.Height;
                    string str = CreateColorStrHelper.Create(FormatMode.anchorsCompareStrTest, ColorInfos);
                    TestResult = GraphicHelper.AnchorsCompareColor(width, height, str.Trim('"'), sims[SimSelectedIndex]).ToString();
                }
                else if (FormatSelectedIndex == FormatMode.anchorsFindStr)
                {
                    Range rect = GetRange();
                    double width = ColorInfo.Width;
                    double height = ColorInfo.Height;
                    string str = CreateColorStrHelper.Create(FormatMode.anchorsFindStrTest, ColorInfos);
                    Point result = GraphicHelper.AnchorsFindColor(rect, width, height, str.Trim('"'), sims[SimSelectedIndex]);
                    if (result.X >= 0 && result.Y >= 0)
                    {
                        //Point point = e.Img.TranslatePoint(new Point(result.X, result.Y), e);
                        //FindResultMargin = new Thickness(point.X - 36, point.Y - 72, 0, 0);
                        //FindResultVisibility = Visibility.Visible;
                    }
                    TestResult = result.ToString();
                }
                else
                {
                    Range rect = GetRange();
                    string str = CreateColorStrHelper.Create(FormatMode.dmFindStr, ColorInfos, rect);
                    string[] strArray = str.Split("\",\"");
                    if (strArray[1].Length <= 3)
                    {
                        Win32Api.MessageBox("多点找色至少需要勾选两个颜色才可进行测试!", "错误");
                        TestResult = "error";
                        return;
                    }
                    string[] _str = strArray[0].Split(",\"");
                    Point result = GraphicHelper.FindMultiColor((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom, _str[^1].Trim('"'), strArray[1].Trim('"'), sims[SimSelectedIndex]);
                    if (result.X >= 0 && result.Y >= 0)
                    {
                        //Point point = e.Img.TranslatePoint(new Point(result.X, result.Y), e);
                        //FindResultMargin = new Thickness(point.X - 36, point.Y - 72, 0, 0);
                        //FindResultVisibility = Visibility.Visible;
                    }
                    TestResult = result.ToString();
                }
        }
        }

        public void Create_Click()
        {
            if (ColorInfos.Count > 0)
            {
                Range rect = GetRange();

                if (Rect.IndexOf("[") != -1)
                {
                    Rect = string.Format("[{0}]", rect.ToString());
                }
                else if (FormatSelectedIndex == FormatMode.anchorsCompareStr || FormatSelectedIndex == FormatMode.anchorsFindStr)
                {
                    Rect = rect.ToString(2);
                }
                else
                {
                    Rect = rect.ToString();
                }

                CreateString = CreateColorStrHelper.Create((FormatMode)FormatSelectedIndex, ColorInfos, rect);
            }
        }

        public async void Copy_Click()
        {
            try
            {
              await Application.Current.Clipboard.SetTextAsync(CreateString);
            }
            catch (Exception ex)
            {
                Win32Api.MessageBox("设置剪贴板失败 , 你的剪贴板可能被其他软件占用\r\n\r\n" + ex.Message, "error");
            }
        }
        public void Clear_Click()
        {
            if (CreateString == string.Empty)
            {
                ColorInfos.Clear();
                DataGridHeight = 40;
            }
            else
            {
                CreateString = string.Empty;
                Rect = string.Empty;
                TestResult = string.Empty;
            }
        }

            private Range GetRange()
        {
            if (ColorInfos.Count == 0)
            {
                return new Range(0, 0, ImgWidth, ImgHeight);
            }
            if (Rect != string.Empty)
            {
                if (Rect.IndexOf("[") != -1)
                {
                    string[] range = Rect.TrimStart('[').TrimEnd(']').Split(',');

                    return new Range(int.Parse(range[0].Trim()), int.Parse(range[1].Trim()), int.Parse(range[2].Trim()), int.Parse(range[3].Trim()));
                }
            }
            double imgWidth = ImgWidth -1;
            double imgHeight = ImgHeight -1;

            if (FormatSelectedIndex == FormatMode.anchorsFindStr || FormatSelectedIndex == FormatMode.anchorsCompareStr)
            {
                imgWidth = ColorInfo.Width;
                imgHeight = ColorInfo.Height;
            }

            double left = imgWidth;
            double top = imgHeight;
            double right = 0;
            double bottom = 0;
            int mode_1 = -1;
            int mode_2 = -1;

            foreach (ColorInfo colorInfo in ColorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (colorInfo.Point.X < left)
                    {
                        left = colorInfo.Point.X;
                        mode_1 = colorInfo.Anchor == AnchorType.Left ? 0 : colorInfo.Anchor == AnchorType.Center ? 1 : colorInfo.Anchor == AnchorType.Right ? 2 : -1;
                    }
                    if (colorInfo.Point.X > right)
                    {
                        right = colorInfo.Point.X;
                        mode_2 = colorInfo.Anchor == AnchorType.Left ? 0 : colorInfo.Anchor == AnchorType.Center ? 1 : colorInfo.Anchor == AnchorType.Right ? 2 : -1;
                    }
                    if (colorInfo.Point.Y < top)
                    {
                        top = colorInfo.Point.Y;
                    }
                    if (colorInfo.Point.Y > bottom)
                    {
                        bottom = colorInfo.Point.Y;
                    }

                }
            }
            return new Range(left >= 50 ? left - 50 : 0, top >= 50 ? top - 50 : 0, right + 50 > imgWidth ? imgWidth : right + 50, bottom + 50 > imgHeight ? imgHeight : bottom + 50, mode_1, mode_2);


        }



        public void LoadBitmap(SystemBitmap bitmap)
        {
            var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Bmp);
            stream.Position = 0;
            Img = new AvaloniaBitmap(stream);
            GraphicHelper.KeepScreen(bitmap);
            stream.Close();
            stream.Dispose();
        }
    }

}
