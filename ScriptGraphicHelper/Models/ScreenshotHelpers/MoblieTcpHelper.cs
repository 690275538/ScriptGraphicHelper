using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ScriptGraphicHelper.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models.ScreenshotHelpers
{
    public class MessageType
    {
        public static byte Stop { get; set; } = 0;
        public static byte Ping { get; set; } = 1;
        public static byte ScreenShot { get; set; } = 2;
    }

    public class TcpClientInfo
    {
        public TcpClient Client { get; set; }
        public string Info { get; set; } = string.Empty;

        public TcpClientInfo(TcpClient client, NetworkStream stream, string info)
        {
            Client = client;
            Info = info;
        }
    }

    class MoblieTcpHelper : BaseScreenshotHelper
    {
        public override string Path { get; set; } = "TCP连接";
        public override string Name { get; set; } = "TCP连接";

        private TcpListener Listener;

        public List<TcpClientInfo> TcpClientInfos { get; set; } = new List<TcpClientInfo>();

        public override void Dispose()
        {
            try
            {
                Listener.Stop();
            }
            catch { };

        }
        public override bool IsStart(int ldIndex)
        {
            return true;
        }


        public static string[] GetLocalAddress()
        {
            try
            {
                var addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                var addresses = addressList.Where(address => address.AddressFamily == AddressFamily.InterNetwork)
                        .Select(address => address.ToString()).ToArray();
                return addresses;
            }
            catch (Exception e)
            {
                MessageBox.ShowAsync($"获取地址失败, 请手动填入\r\n{e}");
                return Array.Empty<string>();
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;

            if (listener.Server == null || !listener.Server.IsBound)
            {
                return;
            }

            TcpClient client = listener.EndAcceptTcpClient(ar);
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buf = new byte[256];
                for (int i = 0; i < 40; i++)
                {
                    Task.Delay(100).Wait();
                    if (stream.DataAvailable)
                    {
                        int length = stream.Read(buf, 0, 256);
                        string info = Encoding.UTF8.GetString(buf, 0, length);
                        TcpClientInfos.Add(new TcpClientInfo(client, stream, info));
                        Listener.BeginAcceptTcpClient(new AsyncCallback(ConnectCallback), Listener);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.ShowAsync(e.Message);
            }
        }

        public override async Task<List<KeyValuePair<int, string>>> ListAll()
        {
            TcpConfig tcpConfig = new();
            TcpConfig.Address = string.Join("|", GetLocalAddress());
            await tcpConfig.ShowDialog(MainWindow.Instance);

            var address = TcpConfig.Address;
            int port = TcpConfig.Port;

            Listener = new TcpListener(IPAddress.Parse(address), port);
            Listener.Start();

            Listener.BeginAcceptTcpClient(new AsyncCallback(ConnectCallback), Listener);

            var task = Task.Run(() =>
            {
                var result = new List<KeyValuePair<int, string>>
                {
                    new KeyValuePair<int, string>(key: 0, value: "null")
                };
                return result;
            });

            return await task;
        }

        public async Task<List<KeyValuePair<int, string>>> GetList()
        {
            var task = Task.Run(() =>
            {
                var result = new List<KeyValuePair<int, string>>();
                for (int i = 0; i < TcpClientInfos.Count; i++)
                {
                    var clientInfo = TcpClientInfos[i];
                    if (GetTcpState(i))
                    {
                        result.Add(new KeyValuePair<int, string>(result.Count, TcpClientInfos[i].Info));
                    }
                    else
                    {
                        TcpClientInfos.Remove(clientInfo);
                        break;
                    }
                }
                if (result.Count == 0)
                {
                    result.Add(new KeyValuePair<int, string>(key: 0, value: "null"));
                }
                return result;
            });
            return await task;
        }


        private bool GetTcpState(int index)
        {
            try
            {
                var stream = TcpClientInfos[index].Client.GetStream();
                for (int j = 0; j < 20; j++)
                {
                    stream.WriteByte(MessageType.Ping);
                    for (int i = 0; i < 10; i++)
                    {
                        Task.Delay(50).Wait();
                        byte[] _ = new byte[9];
                        if (stream.DataAvailable)
                        {
                            int length = stream.Read(_, 0, 1);
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
                }
            }
            catch (Exception e)
            {
                MessageBox.ShowAsync(e.Message);
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

        public override async Task<Bitmap> ScreenShot(int index)
        {
            var task = Task.Run(() =>
            {
                try
                {
                    var stream = TcpClientInfos[index].Client.GetStream();
                    if (!GetTcpState(index))
                    {
                        throw new Exception("Tcp已断开连接! 请重新连接");
                    }
                    stream.WriteByte(MessageType.ScreenShot);

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
                    var bitmap = new Bitmap(GraphicHelper.PxFormat, AlphaFormat.Opaque, sKBitmap.GetPixels(), new PixelSize(sKBitmap.Width, sKBitmap.Height), new Vector(96, 96), sKBitmap.RowBytes);
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
