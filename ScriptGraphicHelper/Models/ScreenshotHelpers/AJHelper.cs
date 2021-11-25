using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScriptGraphicHelper.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models.ScreenshotHelpers
{
    internal class RunData
    {
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string command { get; set; } = "run";
        public string script { get; set; } = string.Empty;
    }

    internal class RunCommand
    {
        public int id { get; set; }
        public string type { get; set; } = "command";
        public RunData? data { get; set; } = null;
    }

    class AJHelper : BaseHelper
    {
        public override string Path { get; } = "AJ连接";
        public override string Name { get; } = "AJ连接";
        public override Action<Bitmap>? Action { get; set; }

        public string LocalIP { get; set; } = string.Empty;

        public string RemoteIP { get; set; } = string.Empty;

        private TcpListener? Server;

        private TcpClient? Client;

        private int runStep;

        private string runCode = string.Empty;

        private string deviceName { get; set; } = string.Empty;

        public AJHelper()
        {

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"assets/script.js"))
            {
                var sr = File.OpenText(AppDomain.CurrentDomain.BaseDirectory + @"assets/script.js");
                this.runCode = sr.ReadToEnd();
                this.runStep = 0;
            }
            else
            {
                throw new FileNotFoundException($"{AppDomain.CurrentDomain.BaseDirectory}assets/script.js");
            }
        }

        private byte[] GetRunCommandBytes(string runCode, string id)
        {
            RunCommand command = new()
            {
                id = runStep,
                data = new RunData()
                {
                    id = id,
                    name = id,
                    script = runCode
                }
            };
            this.runStep++;
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command));
            var recv = new byte[data.Length + 8];
            var len = data.Length.ToBytes();
            len.CopyTo(recv, 0);
            recv[7] = 1;
            data.CopyTo(recv, 8);
            return recv;
        }

        public override async Task<List<KeyValuePair<int, string>>> Initialize()
        {
            var config = new AJConfig(Util.GetLocalAddress());

            var result = await config.ShowDialog<(string,string)?>(MainWindow.Instance);

            if (result != null)
            {
                LocalIP = result.Value.Item1;
                RemoteIP = result.Value.Item2;

                await Task.Run(async () =>
                {
                    try
                    {
                        this.Client = new TcpClient(RemoteIP, 9317);
                        var networkStream = this.Client.GetStream();
                        for (var i = 0; i < 50; i++)
                        {
                            Thread.Sleep(100);
                            if (networkStream.DataAvailable)
                            {

                                var buf = new byte[256];
                                var len = networkStream.Read(buf, 0, 256);
                                var info = Encoding.UTF8.GetString(buf, 8, len - 8);

                                var obj = (JObject?)JsonConvert.DeserializeObject(info);
                                if (obj != null)
                                {
                                    var data = (JObject?)obj.GetValue("data");
                                    if (data != null)
                                    {
                                        var name = (string?)data.GetValue("device_name");
                                        if (name != null)
                                        {
                                            this.deviceName = name;
                                        }
                                    }
                                }

                                var send = new byte[59]
                                {
                                    0x00,0x00,0x00,0x33,0x00,0x00,0x00,0x01,
                                    0x7B,0x22,0x69,0x64,0x22,0x3A,0x31,0x2C,
                                    0x22,0x74,0x79,0x70,0x65,0x22,0x3A,0x22,
                                    0x68,0x65,0x6C,0x6C,0x6F,0x22,0x2C,0x22,
                                    0x64,0x61,0x74,0x61,0x22,0x3A,0x7B,0x22,
                                    0x63,0x6C,0x69,0x65,0x6E,0x74,0x5F,0x76,
                                    0x65,0x72,0x73,0x69,0x6F,0x6E,0x22,0x3A,
                                    0x32,0x7D,0x7D
                                };

                                await networkStream.WriteAsync(send);

                                var capScript = "let _engines = engines.all(); for (let i = 0; i < _engines.length; i++) { if (_engines[i].getSource().toString().indexOf(\"cap_script\") != -1 && _engines[i] != engines.myEngine()) { _engines[i].forceStop(); } } threads.start(function () { if (!requestScreenCapture()) { alert(\"请求截图权限失败\"); exit(); } else { toastLog(\"请求截图权限成功\"); } }); setInterval(() => { }, 1000);";

                                await networkStream.WriteAsync(GetRunCommandBytes(capScript, "cap_script"));
                                this.Server = new TcpListener(IPAddress.Parse(this.LocalIP), 5678);
                                this.Server.Start();
                                this.Server.BeginAcceptTcpClient(new AsyncCallback(ConnectCallback), this.Server);
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.ShowAsync(ex.ToString());
                    }
                });
            }

           
            return await GetList();
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

        private void ConnectCallback(IAsyncResult ar)
        {
            if (ar.AsyncState != null)
            {
                var listener = (TcpListener)ar.AsyncState;

                if (listener.Server == null || !listener.Server.IsBound)
                {
                    return;
                }

                var client = listener.EndAcceptTcpClient(ar);

                var stream = client.GetStream();

                var offset = 0;
                var header = new byte[4];
                for (var i = 0; i < 100; i++)
                {
                    Thread.Sleep(50);
                    if (stream.DataAvailable)
                    {
                        while (offset < 4)
                        {
                            var len = stream.Read(header, offset, 4 - offset);
                            offset += len;
                        }
                        break;
                    }
                }

                var imgLen = header.ToInt();

                var imgData = new byte[imgLen];

                offset = 0;

                while (offset < imgLen)
                {
                    if (stream.DataAvailable)
                    {
                        var len = stream.Read(imgData, offset, imgLen - offset);
                        offset += len;
                    }
                }

                var sKBitmap = SKBitmap.Decode(imgData);
                var pxFormat = sKBitmap.ColorType == SKColorType.Rgba8888 ? PixelFormat.Rgba8888 : PixelFormat.Bgra8888;
                var bitmap = new Bitmap(pxFormat, AlphaFormat.Opaque, sKBitmap.GetPixels(), new PixelSize(sKBitmap.Width, sKBitmap.Height), new Vector(96, 96), sKBitmap.RowBytes);
                sKBitmap.Dispose();

                stream.Close();
                client.Close();

                this.Action?.Invoke(bitmap);
                this.Server?.BeginAcceptTcpClient(new AsyncCallback(ConnectCallback), this.Server);
            }
        }

        public override void ScreenShot(int Index)
        {
            if (this.Client == null)
            {
                throw new Exception("tcp连接失效");
            }
            this.runCode = this.runCode.Replace("let remoteIP;", $"let remoteIP = '{this.LocalIP}'");
            this.Client.GetStream().Write(GetRunCommandBytes(this.runCode, "screenshotHelper"));
        }

        public override bool IsStart(int Index)
        {
            return true;
        }


        public override void Close()
        {
            try
            {
                this.Server?.Stop();
                this.Client?.Close();
                this.Client?.Dispose();
            }
            catch { };
        }
    }
}
