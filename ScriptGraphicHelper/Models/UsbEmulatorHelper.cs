using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScriptGraphicHelper.Models
{
    /// <summary>
    /// USB连接
    /// </summary>
    public class UsbEmulatorHelper : EmulatorHelper
    {
        public override string Path { get; set; } = string.Empty;
        public override string Name { get; set; } = string.Empty;
        public string BmpPath { get; set; }
        private List<KeyValuePair<int, string>> playerList = new List<KeyValuePair<int, string>>();
        public UsbEmulatorHelper()//初始化 , 获取模拟器路径
        {
            Name = Path = "USB连接";
            BmpPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
        }
        public override void Dispose() { }

        public override bool IsStart(int index)
        {
            return true;
        }
        public override async Task<List<KeyValuePair<int, string>>> ListAll()
        {
            List<string> player_list = new List<string>();
            var str = PipeCmd("devices");
            string[] resultArray = str.Trim("\r\n".ToCharArray()).Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Where(d => d.Contains("\t")).ToArray();
            for (int i = 0; i < resultArray.Length; i++)
            {
                player_list.Add(resultArray[i].Split("\t".ToCharArray())[0].Trim());
            }
            this.playerList = player_list.Select((val, index) =>
            {
                return new KeyValuePair<int, string>(index, val);
            }).ToList();
            return playerList;
        }
        public override async Task<Bitmap> ScreenShot(int Index)
        {
            var task = Task.Run(() =>
            {
                if (!IsStart(Index))
                {
                    MessageBox.Show("设备未连接 ! ");
                    return new Bitmap(1, 1);
                }
                string BmpName = "Screen_temp.jpeg";
                string img_path = $"/sdcard/Pictures/{BmpName}";
                Screencap(Index, img_path);
                try
                {
                    SaveImage(Index, img_path, BmpPath);
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
        public void Screencap(int index, string savePath)//截图
        {
            var player = playerList[index].Value;
            var r = PipeCmd($" -s {player} shell screencap -p '{savePath}' ");
            Console.WriteLine(r);
        }
        private void SaveImage(int index, string img_path, string local_path)//截图
        {
            var player = playerList[index].Value;
            var r = PipeCmd($" -s {player}  pull {img_path} {local_path} ");
            Console.WriteLine(r);
        }

        public string PipeCmd(string theCommand)
        {
            string path = "adb.exe";
            ProcessStartInfo start = new ProcessStartInfo(path)
            {
                Arguments = theCommand,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                StandardOutputEncoding = Encoding.UTF8,
            };
            Process pipe = Process.Start(start);
            StreamReader readStream = pipe.StandardOutput;
            string OutputStr = readStream.ReadToEnd();
            pipe.WaitForExit();
            //pipe.WaitForExit(10000);
            pipe.Close();
            readStream.Close();
            return OutputStr;
        }

    }
}
