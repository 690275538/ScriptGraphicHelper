using Avalonia.Media.Imaging;
using ScriptGraphicHelper.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models.EmulatorHelpers
{
    class AdbHelper : BaseEmulatorHelper
    {
        public override string Path { get; set; } = AppDomain.CurrentDomain.BaseDirectory + @"Assets\Adb\";
        public override string Name { get; set; } = "Adb连接";
        public string BmpPath { get; set; } = string.Empty;
        private List<KeyValuePair<int, string>> DeviceInfos = new List<KeyValuePair<int, string>>();

        public AdbHelper()
        {
            if (!Directory.Exists(Path + "\\Screenshot"))
            {
                Directory.CreateDirectory(Path + "\\Screenshot");
            }
        }

        public override bool IsStart(int index)
        {
            return true;
        }
        public override void Dispose() { }
        public override async Task<List<KeyValuePair<int, string>>> ListAll()
        {
            await Task.Run(() =>
            {
                PipeCmd("kill-server");
                PipeCmd("start-server");
            });

            DeviceInfos.Clear();
            var config = new TcpConfig();
            config.Title = "Adb无线调试";
            await config.ShowDialog(MainWindow.Instance);
            if (config.IsTapped && TcpConfig.Port != 9317 && TcpConfig.Port != 5678)
            {
                PipeCmd("connect " + TcpConfig.Address + ":" + TcpConfig.Port.ToString());
            }

            var task = Task.Run(() =>
            {
                string output = PipeCmd("devices");
                string[] array = output.Substring(output.IndexOf("List of devices attached") + 16).Split("\r\n");
                for (int i = 0; i < array.Length; i++)
                {
                    string[] deviceInfo = array[i].Split("dev");
                    if (deviceInfo.Length == 2)
                    {
                        DeviceInfos.Add(new KeyValuePair<int, string>(DeviceInfos.Count, deviceInfo[0].Trim()));
                    }
                }
                if (DeviceInfos.Count == 0)
                {
                    DeviceInfos.Add(new KeyValuePair<int, string>(0, "null"));
                }

                return DeviceInfos;
            });
            return await task;
        }

        public override async Task<Bitmap> ScreenShot(int index)
        {
            var task = Task.Run(() =>
            {
                string BmpName = "Screen_" + DateTime.Now.ToString("yy-MM-dd-HH-mm-ss") + ".png";
                string BmpPath = Path + "\\Screenshot\\" + BmpName;
                PipeCmd("-s " + DeviceInfos[index].Value + " shell /system/bin/screencap -p /sdcard/screenshot.png");
                for (int i = 0; i < 20; i++)
                {
                    Task.Delay(100).Wait();
                    PipeCmd("-s " + DeviceInfos[index].Value + " pull /sdcard/screenshot.png ./Assets/Adb/Screenshot/" + BmpName);
                    Task.Delay(100).Wait();
                    if (File.Exists(BmpPath))
                    {
                        break;
                    }
                }
                try
                {
                    FileStream stream = new(BmpPath, FileMode.Open, FileAccess.Read);
                    var bitmap = new Bitmap(stream);
                    stream.Position = 0;
                    SKBitmap sKBitmap = SKBitmap.Decode(stream);
                    GraphicHelper.KeepScreen(sKBitmap);
                    sKBitmap.Dispose();
                    stream.Dispose();
                    return bitmap;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            });
            return await task;
        }


        public string[] List(int index)
        {
            return Array.Empty<string>();
        }

        public string PipeCmd(string theCommand)
        {
            string path = Path + "adb.exe";
            ProcessStartInfo start = new ProcessStartInfo(path)
            {
                Arguments = theCommand,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
            };
            Process pipe = Process.Start(start);
            StreamReader readStream = pipe.StandardOutput;
            string OutputStr = readStream.ReadToEnd();
            pipe.WaitForExit(10000);
            pipe.Close();
            readStream.Close();
            return OutputStr;
        }
    }
}
