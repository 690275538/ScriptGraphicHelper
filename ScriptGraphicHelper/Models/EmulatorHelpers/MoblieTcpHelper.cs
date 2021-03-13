using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ScriptGraphicHelper.Models.UnmanagedMethods;
using ScriptGraphicHelper.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models.EmulatorHelpers
{
    public class MessageType
    {
        public static byte Stop { get; set; } = 0;
        public static byte Ping { get; set; } = 1;
        public static byte ScreenShot { get; set; } = 2;
    }
    class MoblieTcpHelper : BaseEmulatorHelper
    {
        public override string Path { get; set; } = "TCP连接";
        public override string Name { get; set; } = "TCP连接";

        private TcpClient MyTcpClient;

        private NetworkStream networkStream;

        private int Width = -1;
        private int Height = -1;
        private bool IsInit = false;
        public override void Dispose()
        {
            if (IsInit)
            {
                try
                {
                    IsInit = false;
                    networkStream.WriteByte(MessageType.Stop);
                    networkStream.Close();
                    MyTcpClient.Close();
                }
                catch { };
            }

        }
        public override bool IsStart(int ldIndex)
        {
            return true;
        }
        public override async Task<List<KeyValuePair<int, string>>> ListAll()
        {

            TcpConfig tcpConfig = new();
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
                                string[] info = Encoding.UTF8.GetString(buf, 0, length).Split('|');
                                Width = int.Parse(info[1]);
                                Height = int.Parse(info[2]);
                                result.Add(new KeyValuePair<int, string>(key: 0, value: info[0]));
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

        private bool GetTcpState()
        {
            networkStream.WriteByte(MessageType.Ping);
            for (int i = 0; i < 40; i++)
            {
                Task.Delay(50).Wait();
                byte[] _ = new byte[9];
                if (networkStream.DataAvailable)
                {
                    int length = networkStream.Read(_, 0, 1);
                    if (length == 1)
                    {
                        if (_[0] == MessageType.Ping)
                        {
                            return true;
                        }
                        else if (_[0] == MessageType.Stop)
                        {
                            return false;
                        }
                    }
                }
            }
            return false;
        }
        public override async Task<Bitmap> ScreenShot(int Index)
        {
            var task = Task.Run(() =>
            {
                try
                {
                    if (!GetTcpState())
                    {
                        throw new Exception("Tcp已断开连接! 请重新连接");
                    }
                    networkStream.WriteByte(MessageType.ScreenShot);
                    byte[] result = new byte[Width * Height * 4];
                    int offset = 0;
                    while (offset < result.Length)
                    {
                        int len = result.Length - offset;
                        int length = networkStream.Read(result, offset, len);
                        offset += length;

                    }

                    byte[] data = new byte[Width * 4 * Height];
                    int step = 0;
                    for (int j = 0; j < Height; j++)
                    {
                        int location = Width * 4 * j;
                        for (int i = 0; i < Width; i++)
                        {
                            data[step] = result[location + 2];
                            data[step + 1] = result[location + 1];
                            data[step + 2] = result[location];
                            data[step + 3] = 255;
                            step += 4;
                            location += 4;
                        }
                    }
                    SKBitmap sKBitmap = new(new SKImageInfo(Width, Height));
                    Marshal.Copy(data, 0, sKBitmap.GetPixels(), data.Length);
                    GraphicHelper.KeepScreen(sKBitmap);
                    var bitmap = new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Opaque, sKBitmap.GetPixels(), new PixelSize(Width, Height), new Vector(96, 96), sKBitmap.RowBytes);
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
