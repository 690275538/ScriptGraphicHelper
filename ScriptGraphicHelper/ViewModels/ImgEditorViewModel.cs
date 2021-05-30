﻿using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.Models.UnmanagedMethods;
using ScriptGraphicHelper.ViewModels.Core;
using ScriptGraphicHelper.Views;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
                ImgWidth += 1;
            }
        }

        private bool reverse_IsChecked;
        public bool Reverse_IsChecked
        {
            get => reverse_IsChecked;
            set => this.RaiseAndSetIfChanged(ref reverse_IsChecked, value);
        }

        private bool getColorInfosBtnState;
        public bool GetColorInfosBtnState
        {
            get => getColorInfosBtnState;
            set => this.RaiseAndSetIfChanged(ref getColorInfosBtnState, value);
        }

        private int getColorInfosModeSelectedIndex;
        public int GetColorInfosModeSelectedIndex
        {
            get => getColorInfosModeSelectedIndex;
            set {
                this.RaiseAndSetIfChanged(ref getColorInfosModeSelectedIndex, value);
                Setting.Instance.GetColorInfosConfig.ModeSelectedIndex = value;
            }
        }

        private int getColorInfosThreshold;
        public int GetColorInfosThreshold
        {
            get => getColorInfosThreshold;
            set {
                this.RaiseAndSetIfChanged(ref getColorInfosThreshold, value);
                Setting.Instance.GetColorInfosConfig.Threshold = value;
            }
        }

        private int getColorInfosSize;
        public int GetColorInfosSize
        {
            get => getColorInfosSize;
            set {
                this.RaiseAndSetIfChanged(ref getColorInfosSize, value);
                Setting.Instance.GetColorInfosConfig.Size = value;
            } 
        }

        public ImgEditorViewModel(Models.Range range, byte[] data)
        {
            DrawBitmap = ImgEditorHelper.Init(range, data);
            ImgWidth = (int)DrawBitmap.Size.Width * 5;
            ImgHeight = (int)DrawBitmap.Size.Height * 5;
            WindowWidth = ImgWidth + 320;
            WindowHeight = ImgHeight + 40;

            GetColorInfosBtnState = true;
            GetColorInfosModeSelectedIndex = Setting.Instance.GetColorInfosConfig.ModeSelectedIndex;
            GetColorInfosSize = Setting.Instance.GetColorInfosConfig.Size;
            GetColorInfosThreshold = Setting.Instance.GetColorInfosConfig.Threshold;

            ImgEditorHelper.StartX = (int)range.Left;
            ImgEditorHelper.StartY = (int)range.Top;
        }

        public void CutImg_Click()
        {
            DrawBitmap = DrawBitmap.CutImg();
            ImgWidth = (int)DrawBitmap.Size.Width * 5;
            ImgHeight = (int)DrawBitmap.Size.Height * 5;

            WindowWidth = ImgWidth + 320;
            WindowHeight = ImgHeight + 40;
        }

        public void Reset_Click()
        {
            DrawBitmap = ImgEditorHelper.ResetImg();
        }

        public async void Save_Click()
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
                        Extensions = { "png", "bmp","jpg"}
                    }
                }
            };
            string fileName = await dlg.ShowAsync(MainWindow.Instance);

            if (fileName != null && fileName != "" && fileName != string.Empty)
            {
                if (fileName.IndexOf("bmp") != -1 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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
                        int x = (int)point.X / 5;
                        int y = (int)point.Y / 5;

                        for (int i = -1; i < 2; i++)
                        {
                            for (int j = -1; j < 2; j++)
                            {
                                await DrawBitmap.SetPixel(x + i, y + j, DestColor);
                            }
                        }
                        int width = (int)DrawBitmap.Size.Width * 5;
                        int height = (int)DrawBitmap.Size.Height * 5;

                        ImgWidth -= 1;
                        ImgWidth += 1;
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
                    int x = (int)point.X / 5;
                    int y = (int)point.Y / 5;
                    for (int i = -1; i < 2; i++)
                    {
                        for (int j = -1; j < 2; j++)
                        {
                            await DrawBitmap.SetPixel(x + i, y + j, DestColor);
                        }
                    }
                    ImgWidth -= 1;
                    ImgWidth += 1;
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
                    int x = (int)point.X / 5;
                    int y = (int)point.Y / 5;
                    SrcColor = await DrawBitmap.GetPixel(x, y);
                    DrawBitmap.SetPixels(SrcColor, destColor, Tolerance, reverse_IsChecked);
                    ImgWidth -= 1;
                    ImgWidth += 1;
                }
            }
            IsDown = false;
        });

        public ICommand GetColorInfos_Click => new Command(async (param) =>
        {
            GetColorInfosBtnState = false;

            CutImg_Click();
            if (getColorInfosModeSelectedIndex == 0)
            {
                ImgEditor.ResultColorInfos = await DrawBitmap.GetAllColorInfos(GetColorInfosSize);
            }
            else
            {
                ImgEditor.ResultColorInfos = await DrawBitmap.GetColorInfos(GetColorInfosSize, GetColorInfosThreshold);
            }
            ImgWidth -= 1;
            ImgWidth += 1;
            await Task.Delay(1000);
            Reset_Click();

            GetColorInfosBtnState = true;
        });
    }
}
