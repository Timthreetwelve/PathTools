// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

#region Using Directives
using System;
using System.Diagnostics;
using System.Media;
using System.Windows;
using System.Windows.Input;
using NLog;
using TKUtils;
#endregion Using Directives

namespace PathToolsAlert
{
    public partial class MainWindow : Window
    {
        #region NLog
        private static readonly Logger logTemp = LogManager.GetLogger("logTemp");
        #endregion NLog

        public MainWindow()
        {
            UserSettings.Init(UserSettings.AppFolder, UserSettings.DefaultFilename, true);
            InitializeComponent();
            Setup();
            tbMessage.Text = GetMessageText();
        }

        #region Setup
        private void Setup()
        {
            // Change the log file filename when debugging
            if (Debugger.IsAttached)
            {
                GlobalDiagnosticsContext.Set("TempOrDebug", "debug");
            }
            else
            {
                GlobalDiagnosticsContext.Set("TempOrDebug", "temp");
            }

            logTemp.Info($"{AppInfo.AppName} version {AppInfo.TitleVersion} is starting up with argument" +
                         $" \"{GetMessageText()}.\"");

            PositionWindow();
        }
        #endregion Setup

        #region Move Window to Bottom Right Corner
        private void PositionWindow()
        {
            var r = SystemParameters.WorkArea;
            Left = r.Right - Width - 3;
            Top = r.Bottom - Height - 3;
        }
        #endregion Move Window to Bottom Right Corner

        #region Process command line to get message
        private static string GetMessageText()
        {
            // If count is less that two, bail out
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length < 2)
            {
                logTemp.Error("Argument count < 2.");
                return string.Empty;
            }
            if (UserSettings.Setting.Sound)
            {
                SystemSounds.Exclamation.Play();
            }
            return args[1].Replace("-", "").Replace("/", "");
        }
        #endregion Process command line to get message

        #region Move the MessageBox window
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        #endregion Move the MessageBox window

        #region Button Events
        private void Show_Click(object sender, RoutedEventArgs e)
        {
            logTemp.Info($"{AppInfo.AppName} is starting PathTools.exe.");
            using Process pt = new Process();
            pt.StartInfo.FileName = @".\PathTools.exe";
            try
            {
                _ = pt.Start();
            }
            catch (Exception ex)
            {
                logTemp.Error(ex, "Unable to start PathTools.exe.");
            }
            Close();
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion Button Events

        #region Window Events
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            logTemp.Info($"{AppInfo.AppName} is shutting down.");
            LogManager.Shutdown();
        }
        #endregion Window Events
    }
}
