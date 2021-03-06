using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models.UnmanagedMethods
{
    public struct POINT
    {
        public int X;
        public int Y;
        public POINT(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
    public static class Win32Api
    {
        [DllImport("user32.dll")]
        public static extern int SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT point);

        public static POINT GetCursorPos()
        {
            var point = new POINT();
            GetCursorPos(out point);
            return point;
        }

        [DllImport("user32.dll")]
        public static extern int MessageBox(IntPtr hWnd, string msg, string title, int uType);

        public static void MessageBox(string msg, string title = "提示", int uType = 0)
        {
            MessageBox(IntPtr.Zero, msg, title, uType);
        }

        [DllImport("Comdlg32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
    }
}
