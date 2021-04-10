using ScriptGraphicHelper.Views;
using System;
using System.Runtime.InteropServices;

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
            MessageBox(MainWindow.Instance.Handle, msg, title, uType);
        }

        [DllImport("Comdlg32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);



        internal const uint SPI_SETCURSORS = 87;
        internal const uint SPIF_SENDWININICHANGE = 2;

        [DllImport("user32", CharSet = CharSet.Unicode)]
        internal static extern IntPtr LoadCursorFromFile(string fileName);

        [DllImport("User32.DLL")]
        internal static extern bool SetSystemCursor(IntPtr hcur, uint id);
        internal const uint OCR_NORMAL = 32512;

        [DllImport("User32.DLL")]
        internal static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);
    }
}
