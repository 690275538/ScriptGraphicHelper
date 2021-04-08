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

        private NetworkStream MyNetworkStream;

        private bool IsInit = false;
        public override void Dispose()
        {
            if (IsInit)
            {
                try
                {
                    IsInit = false;
                    MyNetworkStream.WriteByte(MessageType.Stop);
                    MyNetworkStream.Close();
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
                        MyNetworkStream = MyTcpClient.GetStream();
                        byte[] buf = new byte[256];
                        for (int i = 0; i < 40; i++)
                        {
                            Task.Delay(100).Wait();
                            if (MyNetworkStream.DataAvailable)
                            {
                                int length = MyNetworkStream.Read(buf, 0, 256);
                                string info = Encoding.UTF8.GetString(buf, 0, length);
                                result.Add(new KeyValuePair<int, string>(key: 0, value: info));
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
            MyNetworkStream.WriteByte(MessageType.Ping);
            for (int i = 0; i < 40; i++)
            {
                Task.Delay(50).Wait();
                byte[] _ = new byte[9];
                if (MyNetworkStream.DataAvailable)
                {
                    int length = MyNetworkStream.Read(_, 0, 1);
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
                    MyNetworkStream.WriteByte(MessageType.ScreenShot);

                    int offset = 0;
                    byte[] info = new byte[4];
                    for (int i = 0; i < 100; i++)
                    {
                        Task.Delay(100).Wait();
                        if (MyNetworkStream.DataAvailable)
                        {
                            while (offset < 4)
                            {
                                int len = MyNetworkStream.Read(info, offset, 4 - offset);
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
                        int len = MyNetworkStream.Read(data, offset, length - offset);
                        offset += len;
                    }

                    SKBitmap sKBitmap = SKBitmap.Decode(data);
                    GraphicHelper.KeepScreen(sKBitmap);
                    var bitmap = new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Opaque, sKBitmap.GetPixels(), new PixelSize(sKBitmap.Width, sKBitmap.Height), new Vector(96, 96), sKBitmap.RowBytes);
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
