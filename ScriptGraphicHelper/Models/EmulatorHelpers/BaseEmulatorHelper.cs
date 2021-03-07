﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models.EmulatorHelpers
{
    public abstract class BaseEmulatorHelper : IDisposable
    {
        public abstract string Path { get; set; }
        public abstract string Name { get; set; }
        public abstract bool IsStart(int Index);
        public abstract Task<List<KeyValuePair<int, string>>> ListAll();
        public abstract Task<Bitmap> ScreenShot(int Index);
        public abstract void Dispose();
    }
}
