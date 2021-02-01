// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using NLog;
using TKUtils;
using MessageBoxImage = TKUtils.MessageBoxImage;

namespace PathTools
{
    internal static class TextFileViewer
    {
        #region NLog
        private static readonly Logger logTemp = LogManager.GetLogger("logTemp");
        #endregion NLog

        #region Text file viewer
        public static void ViewTextFile(string txtfile)
        {
            if (File.Exists(txtfile))
            {
                try
                {
                    using (Process p = new Process())
                    {
                        p.StartInfo.FileName = txtfile;
                        p.StartInfo.UseShellExecute = true;
                        p.StartInfo.ErrorDialog = false;
                        _ = p.Start();
                    }
                }
                catch (Win32Exception ex)
                {
                    if (ex.NativeErrorCode == 1155)
                    {
                        using (Process p = new Process())
                        {
                            p.StartInfo.FileName = "notepad.exe";
                            p.StartInfo.Arguments = txtfile;
                            p.StartInfo.UseShellExecute = true;
                            p.StartInfo.ErrorDialog = false;
                            _ = p.Start();
                            logTemp.Info($"Falling back to Notepad to open {txtfile}");
                        }
                    }
                    else
                    {
                        _ = TKMessageBox.Show($"Error reading file {txtfile}\n{ex.Message}",
                                              "Error",
                                              MessageBoxButton.OK,
                                              MessageBoxImage.Error);
                        logTemp.Error(ex, $"Unable to open {txtfile}");
                    }
                }
                catch (Exception ex)
                {
                    _ = TKMessageBox.Show($"Unable to start default application used to open {txtfile}",
                                          "Error",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Error);
                    logTemp.Error(ex, $"Unable to open {txtfile}");
                }
            }
            else
            {
                Debug.WriteLine($">>> File not found: {txtfile}");
                logTemp.Error($"{txtfile} could not be found");
                _ = TKMessageBox.Show($"The file: {txtfile} could not be found.",
                      "Error",
                      MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
