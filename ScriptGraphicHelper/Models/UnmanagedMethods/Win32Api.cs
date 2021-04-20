using Avalonia.Input;
using Avalonia.Threading;
using ScriptGraphicHelper.Views;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
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

    public enum ClipboardFormat
    {
        CF_TEXT = 1,
        CF_BITMAP = 2,
        CF_DIB = 3,
        CF_UNICODETEXT = 13,
        CF_HDROP = 15,
        CF_FILENAME = 49158,
        CF_FILENAMEW = 49159,
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

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool OpenClipboard(IntPtr hWndOwner);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        public static extern uint EnumClipboardFormats(uint format);

        [DllImport("user32.dll")]
        public static extern int GetClipboardFormatName(uint format, [Out] StringBuilder lpszFormatName, int cchMaxCount);

        [DllImport("user32.dll")]
        public static extern IntPtr GetClipboardData(ClipboardFormat uFormat);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern IntPtr GlobalLock(IntPtr handle);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern bool GlobalUnlock(IntPtr handle);

        private static async Task<IDisposable> OpenClipboard()
        {
            var i = 10;

            while (!OpenClipboard(IntPtr.Zero))
            {
                if (--i == 0)
                    throw new TimeoutException("Timeout opening clipboard.");
                await Task.Delay(100);
            }

            return Disposable.Create(() => CloseClipboard());
        }

        private static async Task<string> GetFormatDataAsync(ClipboardFormat format)
        {
            using (await OpenClipboard())
            {
                IntPtr ptr = GetClipboardData(format);
                if (ptr == IntPtr.Zero)
                {
                    return null;
                }
                var rv = Marshal.PtrToStringUni(ptr);
                return rv ?? string.Empty;
            }
        }

        public static async Task<string> GetTextAsync()
        {
            return await GetFormatDataAsync(ClipboardFormat.CF_UNICODETEXT) ?? await GetFormatDataAsync(ClipboardFormat.CF_TEXT) ?? string.Empty;
        }

        public static async Task<string> GetFileNameAsync()
        {
            return await GetFormatDataAsync(ClipboardFormat.CF_FILENAMEW) ?? await GetFormatDataAsync(ClipboardFormat.CF_FILENAME) ?? string.Empty;
        }

        public static async Task<Bitmap?> GetBitmapAsync()
        {
            using (await OpenClipboard())
            {
                IntPtr ptr = GetClipboardData(ClipboardFormat.CF_BITMAP);
                if (ptr == IntPtr.Zero)
                {
                    return null;
                }
                return Image.FromHbitmap(ptr);
            }
        }


    }
}
