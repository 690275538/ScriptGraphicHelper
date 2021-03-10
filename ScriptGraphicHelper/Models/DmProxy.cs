using Avalonia;
using DmService;
using Grpc.Net.Client;
using ScriptGraphicHelper.Models.UnmanagedMethods;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static DmService.Methods;

namespace ScriptGraphicHelper.Models
{
    public static class DmProxy
    {
        public static MethodsClient DmClient { get; set; }
        public static bool Inited { get; set; }
        public static string RegCode { get; set; } = string.Empty;

        public static int Hwnd { get; set; } = -1;
        public static string Display { get; set; } = string.Empty;
        public static string Mouse { get; set; } = "normal";
        public static string Keypad { get; set; } = "normal";
        public static string Public_desc { get; set; } = string.Empty;
        public static int Mode { get; set; } = 0;


        private static string Path = AppDomain.CurrentDomain.BaseDirectory + "DmServer.exe";
        public static bool ServerExists()
        {

            Process[] processes = Process.GetProcessesByName("DmServer");
            if (processes.Length > 0)
            {
                if (!Inited)
                {
                    Inited = Init();
                }
                return Inited && Reg();
            }
            else
            {
                if (ServerStart())
                {

                    Inited = Init();
                    return Inited;
                }
                else
                {
                    return false;
                }
            }
        }
        public static bool ServerStart()
        {
            if (File.Exists(Path))
            {
                ProcessStartInfo start = new ProcessStartInfo(Path)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Verb = "runas"
                };
                Process.Start(start);
            }
            else
            {
                Win32Api.MessageBox("大漠GRPC服务程序不存在!");
                return false;
            }

            for (int i = 0; i < 10; i++)
            {
                Task.Delay(1000).Wait();
                Process[] processes = Process.GetProcessesByName("DmServer");
                if (processes.Length > 0)
                {
                    return true;
                }
            }
            return false;

        }

        public static bool Init()
        {
            try
            {
                var channel = GrpcChannel.ForAddress("http://localhost:1024", new GrpcChannelOptions
                {
                    MaxReceiveMessageSize = 20 * 1024 * 1024,
                    MaxSendMessageSize = 1 * 1024 * 1024
                });
                DmClient = new MethodsClient(channel);
                return true;
            }
            catch (Exception e)
            {
                Win32Api.MessageBox(e.Message);
                return false;
            }

        }

        public static bool Reg()
        {
            RegCode = PubSetting.Setting.DmRegcode;
            if (RegCode == string.Empty || RegCode == "")
            {
                Win32Api.MessageBox("错误, 需要在setting.json文件中填写大漠注册码");
                return false;
            }
            
            int result = DmClient.Reg(new RegArgs { Name = RegCode }).Message;
            if (result==1)
            {
                return true;
            }
            else
            {
                Win32Api.MessageBox("注册大漠失败, 返回值" + result.ToString());
                return false;
            }
           
        }

        public static byte[] GetScreenData(int x1, int y1, int x2, int y2)
        {
            if (!ServerExists())
            {
                throw new Exception("大漠GRPC服务异常!");
            }

            return DmClient.GetScreenData(new GetScreenDataArgs { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, Hwnd = Hwnd, Display = Display, Mouse = Mouse, Keypad = Keypad, PubDesc = Public_desc, Mode = Mode }).Data.ToByteArray();
        }

        public static int BindWindowEx()
        {
            if (!ServerExists())
            {
                throw new Exception("大漠GRPC服务异常!");
            }
            return DmClient.BindWindowEx(new BindWindowExArgs { Hwnd = Hwnd, Display = Display, Mouse = Mouse, Keypad = Keypad, PubDesc = Public_desc, Mode = Mode }).Message;
        }

        public static int UnBindWindow()
        {
            if (!ServerExists())
            {
                throw new Exception("大漠GRPC服务异常!");
            }
            return DmClient.UnBindWindow(new NullArgs()).Message;
        }

        public static Point GetClientSize(int hwnd)
        {
            if (!ServerExists())
            {
                throw new Exception("大漠GRPC服务异常!");
            }
            string[] result = DmClient.GetClientSize(new GetClientSizeArgs { Hwnd = hwnd }).Message.Split(',');
            return new Point(int.Parse(result[0]), int.Parse(result[1]));
        }



        public static string EnumWindow(int parent, string title, string class_name, int filter)
        {
            if (!ServerExists())
            {
                throw new Exception("大漠GRPC服务异常!");
            }
            return DmClient.EnumWindow(new EnumWindowArgs { Parent = parent, Title = title, ClsName = class_name, Filter = filter }).Message;
        }
        public static string GetWindowTitle(int hwnd)
        {
            if (!ServerExists())
            {
                throw new Exception("大漠GRPC服务异常!");
            }
            return DmClient.GetWindowTitle(new GetWindowTitleArgs { Hwnd = hwnd }).Message;
        }

        public static int GetMousePointWindow()
        {
            if (!ServerExists())
            {
                throw new Exception("大漠GRPC服务异常!");
            }
            return DmClient.GetMousePointWindow(new NullArgs()).Message;
        }

        public static int GetWindow(int hwnd, int flag)
        {
            if (!ServerExists())
            {
                throw new Exception("大漠GRPC服务异常!");
            }
            return DmClient.GetWindow(new GetWindowArgs { Hwnd = hwnd, Flag = flag }).Message;
        }

        public static string GetWindowClass(int hwnd)
        {
            if (!ServerExists())
            {
                throw new Exception("大漠GRPC服务异常!");
            }
            return DmClient.GetWindowClass(new GetWindowClassArgs { Hwnd = hwnd }).Message;
        }



    }
}
