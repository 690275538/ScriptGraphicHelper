using Avalonia;
using Avalonia.Input;
using Avalonia.Platform;
using ReactiveUI;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.ViewModels.Core;
using ScriptGraphicHelper.Views;
using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace ScriptGraphicHelper.ViewModels
{

    public class Win32Api
    {
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

    public class HwndConfigViewModel : ViewModelBase
    {
        private HwndConfig? configWindow;
        public HwndConfig? ConfigWindow
        {
            get => configWindow;
            set => this.RaiseAndSetIfChanged(ref configWindow, value);
        }


        private ObservableCollection<MoveCategory> hwndInfos = new();
        public ObservableCollection<MoveCategory> HwndInfos
        {
            get => hwndInfos;
            set => this.RaiseAndSetIfChanged(ref hwndInfos, value);
        }

        private MoveCategory? selectedItem;
        public MoveCategory? SelectedItem
        {
            get => selectedItem;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedItem, value);
                BindHwnd = value.Hwnd;
            }
        }


        private int bindHwnd = -1;
        public int BindHwnd
        {
            get => bindHwnd;
            set => this.RaiseAndSetIfChanged(ref bindHwnd, value);
        }

        private int bindGraphicMode = 0;
        public int BindGraphicMode
        {
            get => bindGraphicMode;
            set => this.RaiseAndSetIfChanged(ref bindGraphicMode, value);
        }

        private int bindAttribute = 0;
        public int BindAttribute
        {
            get => bindAttribute;
            set => this.RaiseAndSetIfChanged(ref bindAttribute, value);
        }

        private int bindMode = 0;
        public int BindMode
        {
            get => bindMode;
            set => this.RaiseAndSetIfChanged(ref bindMode, value);
        }

        private Dmsoft Dm = Dmsoft.Instance;

        public HwndConfigViewModel(HwndConfig hwndConfig)
        {
            hwndInfos = new ObservableCollection<MoveCategory>();
            ConfigWindow = hwndConfig;
        }

        public void Ok_Tapped()
        {
            if (BindHwnd == -1)
            {
                MessageBox.ShowAsync("请选择句柄!");
                return;
            }
            string[] graphicModes = new string[] { "normal", "gdi", "gdi2", "dx2", "dx3", "dx.graphic.2d", "dx.graphic.2d.2", "dx.graphic.3d", "dx.graphic.3d.8", "dx.graphic.opengl", "dx.graphic.opengl.esv2", "dx.graphic.3d.10plus" };
            string[] attributes = new string[] { "", "dx.public.active.api", "dx.public.active.message", "dx.public.hide.dll", "dx.public.graphic.protect", "dx.public.anti.api", "dx.public.prevent.block", "dx.public.inject.super" };
            int[] modes = new int[] { 0, 2, 101, 103, 11, 13 };
            Dm.Hwnd = BindHwnd;
            Dm.Display = graphicModes[BindGraphicMode];
            Dm.Public_desc = attributes[BindAttribute];
            Dm.Mode = modes[BindMode];
            ConfigWindow.Close();
        }

        public ICommand GetHwnd_PointerPressed => new Command((param) =>
        {
            if (param != null)
            {
                CommandParameters parameters = (CommandParameters)param;
                var eventArgs = (PointerPressedEventArgs)parameters.EventArgs;
                if (eventArgs.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
                {
                    IntPtr cur = Win32Api.LoadCursorFromFile(AppDomain.CurrentDomain.BaseDirectory + @"Assets/aiming.cur");
                    Win32Api.SetSystemCursor(cur, Win32Api.OCR_NORMAL);
                }

            }
        });

        public ICommand GetHwnd_PointerReleased => new Command((param) =>
        {
            HwndInfos.Clear();
            Win32Api.SystemParametersInfo(Win32Api.SPI_SETCURSORS, 0, IntPtr.Zero, Win32Api.SPIF_SENDWININICHANGE);
            int hwnd = Dm.GetMousePointWindow() ?? -1;
            int parentHwnd = Dm.GetWindow(hwnd, 7) ?? -1;
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            HwndInfos.Add(new MoveCategory(parentHwnd, Dm.GetWindowTitle(parentHwnd) ?? string.Empty, Dm.GetWindowClass(parentHwnd) ?? string.Empty));
            EnumWindows(parentHwnd, HwndInfos[0]);
        });

        private void EnumWindows(int parentHwd, MoveCategory movieCategory)
        {
            string[] hwnds = Dm.EnumWindow(parentHwd, "", "", 4).Split(',');
            for (int i = 0; i < hwnds.Length; i++)
            {
                if (hwnds[i].Trim() != "")
                {
                    int hwnd = int.Parse(hwnds[i].Trim());
                    movieCategory.Moves.Add(new MoveCategory(hwnd, Dm.GetWindowTitle(hwnd) ?? string.Empty, Dm.GetWindowClass(hwnd) ?? string.Empty));
                    EnumWindows(hwnd, movieCategory.Moves[i]);
                }
            }
        }
    }
}
