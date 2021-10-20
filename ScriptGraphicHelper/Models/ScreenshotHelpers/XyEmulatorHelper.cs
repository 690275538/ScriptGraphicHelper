using Avalonia.Media.Imaging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models.ScreenshotHelpers
{
    class XyEmulatorHelper : BaseHelper
    {
        public override Action<Bitmap>? Action { get; set; }
        public override string Path { get; } = string.Empty;
        public override string Name { get; } = string.Empty;
        public string BmpPath { get; set; } = string.Empty;
        public override bool IsStart(int index)
        {
            var result = PipeCmd("isvmrunning -i " + index.ToString());
            result = result.Trim();
            return result == "Running";
        }
        public override void Close() { }
        public override async Task<List<KeyValuePair<int, string>>> Initialize()
        {
            return await GetList();
        }

        public override async Task<List<KeyValuePair<int, string>>> GetList()
        {
            return await Task.Run(() =>
            {
                var resultArray = PipeCmd("listvms").Trim("\r\n".ToCharArray()).Split("\r\n");
                var result = new List<KeyValuePair<int, string>>();
                for (var i = 0; i < resultArray.Length; i++)
                {
                    var LineArray = resultArray[i].Split(',');
                    if (LineArray.Length >= 4)
                    {
                        result.Add(new KeyValuePair<int, string>(key: int.Parse(LineArray[0].Trim()), value: LineArray[1]));
                    }
                }
                return result;
            });
        }

        public override async void ScreenShot(int index)
        {
            await Task.Run(() =>
             {
                 if (!IsStart(index))
                 {
                     throw new Exception("模拟器未启动 ! ");
                 }
                 if (this.BmpPath == string.Empty)
                 {
                     this.BmpPath = BmpPathGet(index);
                 }
                 var BmpName = "Screen_" + DateTime.Now.ToString("yy-MM-dd-HH-mm-ss") + ".png";
                 Screencap(index, "/mnt/sdcard/Pictures", BmpName);
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
                     this.Action?.Invoke(bitmap);
                 }
                 catch (Exception e)
                 {
                     throw new Exception(e.Message);
                 }
             });
        }

        public XyEmulatorHelper()//初始化, 获取模拟器路径
        {
            var path = Setting.Instance.XyPath.Trim("\\".ToCharArray()) + "\\";
            if (path != string.Empty && path != "")
            {
                var index = path.LastIndexOf("\\");
                path = path.Substring(0, index + 1).Trim('"');
                if (File.Exists(path + "memuc.exe"))
                {
                    this.Name = "逍遥模拟器";
                    this.Path = path;
                }
            }
        }

        public string[] List(int index)
        {
            var resultArray = PipeCmd("listvms").Trim("\r\n".ToCharArray()).Split("\r\n");
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
            return Array.Empty<string>();
        }

        public void Screencap(int index, string savePath, string saveName)//截图
        {
            PipeCmd("-i " + index.ToString() + " adb \"shell /system/bin/screencap -p " + savePath.TrimEnd('/') + "/" + saveName + "\"");
        }
        public string BmpPathGet(int index)
        {
            try
            {
                var result = PipeCmd(string.Format("getconfigex -i {0} picturepath", index));
                var firstIndex = result.IndexOf(":") + 2;
                var lastIndex = result.LastIndexOf("\\");
                return result.Substring(firstIndex, lastIndex - firstIndex + 1).Trim() + "逍遥安卓照片\\";
            }
            catch
            {
                return string.Empty;
            }
        }
        public string PipeCmd(string theCommand)
        {
            var path = this.Path + "memuc.exe";
            var start = new ProcessStartInfo(path)
            {
                Arguments = theCommand,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
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
