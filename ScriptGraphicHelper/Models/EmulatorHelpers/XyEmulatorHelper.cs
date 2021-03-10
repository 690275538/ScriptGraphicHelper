using Avalonia.Media.Imaging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models.EmulatorHelpers
{
    class XyEmulatorHelper : BaseEmulatorHelper
    {
        public override string Path { get; set; } = string.Empty;
        public override string Name { get; set; } = string.Empty;
        public string BmpPath { get; set; } = string.Empty;
        public override bool IsStart(int index)
        {
            string result = PipeCmd("isvmrunning -i " + index.ToString());
            result = result.Trim();
            return result == "Running";
        }
        public override void Dispose() { }
        public override async Task<List<KeyValuePair<int, string>>> ListAll()
        {
            var task = Task.Run(() =>
            {
                string[] resultArray = PipeCmd("listvms").Trim("\r\n".ToCharArray()).Split("\r\n".ToCharArray());
                List<KeyValuePair<int, string>> result = new List<KeyValuePair<int, string>>();
                for (int i = 0; i < resultArray.Length; i++)
                {
                    string[] LineArray = resultArray[i].Split(',');
                    result.Add(new KeyValuePair<int, string>(key: int.Parse(LineArray[0].Trim()), value: LineArray[1]));
                }
                return result;
            });
            return await task;
        }
        public override async Task<Bitmap> ScreenShot(int index)
        {
            var task = Task.Run(() =>
            {
                if (!IsStart(index))
                {
                    throw new Exception("模拟器未启动 ! ");
                }
                if (BmpPath == string.Empty)
                {
                    BmpPath = BmpPathGet(index);
                }
                string BmpName = "Screen_" + DateTime.Now.ToString("yy-MM-dd-HH-mm-ss") + ".png";
                Screencap(index, "/mnt/sdcard/Pictures", BmpName);
                for (int i = 0; i < 10; i++)
                {
                    Task.Delay(200).Wait();
                    if (File.Exists(BmpPath + "\\" + BmpName))
                    {
                        break;
                    }
                }
                try
                {
                    FileStream stream = new(BmpPath + "\\" + BmpName, FileMode.Open, FileAccess.Read);
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

        public XyEmulatorHelper()//初始化 , 获取模拟器路径
        {
            Name = "逍遥模拟器";

            string path = PubSetting.Setting.XyPath.Trim("\\".ToCharArray()) + "\\";
            if (path != string.Empty && Path != "")
            {
                int index = path.LastIndexOf("\\");
                Path = path.Substring(0, index + 1).Trim('"');
            }
        }

        public string[] List(int index)
        {
            string[] resultArray = PipeCmd("listvms").Trim("\n".ToCharArray()).Split("\n".ToCharArray());
            for (int i = 0; i < resultArray.Length; i++)
            {
                string[] LineArray = resultArray[i].Split(',');
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
                string result = PipeCmd(string.Format("getconfigex -i {0} picturepath", index));
                int firstIndex = result.IndexOf(":") + 2;
                int lastIndex = result.LastIndexOf("\\");
                return result.Substring(firstIndex, lastIndex - firstIndex + 1).Trim() + "逍遥安卓照片\\";
            }
            catch
            {
                return string.Empty;
            }
        }
        public string PipeCmd(string theCommand)
        {
            string path = Path + "memuc";
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
