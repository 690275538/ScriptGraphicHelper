using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Environment;

namespace ScriptGraphicHelper.Models
{
    /// <summary>
    /// https://mumu.163.com/help/20230428/35047_1085731.html
    /// </summary>
    public class MuMuEmulatorHelper : EmulatorHelper
    {
        public override string Path { get; set; } = string.Empty;
        public override string Name { get; set; } = string.Empty;
        public string BmpPath { get; set; }
        public MuMuEmulatorHelper()//初始化 , 获取模拟器路径
        {
            Name = "MuMu模拟器";
            string path = GetInkTargetPath(@"C:\Users\" + UserName + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\MuMu模拟器12", "MuMu模拟器12.lnk");
            if (path == string.Empty)
            {
                path = GetInkTargetPath(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\MuMu模拟器12", "MuMu模拟器12.lnk");
                if (path == string.Empty)
                {
                    path = GetInkTargetPath(CurrentDirectory, "MuMu模拟器12.lnk");
                }
            }
            if (path != string.Empty)
            {
                int index = path.LastIndexOf("\\");
                Path = path.Substring(0, index + 1).Trim('"');
                BmpPath = BmpPathGet();
            }
        }
        public override void Dispose() { }
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
        private List<KeyValuePair<int, string>> PlayerList()
        {
            int current_select_player_index = 0;
            List<int> player_list_index = new List<int>();
            string[] resultArray = PipeCmd("api get_player_list").Trim("\n".ToCharArray()).Split("\n".ToCharArray());
            for (int i = 0; i < resultArray.Length; i++)
            {
                string[] line = resultArray[i].Split(':');
                if (i == 0)
                {
                    int temp_int = 0;
                    int.TryParse(line[1].Trim(), out temp_int);
                    current_select_player_index = temp_int;
                }
                else if (i == 1)
                {
                    player_list_index = line[1].Trim().Replace(" result","").Trim('[',',',']').Split(",").Select(d=> int.Parse(d) ).ToList();
                }
            }
            var player_list = player_list_index.Select(d =>
            {
                var player_name = GetPlayerName(d);
                return new KeyValuePair<int, string>(d, player_name.Trim());
            }).ToList();

            return player_list;
        }

        private string GetPlayerName(int index)
        {
            return PipeCmd($"setting -v {index} get_key player_name");
        }
        public override bool IsStart(int index)
        {
            string[] resultArray = PipeCmd($"get player state: api -v {index} player_state").Trim("\n".ToCharArray()).Split("\n".ToCharArray());
            bool isIndex = false, isStart = false;
            for (int i = 0; i < resultArray.Length; i++)
            {

                string[] LineArray = resultArray[i].Split(':');
                if (LineArray.Length > 1)
                {
                    if (i == 0 && LineArray[1].Trim() == index.ToString())
                    {
                        isIndex = true;
                    }
                    if (i == 1 && LineArray[1].Trim() == "state=start_finished")
                    {
                        isStart = true;
                    }

                }
            }
            return isIndex && isStart;
        }
        public override Task<List<KeyValuePair<int, string>>> ListAll()
        {
            var list = PlayerList();
            return Task.FromResult(list);
        }
        public override async Task<Bitmap> ScreenShot(int Index)
        {
            var task = Task.Run(() =>
            {
                if (!IsStart(Index))
                {
                    MessageBox.Show("模拟器未启动 ! ");
                    return new Bitmap(1, 1);
                }
                string BmpName = "Screen_" + DateTime.Now.ToString("yy-MM-dd-HH-mm-ss") + ".png";
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
            var r = PipeCmd($" adb -v {index.ToString()} shell screencap -p '{savePath}' ");
            Console.WriteLine(r);
        }
        private void SaveImage(int index, string img_path, string local_path)//截图
        {
            var r = PipeCmd($"adb -v {index}  pull {img_path} {local_path} ");
            Console.WriteLine(r);
        }

        public string BmpPathGet()
        {
            try
            {
                return $"{System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory)}";
            }
            catch
            {
                return "";
            }
        }
        public string PipeCmd(string theCommand)
        {
            string path = Path + "MuMuManager.exe";
            ProcessStartInfo start = new ProcessStartInfo(path)
            {
                Arguments = theCommand,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                StandardOutputEncoding =Encoding.UTF8,
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
