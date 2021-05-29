using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models.ScreenshotHelpers
{
    public abstract class BaseScreenshotHelper : IDisposable
    {
        public abstract string Path { get; set; }
        public abstract string Name { get; set; }
        public abstract bool IsStart(int Index);
        public abstract Task<List<KeyValuePair<int, string>>> ListAll();
        public abstract Task<Bitmap> ScreenShot(int Index);
        public abstract void Dispose();
    }
}
