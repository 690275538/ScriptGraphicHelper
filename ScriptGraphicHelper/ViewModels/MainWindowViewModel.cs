using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Newtonsoft.Json;
using ScriptGraphicHelper.Converters;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.Models.ScreenshotHelpers;
using ScriptGraphicHelper.Models.UnmanagedMethods;
using ScriptGraphicHelper.ViewModels.Core;
using ScriptGraphicHelper.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Image = Avalonia.Controls.Image;
using Point = Avalonia.Point;
using Range = ScriptGraphicHelper.Models.Range;
using TabItem = ScriptGraphicHelper.Models.TabItem;

namespace ScriptGraphicHelper.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            try
            {
                StreamReader sr = File.OpenText(AppDomain.CurrentDomain.BaseDirectory + @"Assets\setting.json");
                string configStr = sr.ReadToEnd();
                sr.Close();
                configStr = configStr.Replace("\\\\", "\\").Replace("\\", "\\\\");
                Setting.Instance = JsonConvert.DeserializeObject<Setting>(configStr) ?? new Setting();
                SimSelectedIndex = Setting.Instance.SimSelectedIndex;
                FormatSelectedIndex = (FormatMode)Setting.Instance.FormatSelectedIndex;
            }
            catch { }

            ColorInfos = new ObservableCollection<ColorInfo>();
            LoupeWriteBmp = LoupeWriteBitmap.Init(241, 241);
            DataGridHeight = 40;
            Loupe_IsVisible = false;
            Rect_IsVisible = false;
            EmulatorSelectedIndex = -1;
            EmulatorInfo = ScreenshotHelperBridge.Init();
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
                    if (point.Y > 500)
                        LoupeMargin = new Thickness(point.X + 20, point.Y - 261, 0, 0);
                    else
                        LoupeMargin = new Thickness(point.X + 20, point.Y + 20, 0, 0);

                    var position = eventArgs.GetPosition((Image)parameters.Sender);
                    var imgPoint = new Point(Math.Floor(position.X / ScaleFactor), Math.Floor(position.Y / ScaleFactor));
                    PointX = (int)imgPoint.X;
                    PointY = (int)imgPoint.Y;
                    int sx = PointX - 7;
                    int sy = PointY - 7;
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
                    var position = eventArgs.GetPosition((Image)parameters.Sender);
                    var point = new Point(Math.Floor(position.X / ScaleFactor), Math.Floor(position.Y / ScaleFactor));
                    int sx = (int)(point.X - Math.Floor(RectWidth / ScaleFactor));
                    int sy = (int)(point.Y - Math.Floor(rectHeight / ScaleFactor));
                    if (RectWidth > 10 && rectHeight > 10)
                    {
                        Rect = string.Format("[{0},{1},{2},{3}]", sx, sy, Math.Min(point.X, ImgWidth - 1), Math.Min(point.Y, ImgHeight - 1));
                    }
                    else
                    {
                        byte[] color = GraphicHelper.GetPixel(sx, sy);

                        if (FormatSelectedIndex == FormatMode.anchorsFindStr || FormatSelectedIndex == FormatMode.anchorsCompareStr)
                        {
                            AnchorType anchor = AnchorType.None;
                            double quarterWidth = imgDrawWidth / 4;
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

        public ICommand GetTcpList => new Command(async (param) =>
        {
            if (ScreenshotHelperBridge.Select != -1 && ScreenshotHelperBridge.Helpers[ScreenshotHelperBridge.Select].GetType() == typeof(MoblieTcpHelper))
            {
                var helper = ScreenshotHelperBridge.Helpers[ScreenshotHelperBridge.Select] as MoblieTcpHelper;
                var result = new ObservableCollection<string>();
                ScreenshotHelperBridge.Info = await helper.GetList();
                foreach (var item in ScreenshotHelperBridge.Info)
                {
                    result.Add(item.Value);
                }
                EmulatorInfo = result;
            }
        });

        public async void Emulator_Selected(int value)
        {
            try
            {
                if (ScreenshotHelperBridge.State == LinkState.success)
                {
                    ScreenshotHelperBridge.Index = EmulatorSelectedIndex;
                }
                else if (ScreenshotHelperBridge.State == LinkState.Waiting)
                {
                    WindowCursor = new Cursor(StandardCursorType.Wait);
                    ScreenshotHelperBridge.Changed(EmulatorSelectedIndex);
                    EmulatorInfo = await ScreenshotHelperBridge.GetAll();
                    EmulatorSelectedIndex = -1;
                }
                else if (ScreenshotHelperBridge.State == LinkState.Starting)
                {
                    ScreenshotHelperBridge.State = LinkState.success;
                }
            }
            catch (Exception e)
            {
                EmulatorSelectedIndex = -1;
                ScreenshotHelperBridge.Dispose();
                EmulatorInfo.Clear();
                EmulatorInfo = ScreenshotHelperBridge.Init();
                MessageBox.ShowAsync(e.Message);
            }
            WindowCursor = new Cursor(StandardCursorType.Arrow);

        }

        public async void ScreenShot_Click()
        {
            WindowCursor = new Cursor(StandardCursorType.Wait);
            if (ScreenshotHelperBridge.Select == -1 || ScreenshotHelperBridge.Index == -1)
            {
                MessageBox.ShowAsync("请先配置 -> (模拟器/tcp/句柄)");
                WindowCursor = new Cursor(StandardCursorType.Arrow);
                return;
            }
            try
            {
                Img = await ScreenshotHelperBridge.ScreenShot();

                var item = new TabItem(Img);
                item.Command = new Command((param) =>
                {
                    TabItems.Remove(item);
                });
                TabItems.Add(item);
                TabControlSelectedIndex = TabItems.Count - 1;
            }
            catch (Exception e)
            {
                MessageBox.ShowAsync(e.Message);
            }


            WindowCursor = new Cursor(StandardCursorType.Arrow);
        }

        public void ResetEmulatorOptions_Click()
        {
            if (ScreenshotHelperBridge.State == LinkState.Starting || ScreenshotHelperBridge.State == LinkState.success)
            {
                EmulatorSelectedIndex = -1;
            }
            ScreenshotHelperBridge.Dispose();
            EmulatorInfo.Clear();
            EmulatorInfo = ScreenshotHelperBridge.Init();
        }

        public async void TurnRight_Click()
        {
            if (Img == null)
            {
                return;
            }
            Img = await GraphicHelper.TurnRight();
        }

        public void DropImage_Event(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.FileNames))
            {
                foreach (var name in e.Data.GetFileNames())
                {
                    if (name != "" && name != string.Empty)
                    {
                        var stream = new FileStream(name, FileMode.Open, FileAccess.Read);
                        Img = new Bitmap(stream);
                        stream.Position = 0;
                        SKBitmap sKBitmap = SKBitmap.Decode(stream);
                        GraphicHelper.KeepScreen(sKBitmap);
                        sKBitmap.Dispose();
                        stream.Dispose();

                        var item = new TabItem(Img);
                        item.Command = new Command((param) =>
                        {
                            TabItems.Remove(item);
                        });
                        TabItems.Add(item);
                        TabControlSelectedIndex = TabItems.Count - 1;
                    }
                }
            }
        }

        public async void Load_Click()
        {
            try
            {
                string fileName = string.Empty;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    OpenFileName ofn = new();
                    ofn.hwnd = MainWindow.Instance.Handle;
                    ofn.structSize = Marshal.SizeOf(ofn);
                    ofn.filter = "位图文件 (*.png;*.bmp;*.jpg)\0*.png;*.bmp;*.jpg\0";
                    ofn.file = new string(new char[256]);
                    ofn.maxFile = ofn.file.Length;
                    ofn.fileTitle = new string(new char[64]);
                    ofn.maxFileTitle = ofn.fileTitle.Length;
                    ofn.title = "请选择文件";

                    if (NativeApi.GetOpenFileName(ofn))
                    {
                        fileName = ofn.file;
                    }
                }
                else
                {
                    var dlg = new OpenFileDialog
                    {
                        Title = "请选择文件",
                        AllowMultiple = false,
                        Filters = new List<FileDialogFilter>
                        {
                            new FileDialogFilter
                            {
                                Name = "位图文件",
                                Extensions = new List<string>()
                                {
                                "png",
                                "bmp",
                                "jpg"
                                }
                            }
                        }
                    };
                    var fileNames = await dlg.ShowAsync(MainWindow.Instance);
                    if (fileNames.Length != 0)
                    {
                        fileName = fileNames[0];
                    }
                }

                if (fileName != "" && fileName != string.Empty)
                {
                    var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    Img = new Bitmap(stream);
                    stream.Position = 0;
                    SKBitmap sKBitmap = SKBitmap.Decode(stream);
                    GraphicHelper.KeepScreen(sKBitmap);
                    sKBitmap.Dispose();
                    stream.Dispose();

                    var item = new TabItem(Img);
                    item.Command = new Command((param) =>
                    {
                        TabItems.Remove(item);
                    });
                    TabItems.Add(item);
                    TabControlSelectedIndex = TabItems.Count - 1;
                }
            }
            catch (Exception e)
            {
                MessageBox.ShowAsync(e.Message);
            }
        }

        public async void Save_Click()
        {
            if (Img == null)
            {
                return;
            }

            try
            {
                string fileName = string.Empty;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    OpenFileName ofn = new();

                    ofn.hwnd = MainWindow.Instance.Handle;
                    ofn.structSize = Marshal.SizeOf(ofn);
                    ofn.filter = "位图文件 (*.png;*.bmp;*.jpg)\0*.png;*.bmp;*.jpg\0";
                    ofn.file = new string(new char[256]);
                    ofn.maxFile = ofn.file.Length;
                    ofn.fileTitle = new string(new char[64]);
                    ofn.maxFileTitle = ofn.fileTitle.Length;
                    ofn.title = "保存文件";
                    ofn.defExt = ".png";
                    if (NativeApi.GetSaveFileName(ofn))
                    {
                        fileName = ofn.file;
                    }
                }
                else
                {
                    var dlg = new SaveFileDialog
                    {
                        InitialFileName = "Screen_" + DateTime.Now.ToString("yy-MM-dd-HH-mm-ss"),
                        Title = "保存文件",
                        Filters = new List<FileDialogFilter>
                        {
                            new FileDialogFilter
                            {
                                Name = "位图文件",
                                Extensions = new List<string>()
                                {
                                    "png",
                                    "bmp",
                                    "jpg"
                                }
                            }
                        }
                    };
                    fileName = await dlg.ShowAsync(MainWindow.Instance);
                }


                if (fileName != null && fileName != "" && fileName != string.Empty)
                {
                    Img.Save(fileName);
                }
            }
            catch (Exception e)
            {
                MessageBox.ShowAsync(e.Message);
            }
        }

        public async void Test_Click()
        {
            if (Img != null && ColorInfos.Count > 0)
            {
                int[] sims = new int[] { 100, 95, 90, 85, 80, 0 };
                if (FormatSelectedIndex == FormatMode.compareStr || FormatSelectedIndex == FormatMode.anjianCompareStr || FormatSelectedIndex == FormatMode.cdCompareStr || FormatSelectedIndex == FormatMode.diyCompareStr)
                {
                    string str = CreateColorStrHelper.Create(FormatMode.compareStr, ColorInfos);

                    var result = GraphicHelper.CompareColorEx(str.Trim('"'), sims[SimSelectedIndex]);
                    if (!result.Result)
                    {
                        MessageBox.ShowAsync(result.ErrorMessage);
                    }
                    TestResult = result.Result.ToString();
                }
                else if (FormatSelectedIndex == FormatMode.anchorsCompareStr)
                {
                    double width = ColorInfo.Width;
                    double height = ColorInfo.Height;
                    string str = CreateColorStrHelper.Create(FormatMode.anchorsCompareStr4Test, ColorInfos);

                    var result = GraphicHelper.AnchorsCompareColor(width, height, str.Trim('"'), sims[SimSelectedIndex]);
                    if (!result.Result)
                    {
                        MessageBox.ShowAsync(result.ErrorMessage);
                    }
                    TestResult = result.Result.ToString();
                }
                else if (FormatSelectedIndex == FormatMode.anchorsFindStr)
                {
                    Range rect = GetRange();
                    double width = ColorInfo.Width;
                    double height = ColorInfo.Height;
                    string str = CreateColorStrHelper.Create(FormatMode.anchorsFindStr4Test, ColorInfos);
                    Point result = GraphicHelper.AnchorsFindColor(rect, width, height, str.Trim('"'), sims[SimSelectedIndex]);

                    TestResult = result.ToString();

                    if (result.X >= 0 && result.Y >= 0)
                    {
                        FindedPoint_Margin = new Thickness(result.X - 36, result.Y - 69, 0, 0);
                        FindedPoint_IsVisible = true;
                        await Task.Delay(2500);
                        FindedPoint_IsVisible = false;
                    }
                }
                else
                {
                    Range rect = GetRange();
                    string str = CreateColorStrHelper.Create(FormatMode.findStr4Test, ColorInfos, rect);
                    string[] strArray = str.Split("\",\"");
                    if (strArray[1].Length <= 3)
                    {
                        MessageBox.ShowAsync("多点找色至少需要勾选两个颜色才可进行测试!", "错误");
                        TestResult = "error";
                        return;
                    }
                    string[] _str = strArray[0].Split(",\"");
                    Point result = GraphicHelper.FindMultiColor((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom, _str[^1].Trim('"'), strArray[1].Trim('"'), sims[SimSelectedIndex]);

                    TestResult = result.ToString();

                    if (result.X >= 0 && result.Y >= 0)
                    {
                        FindedPoint_Margin = new(result.X * ScaleFactor - 36, result.Y * ScaleFactor - 69, 0, 0);
                        FindedPoint_IsVisible = true;
                        await Task.Delay(2500);
                        FindedPoint_IsVisible = false;
                    }
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

                CreateStr = CreateColorStrHelper.Create(FormatSelectedIndex, ColorInfos, rect);
                CreateStr_Copy_Click();
            }
        }

        public async void CreateStr_Copy_Click()
        {
            try
            {
                await Application.Current.Clipboard.SetTextAsync(CreateStr);
            }
            catch (Exception ex)
            {
                MessageBox.ShowAsync("设置剪贴板失败 , 你的剪贴板可能被其他软件占用\r\n\r\n" + ex.Message, "error");
            }
        }

        public void Clear_Click()
        {
            if (CreateStr == string.Empty && Rect == string.Empty)
            {
                ColorInfos.Clear();
                DataGridHeight = 40;
            }
            else
            {
                CreateStr = string.Empty;
                Rect = string.Empty;
                TestResult = string.Empty;
            }
        }

        public ICommand Key_AddColorInfo => new Command((param) =>
        {
            if (!Loupe_IsVisible)
            {
                return;
            }
            int x = (int)PointX;
            int y = (int)PointY;
            string key = (string)param;
            byte[] color = GraphicHelper.GetPixel(x, y);

            AnchorType anchor = AnchorType.None;
            if (FormatSelectedIndex == FormatMode.anchorsFindStr || FormatSelectedIndex == FormatMode.anchorsCompareStr)
            {
                if (key == "A")
                    anchor = AnchorType.Left;
                else if (key == "S")
                    anchor = AnchorType.Center;
                else if (key == "D")
                    anchor = AnchorType.Right;
            }
            ColorInfos.Add(new ColorInfo(ColorInfos.Count, anchor, x, y, color));
            DataGridHeight = (ColorInfos.Count + 1) * 40;
        });

        public ICommand Key_ScaleFactorChanged => new Command((param) =>
        {
            var num = ScaleFactor switch
            {
                0.4 => 0,
                0.6 => 1,
                0.8 => 2,
                1.0 => 3,
                1.5 => 4,
                2.0 => 5,
                2.5 => 6,
                3.0 => 7,
                _ => 3
            };

            if (param.ToString() == "Add")
            {
                num++;
            }
            else if (param.ToString() == "Subtract")
            {
                num--;
            }
            else
            {
                if (num == 0)
                {
                    num = 7;
                }
                else
                {
                    num--;
                }
            }
            num = Math.Min(num, 7);
            num = Math.Max(num, 0);
            ScaleFactor = num switch
            {
                0 => 0.4,
                1 => 0.6,
                2 => 0.8,
                3 => 1.0,
                4 => 1.5,
                5 => 2.0,
                6 => 2.5,
                7 => 3.0,
                _ => 1.0
            };

        });

        public async void Key_GetClipboardData()
        {
            string[] formats = await Application.Current.Clipboard.GetFormatsAsync();
            string fileName = string.Empty;

            if (Array.IndexOf(formats, "FileNames") != -1)
            {
                var fileNames = (List<string>)await Application.Current.Clipboard.GetDataAsync(DataFormats.FileNames);
                if (fileNames.Count != 0)
                {
                    fileName = fileNames[0];
                }
            }

            if (fileName.IndexOf(".bmp") != -1 || fileName.IndexOf(".png") != -1 || fileName.IndexOf(".jpg") != -1)
            {
                var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                Img = new Bitmap(stream);
                stream.Position = 0;
                SKBitmap sKBitmap = SKBitmap.Decode(stream);
                GraphicHelper.KeepScreen(sKBitmap);
                sKBitmap.Dispose();
                stream.Dispose();

                var item = new TabItem(Img);
                item.Command = new Command((param) =>
                {
                    TabItems.Remove(item);
                });
                TabItems.Add(item);
                TabControlSelectedIndex = TabItems.Count - 1;
            }
            else
            {
                string text = await Application.Current.Clipboard.GetTextAsync();
                if (text != string.Empty)
                {
                    ColorInfos.Clear();
                    int[] sims = new int[] { 100, 95, 90, 85, 80, 0 };
                    int sim = sims[SimSelectedIndex];
                    if (sim == 0)
                    {
                        sim = Setting.Instance.DiySim;
                    }
                    var result = DataImportHelper.Import(text);

                    double similarity = (255 - 255 * (sim / 100.0)) / 2;
                    for (int i = 0; i < result.Count; i++)
                    {
                        if (GraphicHelper.CompareColor(new byte[] { result[i].Color.R, result[i].Color.G, result[i].Color.B }, similarity, (int)result[i].Point.X, (int)result[i].Point.Y, 0))
                        {
                            result[i].IsChecked = true;
                        }
                        ColorInfos.Add(result[i]);
                    }
                    DataGridHeight = (ColorInfos.Count + 1) * 40;
                }
            }
        }

        public void Key_ColorInfo_Clear()
        {
            ColorInfos.Clear();
            DataGridHeight = 40;
        }

        public async void Key_SetConfig()
        {
            var config = new Config();
            var setting = Setting.Instance;
            string ysPath = setting.YsPath;
            string xyPath = setting.XyPath;
            string ldpath3 = setting.Ldpath3;
            string ldpath4 = setting.Ldpath4;
            string ldpath64 = setting.Ldpath64;

            await config.ShowDialog(MainWindow.Instance);

            if (ysPath != setting.YsPath || xyPath != setting.XyPath || ldpath3 != setting.Ldpath3 || ldpath4 != setting.Ldpath4 || ldpath64 != setting.Ldpath64)
            {
                ResetEmulatorOptions_Click();
            }
        }

        public async void Rect_Copy_Click()
        {
            try
            {
                await Application.Current.Clipboard.SetTextAsync(Rect);
            }
            catch (Exception ex)
            {
                MessageBox.ShowAsync("设置剪贴板失败 , 你的剪贴板可能被其他软件占用\r\n\r\n" + ex.Message, "error");
            }
        }

        public void Rect_Clear_Click()
        {
            Rect = string.Empty;
        }

        public async void Point_Copy_Click()
        {
            try
            {
                if (DataGridSelectedIndex == -1 || DataGridSelectedIndex > ColorInfos.Count)
                {
                    return;
                }
                Point point = ColorInfos[DataGridSelectedIndex].Point;
                string pointStr = string.Format("{0},{1}", point.X, point.Y);
                await Application.Current.Clipboard.SetTextAsync(pointStr);
            }
            catch (Exception ex)
            {
                MessageBox.ShowAsync("设置剪贴板失败\r\n\r\n" + ex.Message, "错误");
            }
        }

        public async void Color_Copy_Click()
        {
            try
            {
                if (DataGridSelectedIndex == -1 || DataGridSelectedIndex > ColorInfos.Count)
                {
                    return;
                }
                Color color = ColorInfos[DataGridSelectedIndex].Color;
                string hexColor = string.Format("#{0}{1}{2}", color.R.ToString("X2"), color.G.ToString("X2"), color.B.ToString("X2"));
                await Application.Current.Clipboard.SetTextAsync(hexColor);
            }
            catch (Exception ex)
            {
                MessageBox.ShowAsync("设置剪贴板失败\r\n\r\n" + ex.Message, "错误");
            }
        }

        public void ColorInfo_Reset_Click()
        {
            var temp = new ObservableCollection<ColorInfo>();

            foreach (var colorInfo in ColorInfos)
            {
                int x = (int)colorInfo.Point.X;
                int y = (int)colorInfo.Point.Y;
                byte[] color = GraphicHelper.GetPixel(x, y);
                colorInfo.Color = Color.FromRgb(color[0], color[1], color[2]);
                if (x >= ImgWidth || y >= ImgHeight)
                {
                    colorInfo.IsChecked = false;
                }
                temp.Add(colorInfo);
            }

            ColorInfos = temp;

        }

        public void ColorInfo_SelectItemClear_Click()
        {
            if (DataGridSelectedIndex == -1 || DataGridSelectedIndex > ColorInfos.Count)
            {
                return;
            }
            ColorInfos.RemoveAt(DataGridSelectedIndex);
            DataGridHeight = (ColorInfos.Count + 1) * 40;
        }

        public async void CutImg_Click()
        {
            Range range = GetRange();
            var colorInfos = new List<ColorInfo>();
            var imgEditor = new ImgEditor(range, GraphicHelper.GetRectData(range));
            await imgEditor.ShowDialog(MainWindow.Instance);
            if (ImgEditor.Result_ACK && ImgEditor.ResultColorInfos != null && ImgEditor.ResultColorInfos.Count != 0)
            {
                ColorInfos = new ObservableCollection<ColorInfo>(ImgEditor.ResultColorInfos);
                ImgEditor.ResultColorInfos.Clear();
                ImgEditor.Result_ACK = false;
                DataGridHeight = (ColorInfos.Count + 1) * 40;
            }
        }

        private Range GetRange()
        {
            //if (ColorInfos.Count == 0)
            //{
            //    return new Range(0, 0, ImgWidth - 1, ImgHeight - 1);
            //}
            if (Rect != string.Empty)
            {
                if (Rect.IndexOf("[") != -1)
                {
                    string[] range = Rect.TrimStart('[').TrimEnd(']').Split(',');

                    return new Range(int.Parse(range[0].Trim()), int.Parse(range[1].Trim()), int.Parse(range[2].Trim()), int.Parse(range[3].Trim()));
                }
            }
            double imgWidth = ImgWidth - 1;
            double imgHeight = ImgHeight - 1;

            if (FormatSelectedIndex == FormatMode.anchorsFindStr || FormatSelectedIndex == FormatMode.anchorsCompareStr)
            {
                imgWidth = ColorInfo.Width - 1;
                imgHeight = ColorInfo.Height - 1;
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
            var tolerance = Setting.Instance.RangeTolerance;
            return new Range(left >= tolerance ? left - tolerance : 0, top >= tolerance ? top - tolerance : 0, right + tolerance > imgWidth ? imgWidth : right + tolerance, bottom + tolerance > imgHeight ? imgHeight : bottom + tolerance, mode_1, mode_2);

        }
    }

}
