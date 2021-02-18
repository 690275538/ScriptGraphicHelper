﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using static System.Environment;

namespace ScriptGraphicHelper.Models
{
    class XyEmulatorHelper : EmulatorHelper
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
                    MessageBox.Show("模拟器未启动 ! ");
                    return new Bitmap(1, 1);
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
                    FileStream fileStream = new FileStream(BmpPath + "\\" + BmpName, FileMode.Open, FileAccess.Read);
                    Bitmap bmp = (Bitmap)Image.FromStream(fileStream);
                    fileStream.Close();
                    fileStream.Dispose();
                    return bmp;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return new Bitmap(1, 1);
                }
            });
            return await task;
        }
        public XyEmulatorHelper()//初始化 , 获取模拟器路径
        {
            Name = "逍遥模拟器";
            string path = GetInkTargetPath(@"C:\Users\" + UserName + @"\AppData\Roaming\Microsoft\Windows\Start Menu", "逍遥模拟器.lnk");
            if (path == string.Empty)
            {
                path = GetInkTargetPath(@"C:\ProgramData\Microsoft\Windows\Start Menu", "逍遥模拟器.lnk");
                if (path == string.Empty)
                {
                    path = GetInkTargetPath(AppDomain.CurrentDomain.BaseDirectory, "逍遥模拟器.lnk");
                }
            }
            if (path != string.Empty)
            {
                int index = path.LastIndexOf("\\");
                Path = path.Substring(0, index + 1).Trim('"');
            }
        }
        public string GetInkTargetPath(string path, string fileName)
        {
            string result = string.Empty;
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();
            foreach (var item in fileSystemInfos)
            {
                try
                {
                    string fullName = item.FullName;
                    if (Directory.Exists(fullName))
                    {
                        result = GetInkTargetPath(fullName, fileName);
                        if (result != string.Empty)
                        {
                            return result;
                        }
                    }
                    else
                    {
                        if (item.Name == fileName)
                        {
                            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShellClass();
                            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(fullName);
                            return shortcut.TargetPath;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            return result;
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
            return new string[] { };
        }
        public void Screencap(int index, string savePath, string saveName)//截图
        {
            PipeCmd("-i " + index.ToString() + " adb \"shell /system/bin/screencap -p " + savePath.TrimEnd('/') + "/" + saveName + "\"");
        }
        public string BmpPathGet(int index)
        {
            try
            {
                string result = PipeCmd(string.Format("getconfigex -i {0} picturepath",index));
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
