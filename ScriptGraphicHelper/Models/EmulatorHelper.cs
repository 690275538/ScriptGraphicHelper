using Avalonia.Media.Imaging;
using ScriptGraphicHelper.Models.EmulatorHelpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models
{
    public enum EmlatorState
    {
        None = -1,
        Waiting = 0,
        Starting = 1,
        success = 2

    }

    public static class EmulatorHelper
    {
        public static EmlatorState State { get; set; } = EmlatorState.None;
        public static ObservableCollection<string> Result { get; set; }
        public static List<KeyValuePair<int, string>> Info { get; set; }
        public static int Select { get; set; } = -1;

        private static int _index = -1;
        public static int Index
        {
            get { return _index; }
            set
            {
                if (value != -1)
                {
                    _index = Info[value].Key;
                }
            }
        }

        public static List<BaseEmulatorHelper> Helpers = new List<BaseEmulatorHelper>();
        public static ObservableCollection<string> Init()
        {
            Helpers = new List<BaseEmulatorHelper>();
            Helpers.Add(new LdEmulatorHelper(0));
            Helpers.Add(new LdEmulatorHelper(1));
            Helpers.Add(new LdEmulatorHelper(2));
            Helpers.Add(new YsEmulatorHelper());
            Helpers.Add(new XyEmulatorHelper());
            Helpers.Add(new MoblieTcpHelper());
            Helpers.Add(new HwndHelper());
            Result = new ObservableCollection<string>();

            foreach (var emulator in Helpers)
            {
                if (emulator.Path != string.Empty && emulator.Path != "")
                {
                    Result.Add(emulator.Name);
                }
            }
            State = 0;
            return Result;
        }
        public static void Dispose()
        {
            try
            {
                foreach (var emulator in Helpers)
                {
                    emulator.Dispose();
                }
                Result.Clear();
                Info.Clear();
                Helpers.Clear();

                State = EmlatorState.None;
            }
            catch { }

        }
        public static void Changed(int index)
        {
            if (index >= 0)
            {
                for (int i = 0; i < Helpers.Count; i++)
                {
                    if (Helpers[i].Name == Result[index])
                    {
                        Select = i;
                        State = EmlatorState.Starting;
                    }
                }
            }
            else
            {
                Select = -1;
                State = EmlatorState.Starting;
            }
        }
        public async static Task<ObservableCollection<string>> GetAll()
        {
            ObservableCollection<string> result = new ObservableCollection<string>();
            Info = await Helpers[Select].ListAll();
            foreach (var item in Info)
            {
                result.Add(item.Value);
            }
            return result;
        }
        public async static Task<Bitmap> ScreenShot()
        {
            return await Helpers[Select].ScreenShot(Index);
        }
    }
}
