using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ScriptGraphicHelper.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models.ScreenshotHelpers
{
    class ATHelper : BaseHelper
    {
        public override string Path { get; } = "AT连接";
        public override string Name { get; } = "AT连接";
        public override Action<Bitmap>? SuccessCallBack { get; set; }
        public override Action<string>? FailCallBack { get; set; }

        public string LocalIP { get; set; } = string.Empty;

        public string RemoteIP { get; set; } = string.Empty;

        private TcpClient? Client;

        private string deviceName = "null";

        public ATHelper()
        {

        }

        public override async Task<List<KeyValuePair<int, string>>> Initialize()
        {
            var config = new AJConfig(Util.GetLocalAddress());

            var result = await config.ShowDialog<(string, string)?>(MainWindow.Instance);

            if (result != null)
            {
                this.LocalIP = result.Value.Item1;
                this.RemoteIP = result.Value.Item2;

                await Task.Run(() =>
               {
                   try
                   {
                       this.Client = new TcpClient(this.RemoteIP, 1024);
                       ConnectAsync(this.Client);
                   }
                   catch (Exception ex)
                   {
                       MessageBox.ShowAsync(ex.ToString());
                   }
               });
            }

            var list = new List<KeyValuePair<int, string>>
                {
                    new KeyValuePair<int, string>(key: 0, value: "null")
                };

            return list;
        }

        private void ConnectAsync(TcpClient client)
        {
            _ = Task.Run(async () =>
            {
                var stream = client.GetStream();
                while (true)
                {
                    try
                    {
                        Thread.Sleep(50);
                        var data = await Stick.ReadPackAsync(stream);

                        switch (data.Key)
                        {
                            case "init":
                            {
                                this.deviceName = data.Description;
                                break;
                            }
                            case "screenShot_success":
                            {
                                var sKBitmap = SKBitmap.Decode(data.Buffer);
                                var pxFormat = sKBitmap.ColorType == SKColorType.Rgba8888 ? PixelFormat.Rgba8888 : PixelFormat.Bgra8888;
                                var bitmap = new Bitmap(pxFormat, AlphaFormat.Opaque, sKBitmap.GetPixels(), new PixelSize(sKBitmap.Width, sKBitmap.Height), new Vector(96, 96), sKBitmap.RowBytes);

                                this.SuccessCallBack?.Invoke(bitmap);
                                break;
                            }
                            case "screenShot_fail":
                            {
                                this.FailCallBack?.Invoke(data.Description);
                                break;
                            }
                        }
                    }
                    catch
                    {

                    }
                }
            });
        }



        public override async Task<List<KeyValuePair<int, string>>> GetList()
        {
            return await Task.Run(() =>
            {
                var result = new List<KeyValuePair<int, string>>
                 {
                     new KeyValuePair<int, string>(key: 0, value: this.deviceName)
                 };
                return result;
            });
        }


        public override void ScreenShot(int Index)
        {
            if (this.Client == null)
            {
                throw new Exception("tcp连接失效");
            }

            var stream = this.Client.GetStream();

            var pack = Stick.MakePackData("screenShot");
            stream.Write(pack);
        }

        public override bool IsStart(int Index)
        {
            return true;
        }


        public override void Close()
        {
            try
            {
                this.Client?.Close();
                this.Client?.Dispose();
            }
            catch { };
        }
    }
}
