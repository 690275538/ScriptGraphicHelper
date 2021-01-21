﻿using Prism.Ioc;
using ScriptGraphicHelper.Views;
using System.Windows;
using System;

namespace ScriptGraphicHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry) { }
        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Exception exception = e.Exception;
            MessageBox.Show("捕获到未处理的异常 , 请将本提示截图给开发者 \r\n\r\n" + exception.ToString(), "程序即将退出");
            e.Handled = true;
            Current.Shutdown();

        }
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = (Exception)e.ExceptionObject;
            MessageBox.Show("捕获到未处理的异常, 请将本提示截图给开发者\r\n\r\n" + exception.ToString(), "程序即将退出");
            Current.Shutdown();
        }
    }
}
