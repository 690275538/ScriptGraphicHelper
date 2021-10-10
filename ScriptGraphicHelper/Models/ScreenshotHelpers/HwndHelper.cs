using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ScriptGraphicHelper.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models.ScreenshotHelpers
{
    class HwndHelper : BaseScreenshotHelper
    {
        public override string Path { get; set; } = "大漠句柄";
        public override string Name { get; set; } = "大漠句柄";

        private bool Inited = false;

        Dmsoft Dm;

        public override void Dispose()
        {
            try
            {
                this.Dm.UnBindWindow();
            }
            catch { }
        }

        public override bool IsStart(int Index)
        {
            return this.Inited;
        }

        public override async Task<List<KeyValuePair<int, string>>> ListAll()
        {
            this.Dm = Dmsoft.Instance;
            this.Dm.Hwnd = -1;
            HwndConfig config = new();
            await config.ShowDialog(MainWindow.Instance);
            var task = Task.Run(() =>
            {
                List<KeyValuePair<int, string>> result = new();
                if (this.Dm.Hwnd == -1)
                {
                    result.Add(new KeyValuePair<int, string>(key: 0, value: "null"));
                    return result;
                }
                if (this.Dm.BindWindowEx() == 1)
                {
                    this.Inited = true;
                    result.Add(new KeyValuePair<int, string>(key: 0, value: this.Dm.Hwnd.ToString() + "-" + this.Dm.Display));
                    return result;
                }
                result.Add(new KeyValuePair<int, string>(key: 0, value: "null"));
                return result;
            });
            return await task;
        }

        public override async Task<Bitmap> ScreenShot(int Index)
        {
            if (Index == -1 || !this.Inited)
            {
                throw new Exception("请先选择窗口句柄!");
            }
            var task = Task.Run(() =>
            {
                try
                {
                    var point = this.Dm.GetClientSize();
                    var width = (int)point.X;
                    var height = (int)point.Y;
                    var data = this.Dm.GetScreenData(width, height);

                    SKBitmap sKBitmap = new(new SKImageInfo(width, height));
                    Marshal.Copy(data, 0, sKBitmap.GetPixels(), data.Length);
                    GraphicHelper.KeepScreen(sKBitmap);
                    var bitmap = new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Opaque, sKBitmap.GetPixels(), new PixelSize(width, height), new Vector(96, 96), sKBitmap.RowBytes);
                    sKBitmap.Dispose();
                    return bitmap;

                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            });
            return await task;
        }
    }
}
