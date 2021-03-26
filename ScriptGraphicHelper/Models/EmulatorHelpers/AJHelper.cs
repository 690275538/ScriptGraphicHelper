using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScriptGraphicHelper.Models.UnmanagedMethods;
using ScriptGraphicHelper.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models.EmulatorHelpers
{

    class AJHelper : BaseEmulatorHelper
    {
        public override string Path { get; set; } = string.Empty;
        public override string Name { get; set; } = string.Empty;

        private int Step = 0;
        private string RunCode = string.Empty;

        private TcpClient MyTcpClient;

        private NetworkStream networkStream;

        private bool IsInit = false;

        public AJHelper()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Assets/screenshotHelper.js"))
            {
                StreamReader sr = File.OpenText(AppDomain.CurrentDomain.BaseDirectory + @"Assets/screenshotHelper.js");
                RunCode = sr.ReadToEnd();
                Step = 0;
                Path = "AJ连接";
                Name = "AJ连接";
            }
        }

        public override void Dispose()
        {
            if (IsInit)
            {
                try
                {
                    networkStream.Close();
                    MyTcpClient.Close();
                }
                catch { };
            }
        }

        public override bool IsStart(int Index)
        {
            return true;
        }

        public override async Task<List<KeyValuePair<int, string>>> ListAll()
        {
            TcpConfig tcpConfig = new();
            TcpConfig.Port = 9317;
            tcpConfig.Title = "AJ配置";
            await tcpConfig.ShowDialog(new Avalonia.Controls.Window());

            string address = TcpConfig.Address;
            int port = TcpConfig.Port;

            var task = Task.Run(() =>
            {
                List<KeyValuePair<int, string>> result = new();

                if (address != string.Empty && port != -1)
                {
                    try
                    {
                        MyTcpClient = new TcpClient(address, port);
                        networkStream = MyTcpClient.GetStream();
                        byte[] buf = new byte[256];
                        for (int i = 0; i < 40; i++)
                        {
                            Task.Delay(100).Wait();
                            if (networkStream.DataAvailable)
                            {
                                int length = networkStream.Read(buf, 0, 256);
                                string info = Encoding.UTF8.GetString(buf, 8, length - 8);
                                JObject obj = (JObject)JsonConvert.DeserializeObject(info);
                                JObject data = (JObject)obj.GetValue("data");
                                string deviceName = (string)data.GetValue("device_name");

                                byte[] send = new byte[59]
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

                                networkStream.WriteAsync(send);

                                result.Add(new KeyValuePair<int, string>(key: 0, value: deviceName));
                                IsInit = true;
                                return result;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Win32Api.MessageBox(e.Message);
                    }
                }
                result.Add(new KeyValuePair<int, string>(key: 0, value: "null"));
                return result;
            });
            return await task;
        }

        private int Bytes2Int(byte[] src, int offset = 0)
        {
            int value;
            value = ((src[offset] & 0xFF) << 24)
                    | ((src[offset + 1] & 0xFF) << 16)
                    | ((src[offset + 2] & 0xFF) << 8)
                    | (src[offset + 3] & 0xFF);
            return value;
        }

        private byte[] Int2Bytes(int value)
        {
            byte[] src = new byte[4];
            src[0] = (byte)((value >> 24) & 0xFF);
            src[1] = (byte)((value >> 16) & 0xFF);
            src[2] = (byte)((value >> 8) & 0xFF);
            src[3] = (byte)(value & 0xFF);
            return src;
        }

        public class RunData
        {
            public string id { get; set; } = "screenshotHelper";
            public string name { get; set; } = "screenshotHelper";
            public string command { get; set; } = "run";
            public string script { get; set; }
        }

        public class RunCommand
        {
            public int id { get; set; }
            public string type { get; set; } = "command";
            public RunData data { get; set; }
        }

        private byte[] GetRunCommandBytes()
        {

            RunCommand command = new()
            {
                id = Step,
                data = new RunData()
                {
                    script = RunCode
                }

            };
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command));
            byte[] recv = new byte[data.Length + 8];
            byte[] len = Int2Bytes(data.Length);
            len.CopyTo(recv, 0);
            recv[7] = 1;
            data.CopyTo(recv, 8);
            return recv;
        }

        public override async Task<Bitmap> ScreenShot(int Index)
        {
            var task = Task.Run(() =>
            {
                try
                {
                    networkStream.Write(GetRunCommandBytes());

                    var client = new TcpClient();
                    for (int i = 0; i < 300; i++)
                    {
                        Task.Delay(100).Wait();
                        try
                        {
                            client.Connect(TcpConfig.Address, 5678);
                            if (client.Connected)
                            {
                                break;
                            }
                        }
                        catch { }
                    }

                    var stream = client.GetStream();

                    int offset = 0;
                    byte[] info = new byte[4];
                    for (int i = 0; i < 100; i++)
                    {
                        Task.Delay(100).Wait();
                        if (stream.DataAvailable)
                        {
                            while (offset < 4)
                            {
                                int len = stream.Read(info, offset, 4 - offset);
                                offset += 4;
                            }
                            break;
                        }
                    }

                    int length = Bytes2Int(info);

                    byte[] data = new byte[length];

                    offset = 0;

                    while (offset < length)
                    {
                        int len = stream.Read(data, offset, length - offset);
                        offset += len;
                    }

                    SKBitmap sKBitmap = SKBitmap.Decode(data);
                    GraphicHelper.KeepScreen(sKBitmap);
                    var bitmap = new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Opaque, sKBitmap.GetPixels(), new PixelSize(sKBitmap.Width, sKBitmap.Height), new Vector(96, 96), sKBitmap.RowBytes);
                    sKBitmap.Dispose();
                    stream.Dispose();
                    client.Dispose();
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
