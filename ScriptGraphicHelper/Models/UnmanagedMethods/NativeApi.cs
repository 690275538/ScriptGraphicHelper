using ScriptGraphicHelper.Views;
using System;
using System.Drawing;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models.UnmanagedMethods
{

    public static class NativeApi
    {
        [DllImport("mouse.dll")]
        public static extern void Move2Left();

        [DllImport("mouse.dll")]
        public static extern void Move2Top();

        [DllImport("mouse.dll")]
        public static extern void Move2Right();

        [DllImport("mouse.dll")]
        public static extern void Move2Bottom();
    }
}
