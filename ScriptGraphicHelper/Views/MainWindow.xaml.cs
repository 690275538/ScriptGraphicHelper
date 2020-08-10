﻿using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ScriptGraphicHelper.Models;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Newtonsoft.Json;
using static System.Environment;
using System.Windows.Controls;

namespace ScriptGraphicHelper.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private DispatcherTimer showTimer = new DispatcherTimer();
        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(Theme.Dark);
            theme.SecondaryMid = new ColorPair(Color.FromRgb(0x66, 0x66, 0x66), Colors.White);
            theme.PrimaryMid = new ColorPair(Color.FromRgb(0x66, 0x66, 0x66), Colors.White);
            paletteHelper.SetTheme(theme);
            FocusManager.SetFocusedElement(Img, SelectRectangle);

            showTimer.Tick += new EventHandler(Panel_5_SetVisibility);
            try
            {
                StreamReader sr = File.OpenText(CurrentDirectory + "\\setting.json");
                string configStr = sr.ReadToEnd();
                sr.Close();
                Setting setting = JsonConvert.DeserializeObject<Setting>(configStr);
                if (setting.LastSize)
                {
                    Width = setting.LastWidth;
                    Height = setting.LastHeight;
                }
                if (setting.LastSim && Sim.Items.Count > setting.SimSelect)
                {
                    Sim.SelectedIndex = setting.SimSelect;
                }
                if (setting.LastFormat && Format.Items.Count > setting.SimSelect)
                {
                    Format.SelectedIndex = setting.FormatSelect;
                }
            }
            catch { }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            Setting setting;
            try
            {
                StreamReader sr = File.OpenText(CurrentDirectory + "\\setting.json");
                string configStr = sr.ReadToEnd();
                sr.Close();
                setting = JsonConvert.DeserializeObject<Setting>(configStr);
            }
            catch
            {
                setting = new Setting();
            }
            if (setting.LastSize)
            {
                setting.LastWidth = Width;
                setting.LastHeight = Height;
            }
            if (setting.LastSim)
            {
                setting.SimSelect = Sim.SelectedIndex;
            }
            if (setting.LastFormat)
            {
                setting.FormatSelect = Format.SelectedIndex;
            }
           
            string settingStr = JsonConvert.SerializeObject(setting, Formatting.Indented);
            File.WriteAllText(CurrentDirectory + "\\setting.json", settingStr);
        }

        private void CopyPoint_Click(object sender, RoutedEventArgs e)
        {
            if (Color_Info.SelectedIndex != -1)
            {
                IList<ColorInfo> colorInfos = (IList<ColorInfo>)Color_Info.ItemsSource;
                ColorInfo colorInfo = colorInfos[Color_Info.SelectedIndex];
                Clipboard.SetText(colorInfo.PointStr);
            }
        }

        private void CopyColor_Click(object sender, RoutedEventArgs e)
        {
            if (Color_Info.SelectedIndex != -1)
            {
                IList<ColorInfo> colorInfos = (IList<ColorInfo>)Color_Info.ItemsSource;
                ColorInfo colorInfo = colorInfos[Color_Info.SelectedIndex];
                Clipboard.SetText(colorInfo.ColorStr);
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Panel_1.MaxHeight = ActualHeight - 50;
            Panel_2.MaxWidth = ActualWidth - Panel_1.ActualWidth - Panel_3.ActualWidth - 20;
            Panel_2.MaxHeight = ActualHeight - 45;
            Color_Info.MaxHeight = ActualHeight - 100;
        }

        [DllImport("user32.dll")]
        private static extern int SetCursorPos(int x, int y);
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            Point point = PointToScreen(Mouse.GetPosition(this));
            if (Panel_4.Visibility == Visibility.Visible)
            {
                if (key == Key.Up)
                {
                    SetCursorPos((int)point.X, (int)point.Y - 1);
                }
                else if (key == Key.Down)
                {
                    SetCursorPos((int)point.X, (int)point.Y + 1);
                }
                else if (key == Key.Left)
                {
                    SetCursorPos((int)point.X - 1, (int)point.Y);
                }
                else if (key == Key.Right)
                {
                    SetCursorPos((int)point.X + 1, (int)point.Y);
                }
                e.Handled = true;
            }

        }
        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ColorString.Text);
        }

        private void Panel_5_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Panel_5.Visibility == Visibility.Visible)
            {
                showTimer.Interval = new TimeSpan(0, 0, 0, 5);
                showTimer.Start();
            }
        }

        private void Panel_5_SetVisibility(object sender, EventArgs e)
        {
            if (Panel_5.Visibility == Visibility.Visible)
            {
                Panel_5.Visibility = Visibility.Hidden;
                showTimer.Stop();
            }
        }

        private void OffsetListState_Click(object sender, RoutedEventArgs e)
        {
            if (OffsetListState.Header.ToString() == "显示偏色列表")
            {
                OffsetList.Visibility = Visibility.Visible;
                OffsetListState.Header = "隐藏偏色列表";
            }
            else
            {
                OffsetList.Visibility = Visibility.Collapsed;
                OffsetListState.Header = "显示偏色列表";
            }

        }
    }
}
