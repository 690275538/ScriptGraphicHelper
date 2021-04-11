using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.Models.UnmanagedMethods;
using ScriptGraphicHelper.ViewModels.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ScriptGraphicHelper.ViewModels
{
    public class ImgEditorViewModel : ViewModelBase
    {
        private int windowWidth;
        public int WindowWidth
        {
            get => windowWidth;
            set => this.RaiseAndSetIfChanged(ref windowWidth, value);
        }

        private int windowHeight;
        public int WindowHeight
        {
            get => windowHeight;
            set => this.RaiseAndSetIfChanged(ref windowHeight, value);
        }

        private WriteableBitmap drawBitmap;
        public WriteableBitmap DrawBitmap
        {
            get => drawBitmap;
            set => this.RaiseAndSetIfChanged(ref drawBitmap, value);
        }

        private int imgWidth;
        public int ImgWidth
        {
            get => imgWidth;
            set => this.RaiseAndSetIfChanged(ref imgWidth, value);
        }

        private int imgHeight;
        public int ImgHeight
        {
            get => imgHeight;
            set => this.RaiseAndSetIfChanged(ref imgHeight, value);
        }

        private Color srcColor = Colors.White;
        public Color SrcColor
        {
            get => srcColor;
            set => this.RaiseAndSetIfChanged(ref srcColor, value);
        }

        private Color destColor = Colors.Red;
        public Color DestColor
        {
            get => destColor;
            set => this.RaiseAndSetIfChanged(ref destColor, value);
        }

        private bool pen_IsChecked;
        public bool Pen_IsChecked
        {
            get => pen_IsChecked;
            set => this.RaiseAndSetIfChanged(ref pen_IsChecked, value);
        }

        private int tolerance = 5;
        public int Tolerance
        {
            get => tolerance;
            set
            {
                this.RaiseAndSetIfChanged(ref tolerance, value);
                DrawBitmap = ImgEditorHelper.ResetImg();
                DrawBitmap.SetPixels(SrcColor, destColor, Tolerance, reverse_IsChecked);
                ImgWidth -= 1;
                ImgHeight -= 1;
                ImgWidth += 1;
                ImgHeight += 1;
            }
        }

        private bool reverse_IsChecked;
        public bool Reverse_IsChecked
        {
            get => reverse_IsChecked;
            set => this.RaiseAndSetIfChanged(ref reverse_IsChecked, value);
        }

        public ImgEditorViewModel(Models.Range range, byte[] data)
        {
            DrawBitmap = ImgEditorHelper.Init(range, data);
            ImgWidth = (int)DrawBitmap.Size.Width * 3;
            ImgHeight = (int)DrawBitmap.Size.Height * 3;
            WindowWidth = ImgWidth + 180;
            WindowHeight = ImgHeight + 40;
        }

        public async void CutImg_Click()
        {
            DrawBitmap = DrawBitmap.CutImg();
            ImgWidth = (int)DrawBitmap.Size.Width * 3;
            ImgHeight = (int)DrawBitmap.Size.Height * 3;
        }

        public void Reset_Click()
        {
            DrawBitmap = ImgEditorHelper.ResetImg();
        }

        public void Save_Click()
        {
            try
            {
                OpenFileName ofn = new();
                ofn.structSize = Marshal.SizeOf(ofn);
                ofn.filter = "位图文件 (*.bmp;*.png;*.jpg)\0*.bmp;*.png;*.jpg\0";
                ofn.file = new string(new char[256]);
                ofn.maxFile = ofn.file.Length;
                ofn.fileTitle = new string(new char[64]);
                ofn.maxFileTitle = ofn.fileTitle.Length;
                ofn.title = "保存文件";
                ofn.defExt = ".bmp";
                if (Win32Api.GetSaveFileName(ofn))
                {
                    string fileName = ofn.file;
                    if (fileName.IndexOf("bmp")!=-1)
                    {
                        var bitmap = DrawBitmap.GetBitmap();
                        bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);
                    }
                    else
                    {
                        DrawBitmap.Save(fileName);
                    }
                }
            }
            catch (Exception e)
            {
                Win32Api.MessageBox(e.Message, uType: 48);
            }
        }


        private bool IsDown = false;
        public ICommand Img_PointerPressed => new Command(async (param) =>
        {
            IsDown = true;
            if (pen_IsChecked)
            {
                if (param != null)
                {
                    CommandParameters parameters = (CommandParameters)param;
                    var eventArgs = (PointerPressedEventArgs)parameters.EventArgs;
                    if (eventArgs.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
                    {
                        var point = eventArgs.GetPosition((Image)parameters.Sender);
                        int x = (int)point.X / 3;
                        int y = (int)point.Y / 3;

                        for (int i = -1; i < 2; i++)
                        {
                            for (int j = -1; j < 2; j++)
                            {
                                await DrawBitmap.SetPixel(x + i, y + j, DestColor);
                            }
                        }
                        int width = (int)DrawBitmap.Size.Width * 3;
                        int height = (int)DrawBitmap.Size.Height * 3;

                        ImgWidth -= 1;
                        ImgHeight -= 1;
                        ImgWidth += 1;
                        ImgHeight += 1;
                        //Image控件不会自动刷新, 解决方案是改变一次宽高, 可能是bug https://github.com/AvaloniaUI/Avalonia/issues/1995
                    }
                }
            }

        });

        public ICommand Img_PointerMoved => new Command(async (param) =>
        {
            if (pen_IsChecked && IsDown)
            {
                if (param != null)
                {
                    CommandParameters parameters = (CommandParameters)param;
                    var eventArgs = (PointerEventArgs)parameters.EventArgs;
                    var point = eventArgs.GetPosition((Image)parameters.Sender);
                    int x = (int)point.X / 3;
                    int y = (int)point.Y / 3;
                    for (int i = -1; i < 2; i++)
                    {
                        for (int j = -1; j < 2; j++)
                        {
                            await DrawBitmap.SetPixel(x + i, y + j, DestColor);
                        }
                    }
                    ImgWidth -= 1;
                    ImgHeight -= 1;
                    ImgWidth += 1;
                    ImgHeight += 1;
                }
            }
        });

        public ICommand Img_PointerReleased => new Command(async (param) =>
        {

            if (IsDown && !Pen_IsChecked)
            {
                if (param != null)
                {
                    CommandParameters parameters = (CommandParameters)param;
                    var eventArgs = (PointerEventArgs)parameters.EventArgs;
                    var point = eventArgs.GetPosition((Image)parameters.Sender);
                    int x = (int)point.X / 3;
                    int y = (int)point.Y / 3;
                    SrcColor = await DrawBitmap.GetPixel(x, y);
                    DrawBitmap.SetPixels(SrcColor, destColor, Tolerance, reverse_IsChecked);
                    ImgWidth -= 1;
                    ImgHeight -= 1;
                    ImgWidth += 1;
                    ImgHeight += 1;
                }
            }
            IsDown = false;
        });

    }
}
