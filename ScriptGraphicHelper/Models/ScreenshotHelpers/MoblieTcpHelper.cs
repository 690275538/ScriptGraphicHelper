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
            this.Client = client;
            this.Info = info;
        }
    }

    class MoblieTcpHelper : BaseHelper
    {
        public override Action<Bitmap>? Action { get; set; }
        public override string Path { get; } = "TCP连接";
        public override string Name { get; } = "TCP连接";

        private TcpListener? Listener;

        public List<TcpClientInfo> TcpClientInfos { get; set; } = new List<TcpClientInfo>();

        public override void Close()
        {
            try
            {
                this.Listener?.Stop();
            }
            catch { };

        }
        public override bool IsStart(int ldIndex)
        {
            return true;
        }


        public static string GetLocalAddress()
        {
            var addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            foreach (var address in addressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    var ip = address.ToString();
                    if (ip.StartsWith("192.168"))
                    {
                        Console.WriteLine(ip);
                    }
                }
            }

            return addressList.Where(addresss => addresss.AddressFamily == AddressFamily.InterNetwork)
                .Select(addresss => addresss.ToString()).FirstOrDefault()
                ?? addressList[0].ToString();
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            var listener = (TcpListener)ar.AsyncState;

            if (listener.Server == null || !listener.Server.IsBound)
            {
                return;
            }

            var client = listener.EndAcceptTcpClient(ar);
            try
            {
                var stream = client.GetStream();
                var buf = new byte[256];
                for (var i = 0; i < 40; i++)
                {
                    Task.Delay(100).Wait();
                    if (stream.DataAvailable)
                    {
                        var length = stream.Read(buf, 0, 256);
                        var info = Encoding.UTF8.GetString(buf, 0, length);
                        this.TcpClientInfos.Add(new TcpClientInfo(client, stream, info));
                        this.Listener.BeginAcceptTcpClient(new AsyncCallback(ConnectCallback), this.Listener);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.ShowAsync(e.ToString());
            }
        }

        public override async Task<List<KeyValuePair<int, string>>> Initialize()
        {
            var tcpConfig = new TcpConfig(Util.GetLocalAddress());

            var result = await tcpConfig.ShowDialog<(string, int)?>(MainWindow.Instance);

            if (result != null)
            {
                var address = result.Value.Item1;
                var port = result.Value.Item2;

                this.Listener = new TcpListener(IPAddress.Parse(address), port);
                this.Listener.Start();

                this.Listener.BeginAcceptTcpClient(new AsyncCallback(ConnectCallback), this.Listener);
            }

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

        public override async Task<List<KeyValuePair<int, string>>> GetList()
        {
            var task = Task.Run(() =>
            {
                var result = new List<KeyValuePair<int, string>>();
                for (var i = 0; i < this.TcpClientInfos.Count; i++)
                {
                    var clientInfo = this.TcpClientInfos[i];
                    if (GetTcpState(i))
                    {
                        result.Add(new KeyValuePair<int, string>(result.Count, this.TcpClientInfos[i].Info));
                    }
                    else
                    {
                        this.TcpClientInfos.Remove(clientInfo);
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
                var stream = this.TcpClientInfos[index].Client.GetStream();
                for (var j = 0; j < 20; j++)
                {
                    stream.WriteByte(MessageType.Ping);
                    for (var i = 0; i < 10; i++)
                    {
                        Task.Delay(50).Wait();
                        var _ = new byte[9];
                        if (stream.DataAvailable)
                        {
                            var length = stream.Read(_, 0, 1);
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
                MessageBox.ShowAsync(e.ToString());
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
            var src = new byte[4];
            src[0] = (byte)((value >> 24) & 0xFF);
            src[1] = (byte)((value >> 16) & 0xFF);
            src[2] = (byte)((value >> 8) & 0xFF);
            src[3] = (byte)(value & 0xFF);
            return src;
        }

        public override async void ScreenShot(int index)
        {
            await Task.Run(() =>
            {
                var stream = this.TcpClientInfos[index].Client.GetStream();
                if (!GetTcpState(index))
                {
                    throw new Exception("Tcp已断开连接! 请重新连接");
                }
                stream.WriteByte(MessageType.ScreenShot);

                var offset = 0;
                var info = new byte[4];
                for (var i = 0; i < 100; i++)
                {
                    Task.Delay(100).Wait();
                    if (stream.DataAvailable)
                    {
                        while (offset < 4)
                        {
                            var len = stream.Read(info, offset, 4 - offset);
                            offset += 4;
                        }
                        break;
                    }
                }

                var length = Bytes2Int(info);

                var data = new byte[length];

                offset = 0;

                while (offset < length)
                {
                    var len = stream.Read(data, offset, length - offset);
                    offset += len;
                }

                var sKBitmap = SKBitmap.Decode(data);
                GraphicHelper.KeepScreen(sKBitmap);
                var bitmap = new Bitmap(GraphicHelper.PxFormat, AlphaFormat.Opaque, sKBitmap.GetPixels(), new PixelSize(sKBitmap.Width, sKBitmap.Height), new Vector(96, 96), sKBitmap.RowBytes);
                sKBitmap.Dispose();
                this.Action?.Invoke(bitmap);
            }).ContinueWith((t) =>
            {
                if (t.Exception != null)
                    MessageBox.ShowAsync(t.Exception.ToString());
            }); ;
        }
    }
}
