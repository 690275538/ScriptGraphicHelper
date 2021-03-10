using DmService;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DmServer
{
    class Program
    {
        const int MAX_LENGTH = 1024 * 1024 * 20;
        static void Main(string[] args)
        {
            const int Port = 1024;

            var channelOptions = new List<ChannelOption>();
            channelOptions.Add(new ChannelOption(ChannelOptions.MaxReceiveMessageLength, 1024 * 1024));
            channelOptions.Add(new ChannelOption(ChannelOptions.MaxSendMessageLength, MAX_LENGTH));

            Server server = new(channelOptions)
            {
                Services = { Methods.BindService(new GRPCImpl()) },

                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }

            };

            server.Start();

            Console.WriteLine("大漠GRPC服务正在运行中...");

            Console.WriteLine("按任意键退出...");

            Console.ReadKey();
            server.ShutdownAsync().Wait();
        }
    }

    class GRPCImpl : Methods.MethodsBase
    {
        private Dmsoft Dm = new Dmsoft();
        private bool IsReg = false;

        public override Task<Int32Reply> Reg(RegArgs args, ServerCallContext context)
        {
            int result = -1;
            if (!IsReg)
            {
                result = Dm.Reg(args.Name, "");
                if (result == 1)
                {
                    IsReg = true;
                }
            }
            else
            {
                result = 1;
            }

            return Task.FromResult(new Int32Reply { Message = result });
        }

        public override Task<Int32Reply> UnBindWindow(NullArgs args, ServerCallContext context)
        {
            return Task.FromResult(new Int32Reply { Message = Dm.UnBindWindow() });
        }

        public override Task<StrReply> GetClientSize(GetClientSizeArgs args, ServerCallContext context)
        {
            Dm.GetClientSize(args.Hwnd, out int width, out int height);
            return Task.FromResult(new StrReply { Message = string.Format("{0},{1}", width, height) });
        }

        public override Task<BytesReply> GetScreenData(GetScreenDataArgs args, ServerCallContext context)
        {
            if (Dm.GetBindWindow() != args.Hwnd)
            {
                Dm.BindWindowEx(args.Hwnd, args.Display, args.Mouse, args.Keypad, args.PubDesc, args.Mode);
            }

            IntPtr intPtr = (IntPtr)Dm.GetScreenData(args.X1, args.Y1, args.X2, args.Y2);
            byte[] data = new byte[args.X2 * args.Y2 * 4];
            Marshal.Copy(intPtr, data, 0, args.X2 * args.Y2 * 4);
            return Task.FromResult(new BytesReply { Data = ByteString.CopyFrom(data) });
        }

        public override Task<StrReply> EnumWindow(EnumWindowArgs args, ServerCallContext context)
        {
            string result = Dm.EnumWindow(args.Parent, args.Title, args.ClsName, args.Filter);
            return Task.FromResult(new StrReply { Message = result });
        }
        public override Task<StrReply> GetWindowTitle(GetWindowTitleArgs args, ServerCallContext context)
        {
            string result = Dm.GetWindowTitle(args.Hwnd);
            return Task.FromResult(new StrReply { Message = result });
        }

        public override Task<Int32Reply> GetMousePointWindow(NullArgs args, ServerCallContext context)
        {
            int result = Dm.GetMousePointWindow();
            return Task.FromResult(new Int32Reply { Message = result });
        }

        public override Task<Int32Reply> GetWindow(GetWindowArgs args, ServerCallContext context)
        {
            int result = Dm.GetWindow(args.Hwnd, args.Flag);
            return Task.FromResult(new Int32Reply { Message = result });
        }

        public override Task<StrReply> GetWindowClass(GetWindowClassArgs args, ServerCallContext context)
        {
            string result = Dm.GetWindowClass(args.Hwnd);
            return Task.FromResult(new StrReply { Message = result });
        }

        public override Task<Int32Reply> BindWindowEx(BindWindowExArgs args, ServerCallContext context)
        {
            int result = Dm.BindWindowEx(args.Hwnd, args.Display, args.Mouse, args.Keypad, args.PubDesc, args.Mode);
            return Task.FromResult(new Int32Reply { Message = result });
        }
    }

}
