// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

#region Using directives
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using TKUtils;
using MessageBoxImage = TKUtils.MessageBoxImage;
using NLog;
#endregion Using directives

namespace PathTools
{
    public partial class LogFileWindow : Window
    {
        #region NLog
        private static readonly Logger logTemp = LogManager.GetLogger("logTemp");
        private static readonly Logger logPerm = LogManager.GetLogger("logPerm");
        #endregion NLog

        public LogFileWindow()
        {
            InitializeComponent();

            ReadSettings();
        }

        #region Read Settings
        // Read settings for this window
        private void ReadSettings()
        {
            string lf = UserSettings.Setting.LogFile;
            if (!string.IsNullOrEmpty(lf))
            {
                tbLogFileName.Text = lf;
            }

            cbKeepLog.IsChecked = UserSettings.Setting.KeepLogFile;

            if (cbKeepLog.IsChecked != true)
            {
                tbLogFileName.IsEnabled = false;
                tb1.Foreground = Brushes.Gray;
            }
        }
        #endregion Settings

        #region Button Events
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (cbKeepLog.IsChecked == true)
            {
                if (string.IsNullOrEmpty(tbLogFileName.Text))
                {
                    SystemSounds.Asterisk.Play();
                    _ = tbLogFileName.Focus();
                    tbLogFileName.Background = Brushes.LemonChiffon;
                    lblStatus.Foreground = Brushes.Red;
                    lblStatus.FontWeight = FontWeights.Bold;
                    lblStatus.FontSize = 16;
                    lblStatus.Text = "File name can't be blank";
                    return;
                }

                UserSettings.Setting.LogFile = tbLogFileName.Text;

                if (!File.Exists(tbLogFileName.Text))
                {
                    try
                    {
                        GlobalDiagnosticsContext.Set("LogPerm", tbLogFileName.Text);
                        logPerm.Info($"This log file was created by {AppInfo.AppName} {AppInfo.TitleVersion}");
                    }
                    catch (System.Exception ex)
                    {
                        logTemp.Error("Error creating log file.");
                        logTemp.Error(ex, $"Error creating log file: {UserSettings.Setting.LogFile}");
                        _ = TKMessageBox.Show($"Error creating log file.\n{ex.Message}",
                                              "ERROR",
                                              MessageBoxButton.OK,
                                              MessageBoxImage.Error);
                    }
                }
            }
            Close();
        }

        private void BtnOpenDlg_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlgOpen = new OpenFileDialog
            {
                Title = "Choose Log File",
                Multiselect = false,
                CheckFileExists = false,
                CheckPathExists = true,
                Filter = "Log files (*.log;*.txt)|*.log;*.txt| All files (*.*)|*.*"
            };
            bool? result = dlgOpen.ShowDialog();
            if (result == true)
            {
                tbLogFileName.Text = dlgOpen.FileName;
            }
        }
        #endregion Button Events

        #region Checkbox events
        // Checkbox
        private void CbKeepLog_Unchecked(object sender, RoutedEventArgs e)
        {
            UserSettings.Setting.KeepLogFile = false;
            tbLogFileName.IsEnabled = false;
            tb1.Foreground = Brushes.Gray;
        }

        private void CbKeepLog_Checked(object sender, RoutedEventArgs e)
        {
            UserSettings.Setting.KeepLogFile = true;
            tbLogFileName.IsEnabled = true;
            tb1.Foreground = Brushes.Black;
        }
        #endregion

        #region Window events
        //Window Closing
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (cbKeepLog.IsChecked == true && string.IsNullOrEmpty(tbLogFileName.Text))
            {
                _ = TKMessageBox.Show("Please supply a log file name or uncheck 'Keep a log' checkbox",
                                      "Try again please",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Warning);
                e.Cancel = true;
            }
            else
            {
                UserSettings.SaveSettings();
            }

            Debug.WriteLine("LogFileWindow closing");
        }

        // Window loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("LogFileWindow loaded");
        }
        #endregion Window events

        #region Keyboard events
        // Keyboard
        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                Close();
            }
        }
        #endregion Keyboard events

        #region Textbox events
        private void TbLogFileName_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            tbLogFileName.Background = Brushes.White;
            lblStatus.Foreground = Brushes.Black;
            lblStatus.FontWeight = FontWeights.Normal;
            lblStatus.Text = string.Empty;
        }

        private void TbLogFileName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            tbLogFileName.Background = Brushes.White;
            lblStatus.Foreground = Brushes.Black;
            lblStatus.FontWeight = FontWeights.Normal;
            lblStatus.Text = string.Empty;
        }
        #endregion Textbox events

        #region Move the MessageBox window
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
        #endregion Move the MessageBox window
    }
}