using Avalonia.Media.Imaging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static System.Environment;

namespace ScriptGraphicHelper.Models.ScreenshotHelpers
{
    class YsEmulatorHelper : BaseScreenshotHelper
    {
        public override string Path { get; set; } = string.Empty;
        public override string Name { get; set; } = string.Empty;
        public string BmpPath { get; set; }
        public YsEmulatorHelper()//初始化 , 获取模拟器路径
        {
            var path = Setting.Instance.YsPath.Trim("\\".ToCharArray()) + "\\";
            if (path != string.Empty && path != "")
            {
                var index = path.LastIndexOf("\\");
                path = path.Substring(0, index + 1).Trim('"');
                if (File.Exists(path + "NoxConsole.exe"))
                {
                    this.Name = "夜神模拟器";
                    this.Path = path;
                    this.BmpPath = BmpPathGet();
                }
            }
        }
        public override void Dispose() { }

        public string[] List(int index)
        {
            var resultArray = PipeCmd("list").Trim("\n".ToCharArray()).Split("\n".ToCharArray());
            for (var i = 0; i < resultArray.Length; i++)
            {
                var LineArray = resultArray[i].Split(',');
                if (LineArray.Length > 1)
                {
                    if (LineArray[0] == index.ToString())
                    {
                        return LineArray;
                    }
                }
            }
            return new string[] { };
        }
        public override bool IsStart(int index)
        {
            var resultArray = PipeCmd("list").Trim("\n".ToCharArray()).Split("\n".ToCharArray());
            var LineArray = resultArray[index].Split(',');
            if (LineArray.Length > 1)
            {
                if (LineArray.Length == 8) return LineArray[6] != "-1";
                else return LineArray[5] != "-1";
            }
            else
            {
                return false;
            }
        }
        public override async Task<List<KeyValuePair<int, string>>> ListAll()
        {
            var task = Task.Run(() =>
            {
                var resultArray = PipeCmd("list").Trim("\n".ToCharArray()).Split("\n".ToCharArray());
                var result = new List<KeyValuePair<int, string>>();
                for (var i = 0; i < resultArray.Length; i++)
                {
                    var LineArray = resultArray[i].Split(',');
                    if (LineArray.Length == 8)
                    {
                        result.Add(new KeyValuePair<int, string>(key: int.Parse(LineArray[0].Trim()), value: LineArray[2]));
                    }
                    else
                    {
                        result.Add(new KeyValuePair<int, string>(key: i, value: LineArray[1]));
                    }
                }
                return result;
            });
            return await task;
        }
        public override async Task<Bitmap> ScreenShot(int Index)
        {
            var task = Task.Run(() =>
            {
                if (!IsStart(Index))
                {
                    throw new Exception("模拟器未启动 ! ");
                }
                var BmpName = "Screen_" + DateTime.Now.ToString("yy-MM-dd-HH-mm-ss") + ".png";
                Screencap(Index, "/mnt/sdcard/Pictures", BmpName);
                for (var i = 0; i < 10; i++)
                {
                    Task.Delay(200).Wait();
                    if (File.Exists(this.BmpPath + "\\" + BmpName))
                    {
                        break;
                    }
                }
                try
                {
                    FileStream stream = new(this.BmpPath + "\\" + BmpName, FileMode.Open, FileAccess.Read);
                    var bitmap = new Bitmap(stream);
                    stream.Position = 0;
                    var sKBitmap = SKBitmap.Decode(stream);
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
        public void Screencap(int index, string savePath, string saveName)//截图
        {
            PipeCmd("adb -index:" + index.ToString() + " -command:\"shell /system/bin/screencap -p " + savePath.TrimEnd('/') + "/Screenshots/" + saveName + "\"");
        }

        public string BmpPathGet()
        {
            try
            {
                return @"C:\Users\" + UserName + @"\Nox_share\ImageShare\Screenshots\";
            }
            catch
            {
                return "";
            }
        }
        public string PipeCmd(string theCommand)
        {
            var path = this.Path + "NoxConsole.exe";
            var start = new ProcessStartInfo(path)
            {
                Arguments = theCommand,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false
            };
            var pipe = Process.Start(start);
            var readStream = pipe.StandardOutput;
            var OutputStr = readStream.ReadToEnd();
            pipe.WaitForExit(10000);
            pipe.Close();
            readStream.Close();
            return OutputStr;
        }
    }
}
