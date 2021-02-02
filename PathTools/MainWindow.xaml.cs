// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

#region using directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using NLog;
using NLog.Targets;
using TKUtils;
using MessageBoxImage = TKUtils.MessageBoxImage;
#endregion

namespace PathTools
{
    public partial class MainWindow : Window
    {
        #region Private constants
        private const int SW_SHOW = 5;
        private const uint SEE_MASK_INVOKEIDLIST = 12;
        #endregion Private constants

        #region Filenames
        private readonly string savedPathFile = Path.Combine(AppInfo.AppDirectory, "SavedPath.json");
        #endregion

        #region Stopwatch
        private readonly Stopwatch stopwatch = new Stopwatch();
        #endregion Stopwatch

        #region NLog
        private static readonly Logger logTemp = LogManager.GetLogger("logTemp");
        private static readonly Logger logPerm = LogManager.GetLogger("logPerm");
        #endregion NLog

        public MainWindow()
        {
            UserSettings.Init(UserSettings.AppFolder, UserSettings.DefaultFilename, true);

            InitializeComponent();

            ReadSettings();

            ProcessCommandLine();

            FirstRunSetup();

            ComparePath(GetCurrentPath());

            WriteSavedPathFile();

            ShowOrNot();
        }

        #region Read settings file
        private void ReadSettings()
        {
            // Change the log file filename when debugging
            if (Debugger.IsAttached)
                GlobalDiagnosticsContext.Set("TempOrDebug", "debug");
            else
                GlobalDiagnosticsContext.Set("TempOrDebug", "temp");

            // Log startup
            WriteTempFile($"{AppInfo.AppName} {AppInfo.TitleVersion} is starting up");
            WriteTempFile($"Path to {AppInfo.AppName} is {AppInfo.AppPath}");

            // Start the elapsed time timer
            stopwatch.Start();

            // Unhandled exception handler
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Settings change event
            UserSettings.Setting.PropertyChanged += UserSettingChanged;

            // First time?
            if (UserSettings.Setting.FirstRun)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            // Window position
            Top = UserSettings.Setting.WindowTop;
            Left = UserSettings.Setting.WindowLeft;
            Topmost = UserSettings.Setting.KeepOnTop;

            // Alternate row shading
            if (UserSettings.Setting.ShadeAltRows)
            {
                AltRowShadingOn();
            }

            // Show grid lines
            if (!UserSettings.Setting.ShowGridLines)
            {
                dgPath.GridLinesVisibility = DataGridGridLinesVisibility.None;
            }

            // Datagrid font size
            double curZoom = UserSettings.Setting.GridZoom;
            dgPath.LayoutTransform = new ScaleTransform(curZoom, curZoom);

            if (!UserSettings.Setting.ShowToolbox)
            {
                buttonPanel.Visibility = Visibility.Collapsed;
                headerPanel.Visibility = Visibility.Visible;
            }
            else
            {
                buttonPanel.Visibility = Visibility.Visible;
                headerPanel.Visibility = Visibility.Collapsed;
            }

            // Put version number in title bar
            WindowTitleVersion();

            // Verify the saved path file
            CheckSavedPath();
        }
        #endregion Read settings file

        #region Process command line args
        private void ProcessCommandLine()
        {
            // If count is less that two, bail out
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length < 2)
                return;

            foreach (string item in args)
            {
                if (item.Replace("-", "").Replace("/", "").Equals("hide", StringComparison.OrdinalIgnoreCase))
                {
                    // hide the window
                    Visibility = Visibility.Hidden;

                    WriteTempFile("Argument \"hide\" specified ");
                }
                else if (item != args[0])
                {
                    WriteTempFile($"Unknown command line argument  \"{item}\" found.");
                }
            }
        }
        #endregion Process command line args

        #region Check the saved path file
        private void CheckSavedPath()
        {
            // If the path file doesn't exist, create a new one and seed it with two records
            if (!File.Exists(savedPathFile))
            {
                WriteTempFile($"Creating saved path file: {savedPathFile}");
                List<SavedPath> spl = new List<SavedPath>();
                SavedPath sp = new SavedPath
                {
                    SeqNumber = 1,
                    PathType = "Machine",
                    PathDirectory = @"C:\WINDOWS\system32"
                };
                spl.Add(sp);
                sp = new SavedPath
                {
                    SeqNumber = 1,
                    PathType = "Machine",
                    PathDirectory = @"C:\WINDOWS"
                };
                spl.Add(sp);
                string json = JsonSerializer.Serialize(spl);
                File.WriteAllText(savedPathFile, json);
            }
            else
            {
                WriteTempFile($"Saved path file: {savedPathFile}");
            }
        }
        #endregion Saved path

        #region Get the current path
        private static List<SavedPath> GetCurrentPath()
        {
            // Get both the Machine and User paths
            string machinePath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine);
            string userPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User);
            DisplayPath.TotalPathLength = (machinePath + userPath).Length;

            List<SavedPath> curPath = new List<SavedPath>();

            int seqNum = 0;

            // Get each directory in the machine path
            foreach (string part in machinePath.TrimEnd(';').Split(';'))
            {
                seqNum++;
                SavedPath x = new SavedPath
                {
                    SeqNumber = seqNum,
                    PathType = "Machine",
                    PathDirectory = part
                };
                curPath.Add(x);
            }

            // Get each directory in the user path
            foreach (string part in userPath.TrimEnd(';').Split(';'))
            {
                seqNum++;
                SavedPath x = new SavedPath
                {
                    SeqNumber = seqNum,
                    PathType = "User",
                    PathDirectory = part
                };
                curPath.Add(x);
            }
            DisplayPath.TotalInPath = curPath.Count;

            return curPath;
        }
        #endregion Get the current path

        #region Compare current path to saved path
        private void ComparePath(List<SavedPath> curPath)
        {
            List<SavedPath> listSavedPath = ListSaved();
            List<DisplayPath> listCurrentPath = ListCurrent(curPath);
            List<DisplayPath> listMerged = new List<DisplayPath>();

            IEnumerable<string> listUnchanged;
            IEnumerable<string> listAdded;
            IEnumerable<string> listRemoved;

            if (UserSettings.Setting.CaseSensitive)
            {
                WriteTempFile("Comparisons are case sensitive");
                // Unchanged
                listUnchanged = listSavedPath.Select(x => x.PathDirectory)
                    .Intersect(listCurrentPath.Select(x => x.PathDirectory), StringComparer.Ordinal);
                ListUnchanged(listCurrentPath, listUnchanged, listMerged);

                // Added
                listAdded = listCurrentPath.Select(x => x.PathDirectory)
                    .Except(listSavedPath.Select(x => x.PathDirectory), StringComparer.Ordinal);
                ListAdded(listCurrentPath, listAdded, listMerged);

                // Removed
                listRemoved = listSavedPath.Select(x => x.PathDirectory)
                    .Except(listCurrentPath.Select(x => x.PathDirectory), StringComparer.Ordinal);
                ListRemoved(listSavedPath, listRemoved, listMerged);
            }
            else
            {
                WriteTempFile("Comparisons are not case sensitive");
                // Unchanged
                listUnchanged = listSavedPath.Select(x => x.PathDirectory)
                    .Intersect(listCurrentPath.Select(x => x.PathDirectory), StringComparer.OrdinalIgnoreCase);
                ListUnchanged(listCurrentPath, listUnchanged, listMerged);

                // Added
                listAdded = listCurrentPath.Select(x => x.PathDirectory)
                    .Except(listSavedPath.Select(x => x.PathDirectory), StringComparer.OrdinalIgnoreCase);
                ListAdded(listCurrentPath, listAdded, listMerged);

                // Removed
                listRemoved = listSavedPath.Select(x => x.PathDirectory)
                    .Except(listCurrentPath.Select(x => x.PathDirectory), StringComparer.OrdinalIgnoreCase);
                ListRemoved(listSavedPath, listRemoved, listMerged);
            }

            // Sort the list by sequence number and return
            listMerged.Sort((x, y) => x.SeqNumber.CompareTo(y.SeqNumber));

            // Set source for data grid
            dgPath.ItemsSource = listMerged;

            DisplayPath.TotalAdded = listAdded.Count();
            DisplayPath.TotalRemoved = listRemoved.Count();

            string totalsMsg = $"Added: {DisplayPath.TotalAdded}  Removed: {DisplayPath.TotalRemoved}" +
                               $"  Unchanged: {listUnchanged.Count()}  Total: {DisplayPath.TotalInPath}";
            WriteTempFile(totalsMsg);

            if (DisplayPath.NumDifferences > 0)
            {
                string diffMsg = (DisplayPath.NumDifferences == 1) ? "Found 1 change" : $"Found {DisplayPath.NumDifferences} changes";
                WriteTempFile(diffMsg);
            }

            stbStatusL.Content = $"{DisplayPath.TotalInPath} Total.   {DisplayPath.TotalAdded} Added.   {DisplayPath.TotalRemoved} Removed.";
        }
        #endregion Compare current path to saved path

        #region Get saved path
        private List<SavedPath> ListSaved()
        {
            string json = File.ReadAllText(savedPathFile);
            return JsonSerializer.Deserialize<List<SavedPath>>(json);
        }
        #endregion Get saved path

        #region Get the current path
        private static List<DisplayPath> ListCurrent(List<SavedPath> curPath)
        {
            // Put current path into a list
            List<DisplayPath> listCurrentPath = new List<DisplayPath>();
            foreach (var item in curPath)
            {
                if (item != null)
                {
                    DisplayPath x = new DisplayPath
                    {
                        SeqNumber = item.SeqNumber,
                        PathType = item.PathType,
                        PathDirectory = item.PathDirectory
                    };
                    listCurrentPath.Add(x);
                }
            }

            return listCurrentPath;
        }
        #endregion Get the current path

        #region Get the unchanged directories
        private static void ListUnchanged(List<DisplayPath> listCurrentPath, IEnumerable<string> listUnchanged, List<DisplayPath> listMerged)
        {
            if (listUnchanged.Any())
            {
                foreach (var x in listCurrentPath)
                {
                    foreach (var s in listUnchanged)
                    {
                        if (x.PathDirectory.Equals(s, StringComparison.OrdinalIgnoreCase))
                        {
                            DisplayPath merge = new DisplayPath
                            {
                                SeqNumber = x.SeqNumber,
                                PathType = x.PathType,
                                PathDirectory = x.PathDirectory,
                                PathStatus = "Unchanged"
                            };
                            foreach (var path in listCurrentPath)
                            {
                                if (path.PathDirectory.Equals(s, StringComparison.OrdinalIgnoreCase) &&
                                    path.SeqNumber != x.SeqNumber)
                                {
                                    merge.PathStatus = "Duplicate";
                                    logTemp.Warn($"Duplicate found: {merge.SeqNumber} {merge.PathType} {merge.PathDirectory}");
                                    logPerm.Warn($"Duplicate found: {merge.SeqNumber} {merge.PathType} {merge.PathDirectory}");
                                    DisplayPath.NumDifferences++;
                                }
                            }
                            listMerged.Add(merge);
                            break;
                        }
                    }
                }
            }
        }
        #endregion Get the unchanged directories

        #region Get the directories that have been added
        private static void ListAdded(List<DisplayPath> listCurrentPath, IEnumerable<string> listAdded, List<DisplayPath> listMerged)
        {
            if (listAdded.Any())
            {
                foreach (var s in listAdded)
                {
                    foreach (var x in listCurrentPath)
                    {
                        if (x.PathDirectory.Equals(s, StringComparison.OrdinalIgnoreCase))
                        {
                            DisplayPath merge = new DisplayPath
                            {
                                SeqNumber = x.SeqNumber,
                                PathType = x.PathType,
                                PathDirectory = x.PathDirectory,
                                PathStatus = "Added"
                            };
                            listMerged.Add(merge);
                            DisplayPath.NumDifferences++;
                            if (UserSettings.Setting.KeepLogFile)
                            {
                                WriteMsg2Log("Added", x.PathType, x.PathDirectory);
                            }
                            break;
                        }
                    }
                }
            }
        }
        #endregion

        #region Get the directories that have been removed
        private static void ListRemoved(List<SavedPath> listSavedPath, IEnumerable<string> listRemoved, List<DisplayPath> listMerged)
        {
            if (listRemoved.Any())
            {
                foreach (var s in listRemoved)
                {
                    foreach (var x in listSavedPath)
                    {
                        if (x.PathDirectory.Equals(s, StringComparison.OrdinalIgnoreCase))
                        {
                            DisplayPath merge = new DisplayPath
                            {
                                SeqNumber = x.SeqNumber,
                                PathType = x.PathType,
                                PathDirectory = x.PathDirectory,
                                PathStatus = "Removed"
                            };
                            listMerged.Add(merge);
                            DisplayPath.NumDifferences++;
                            if (UserSettings.Setting.KeepLogFile)
                            {
                                WriteMsg2Log("Removed", x.PathType, x.PathDirectory);
                            }
                            break;
                        }
                    }
                }
            }
        }
        #endregion Get the directories that have been removed

        #region Check for duplicates
        private static void CheckForDupes(List<SavedPath> curPath)
        {
            var duplicates = curPath.GroupBy(x => x.PathDirectory).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
            if (duplicates.Count > 0)
            {
                List<SavedPath> allDupes = new List<SavedPath>();
                foreach (string dupe in duplicates)
                {
                    foreach (var item in curPath.FindAll(x => x.PathDirectory == dupe))
                    {
                        allDupes.Add(item);
                        logTemp.Warn($"Duplicate found: {item.SeqNumber} {item.PathType} {item.PathDirectory}");
                    }
                }
                Duplicates d = new Duplicates();
                d.dgDupes.ItemsSource = allDupes;
                d.Owner = Application.Current.MainWindow;
                d.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                _ = d.ShowDialog();
            }
            else
            {
                _ = TKMessageBox.Show("No duplicates found.",
                                      "PathTools",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Check);
            }
        }
        #endregion Check for duplicates

        #region Refresh DataGrid
        private void Refresh()
        {
            ComparePath(GetCurrentPath());
            dgPath.Items.Refresh();
        }
        #endregion Refresh DataGrid

        #region Show window if there are differences
        private void ShowOrNot()
        {
            // If hidden and differences found, show alert window
            if (Visibility == Visibility.Hidden)
            {
                if (DisplayPath.NumDifferences > 0)
                {
                    WriteTempFile("Changes found, showing window");
                    using Process pta = new Process();
                    pta.StartInfo.FileName = @".\PathToolsAlert.exe";
                    pta.StartInfo.Arguments = $"\"{ DisplayPath.NumDifferences} changes were found in the PATH\"";
                    try
                    {
                        _ = pta.Start();
                        Application.Current.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        logTemp.Error(ex, "Unable to start PathToolsAlert.exe");
                        Visibility = Visibility.Visible;
                        WindowState = WindowState.Normal;
                    }
                }
                // If no differences then quit
                else
                {
                    WriteTempFile("No differences found, shutting down");
                    Application.Current.Shutdown();
                }
            }
        }
        #endregion Show window if there are differences

        #region Log changes to PATH
        public static void WriteMsg2Log(string status, string type, string directory)
        {
            string message;

            if (!string.Equals(status, "blank", StringComparison.OrdinalIgnoreCase))
            {
                message = $"PATH change detected:  {status,-7}  {type,-7}  {directory}";
            }
            else
            {
                message = new string('-', 110);
            }
            WriteTempFile(message);
            WriteLogFile(message);
        }
        #endregion Log changes to PATH

        #region Title version
        public void WindowTitleVersion()
        {
            if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                Title = AppInfo.AppName + " - " + AppInfo.TitleVersion + " - (Administrator)";
            }
            else
            {
                Title = AppInfo.AppName + " - " + AppInfo.TitleVersion;
            }
        }
        #endregion Title version

        #region Mouse Over DataGrid row
        private void DataGridRow_MouseEnter(object sender, MouseEventArgs e)
        {
            if (UserSettings.Setting.ShowRightStatus)
            {
                try
                {
                    DataGridRow row = e.Source as DataGridRow;
                    DisplayPath dp = (DisplayPath)dgPath.Items.GetItemAt(row.GetIndex());
                    DirectoryInfo di = new DirectoryInfo(dp.PathDirectory);
                    FileInfo[] fi = di.GetFiles();
                    DirectoryInfo[] d = di.GetDirectories();
                    string fn = fi.Length == 1 ? "File" : "Files";
                    string dn = d.Length == 1 ? "Subdirectory" : "Subdirectories";
                    stbStatusR.Content = $"Contains {fi.Length:N0} {fn} and {d.Length:N0} {dn}";
                }
                catch (UnauthorizedAccessException ex)
                {
                    logTemp.Error(ex, "Error enumerating files and subdirectories");
                    stbStatusR.Content = "Not Authorized";
                }
                catch (Exception ex)
                {
                    logTemp.Error(ex, "Error enumerating files and subdirectories");
                    stbStatusR.Content = "Error";
                }
            }
        }

        private void DataGridRow_MouseLeave(object sender, MouseEventArgs e)
        {
            if (UserSettings.Setting.ShowRightStatus)
            {
                stbStatusR.Content = string.Empty;
            }
        }
        #endregion Mouse Over DataGrid row

        #region DataGrid Double Click
        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgPath.SelectedItem != null)
            {
                DisplayPath row = (DisplayPath)dgPath.SelectedItem;
                string selPath = Path.GetFullPath(row.PathDirectory);
                using Process exp = new Process();
                exp.StartInfo.FileName = "Explorer.exe";
                exp.StartInfo.Arguments = selPath;
                _ = exp.Start();
            }
        }
        #endregion DataGrid Double Click

        #region Alternate row shading
        private void AltRowShadingOff()
        {
            dgPath.AlternationCount = 0;
            dgPath.RowBackground = new SolidColorBrush(Colors.GhostWhite);
            dgPath.AlternatingRowBackground = new SolidColorBrush(Colors.GhostWhite);
            dgPath.Items.Refresh();
        }

        private void AltRowShadingOn()
        {
            dgPath.AlternationCount = 2;
            dgPath.RowBackground = new SolidColorBrush(Colors.GhostWhite);
            dgPath.AlternatingRowBackground = new SolidColorBrush(Colors.White);
            dgPath.Items.Refresh();
        }
        #endregion Alternate row shading

        #region Copy to clipboard
        private void CopytoClipBoard()
        {
            // Clear the clipboard
            Clipboard.Clear();

            // Include the header row
            dgPath.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;

            // Select all the cells
            dgPath.SelectAllCells();

            // Execute the copy
            ApplicationCommands.Copy.Execute(null, dgPath);

            // De-select the cells
            dgPath.UnselectAllCells();

            // Exclude the header row
            dgPath.ClipboardCopyMode = DataGridClipboardCopyMode.ExcludeHeader;
        }
        #endregion Copy to clipboard

        #region Copy to text file
        private void CopyToTextfile()
        {
            string fname = "PathTools_" + DateTime.Now.Date.ToString("yyyy-MM-dd") + ".txt";
            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Save Details as Text File",
                Filter = "Text File|*.txt",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                FileName = fname
            };
            var result = dialog.ShowDialog();
            if (result == true)
            {
                CopytoClipBoard();
                string gridData = (string)Clipboard.GetData(DataFormats.Text);
                File.WriteAllText(dialog.FileName, gridData, Encoding.UTF8);
            }
        }
        #endregion Copy to text file

        #region Write to temp file
        public static void WriteTempFile(string msg)
        {
            logTemp.Info(msg);
        }
        #endregion Write to temp file

        #region Write to log file
        public static void WriteLogFile(string msg)
        {
            if (UserSettings.Setting.KeepLogFile)
            {
                GlobalDiagnosticsContext.Set("LogPerm", UserSettings.Setting.LogFile);
                if (!File.Exists(UserSettings.Setting.LogFile))
                {
                    logPerm.Info($"This log file was created by {AppInfo.AppName} {AppInfo.TitleVersion}");
                    logPerm.Info("");
                }
                logPerm.Info(msg);
            }
        }
        #endregion Write to log file

        #region Window events
        // Save window position and other settings, write current path to file at shutdown
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            WriteSavedPathFile();
            stopwatch.Stop();
            string line = string.Format("{0} is shutting down.  elapsed time: {1:g}", AppInfo.AppName, stopwatch.Elapsed);
            WriteTempFile(line);
            LogManager.Shutdown();  // Remember to flush
            UserSettings.Setting.WindowLeft = Left;
            UserSettings.Setting.WindowTop = Top;
            UserSettings.SaveSettings();
        }
        #endregion Window events

        #region Menu events

        #region Copy to clipboard
        private void MnuCopyCliboard_Click(object sender, RoutedEventArgs e)
        {
            CopytoClipBoard();
        }
        #endregion Copy to clipboard

        #region Save to Text file
        private void MnuSaveText_Click(object sender, RoutedEventArgs e)
        {
            CopyToTextfile();
        }
        #endregion Save to Text file

        #region View log file
        private void MnuLogFile_Click(object sender, RoutedEventArgs e)
        {
            TextFileViewer.ViewTextFile(UserSettings.Setting.LogFile);
        }
        #endregion

        #region Exit
        private void MnuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion Exit

        #region Refresh
        private void MnuRefresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
        #endregion Refresh

        #region Font Size
        private void FontSmaller_Click(object sender, RoutedEventArgs e)
        {
            FontSmaller();
        }
        private void FontLarger_Click(object sender, RoutedEventArgs e)
        {
            FontLarger();
        }
        private void FontReset_Click(object sender, RoutedEventArgs e)
        {
            FontSizeReset();
        }
        #endregion Font Size

        #region Duplicates
        private void MnuCheckDupes_Click(object sender, RoutedEventArgs e)
        {
            CheckForDupes(GetCurrentPath());
        }
        #endregion Duplicates

        #region Find on path
        private void FindOnPath_Click(object sender, RoutedEventArgs e)
        {
            FindOnPathWindow findOnPath = new FindOnPathWindow()
            {
                Owner = this
            };
            _ = findOnPath.ShowDialog();
        }
        #endregion Find on path

        #region Check path
        private void MnuCheckPath_Click(object sender, RoutedEventArgs e)
        {
            PathCheck checkWindow = new PathCheck
            {
                Owner = this
            };
            checkWindow.Show();
        }
        #endregion Check path

        #region Verify with PowerShell
        private void MnuPowershell_Click(object sender, RoutedEventArgs e)
        {
            // Execute the PowerShell script
            string poshPath = Path.Combine(AppInfo.AppDirectory, "PathCheck.ps1");
            using Process ps = new Process();
            ps.StartInfo.FileName = "powershell.exe";
            ps.StartInfo.Arguments = $"-NoProfile -NoExit -ExecutionPolicy Bypass -File \"{poshPath}\"  ";
            _ = ps.Start();
        }
        #endregion Verify with PowerShell

        #region Check Long Path in Registry
        private void MnuLongPath_Click(object sender, RoutedEventArgs e)
        {
            string regMessage = null;
            try
            {
                using RegistryKey key = Registry.LocalMachine.
                OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\FileSystem");
                string longPath = key.GetValue("LongPathsEnabled", "no value present").ToString();
                if (longPath != null)
                {
                    if (longPath == "1")
                        regMessage = "Long paths ARE enabled.";
                    else
                        regMessage = "Long paths are NOT enabled.";

                    regMessage += "\n\nRegistry location and value:" +
                    "\n\nHKLM\\SYSTEM\\CurrentControlSet\\Control\\FileSystem\\LongPathsEnable = " +
                    longPath;
                }
                else
                {
                    regMessage = "Registry value for LongPathsEnabled not found";
                }
            }
            catch (SecurityException)
            {
                regMessage = "Permission to access registry denied";
            }
            catch (UnauthorizedAccessException)
            {
                regMessage = "No registry rights ";
            }
            finally
            {
                _ = TKMessageBox.Show(regMessage, "Long Paths", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion Check Long Path in Registry

        #region Path Length
        private void MnuPathLen_Click(object sender, RoutedEventArgs e)
        {
            string message = $"The PATH variable is {DisplayPath.TotalPathLength} bytes";
            _ = TKMessageBox.Show(message, "Path Length", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion Path Length

        #region Path in Command Prompt
        private void MnuCmdPrompt_Click(object sender, RoutedEventArgs e)
        {
            const string cmd = @"C:\Windows\System32\cmd.exe";
            const string arguments = "/k Title Current Path && cd /d %userprofile% &&" +
                                     "echo Current Path && echo ------------ && echo. && " +
                                     "echo %PATH% && echo. && echo - or in an easier to read format - " +
                                     "&& echo. && echo %PATH:;=&echo.%";
            _ = Process.Start(cmd, arguments);
        }
        #endregion Path in Command Prompt

        #region Open Environmental Variables
        private void MnuOpenEV_Click(object sender, RoutedEventArgs e)
        {
            const string rundll32 = @"C:\Windows\System32\rundll32.exe";
            const string arguments = "sysdm.cpl,EditEnvironmentVariables";
            _ = Process.Start(rundll32, arguments);
        }
        #endregion Open Environmental Variables

        #region Sub-menu opened
        private void Menu_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            mnuLogFile.IsEnabled = UserSettings.Setting.KeepLogFile;
        }
        #endregion Sub-menu opened

        #region Alternate row shading
        private void MnuShadeAlt_Checked(object sender, RoutedEventArgs e)
        {
            AltRowShadingOn();
        }

        private void MnuShadeAlt_Unchecked(object sender, RoutedEventArgs e)
        {
            AltRowShadingOff();
        }
        #endregion Alternate row shading

        #region Log file setup
        private void MnuLogFileSetup_Click(object sender, RoutedEventArgs e)
        {
            LogFileWindow logFileWindow = new LogFileWindow
            {
                Owner = this
            };
            _ = logFileWindow.ShowDialog();
        }
        #endregion Log file setup

        #region About dialog
        public void MnuAbout_Click(object sender, RoutedEventArgs e)
        {
            About about = new About
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            _ = about.ShowDialog();
        }
        #endregion About dialog

        #region View ReadMe
        private void MnuReadme_Click(object sender, RoutedEventArgs e)
        {
            TextFileViewer.ViewTextFile(@".\ReadMe.txt");
        }
        #endregion View ReadMe

        #region View Temp Log
        private void MnuViewTemp_Click(object sender, RoutedEventArgs e)
        {
            TextFileViewer.ViewTextFile(GetTempLogFile());
        }
        #endregion View Temp Log

        #region View saved path file
        private void MnuViewSavedPath(object sender, RoutedEventArgs e)
        {
            TextFileViewer.ViewTextFile(savedPathFile);
        }
        #endregion

        #endregion Menu events

        #region Context Menu

        #region Open directory in Windows Explorer
        private void CxmExplore_Click(object sender, RoutedEventArgs e)
        {
            if (dgPath.SelectedItem != null)
            {
                DisplayPath row = (DisplayPath)dgPath.SelectedItem;
                string selPath = Path.GetFullPath(row.PathDirectory);
                using Process exp = new Process();
                exp.StartInfo.FileName = "Explorer.exe";
                exp.StartInfo.Arguments = selPath;
                _ = exp.Start();
            }
        }
        #endregion Open directory in Windows Explorer

        #region Open Terminal Here
        private void CxmTerminal_Click(object sender, RoutedEventArgs e)
        {
            if (dgPath.SelectedItem != null)
            {
                DisplayPath row = (DisplayPath)dgPath.SelectedItem;
                string selPath = Path.GetFullPath(row.PathDirectory).TrimEnd('\\');
                using Process wt = new Process();
                wt.StartInfo.FileName = "wt.exe";
                wt.StartInfo.Arguments = $"-d \"{selPath}\" ";
                wt.StartInfo.UseShellExecute = false;
                try
                {
                    _ = wt.Start();
                }
                catch (Win32Exception ex)
                {
                    logTemp.Warn(ex, "Unable to launch Windows Terminal. Falling back to CMD.exe");
                    using Process cmd = new Process();
                    cmd.StartInfo.FileName = "cmd.exe";
                    cmd.StartInfo.Arguments = "/k Title PathTools";
                    cmd.StartInfo.WorkingDirectory = selPath;
                    cmd.StartInfo.UseShellExecute = true;
                    try
                    {
                        _ = cmd.Start();
                    }
                    catch (Exception ex2)
                    {
                        logTemp.Warn(ex2, "Unable to launch CMD.exe");
                        _ = TKMessageBox.Show("Unable to launch a terminal. See log file.",
                                              "PathTools Error",
                                              MessageBoxButton.OK,
                                              MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    _ = TKMessageBox.Show("Unable to launch a terminal. See log file.",
                      "PathTools Error",
                      MessageBoxButton.OK,
                      MessageBoxImage.Error);
                    logTemp.Warn(ex, "Unable to launch Windows Terminal.");
                }
            }
        }
        #endregion Open Terminal Here

        #region Copy Directory Name to Clipboard
        private void CmxCopyCB_Click(object sender, RoutedEventArgs e)
        {
            DisplayPath row = (DisplayPath)dgPath.SelectedItem;
            string selPath = Path.GetFullPath(row.PathDirectory);
            Clipboard.Clear();
            Clipboard.SetText(selPath);
        }
        #endregion Copy Directory Name to Clipboard

        #region File properties
        private void CmxProps_Click(object sender, RoutedEventArgs e)
        {
            if (dgPath.SelectedItem != null)
            {
                DisplayPath row = (DisplayPath)dgPath.SelectedItem;
                string selPath = Path.GetFullPath(row.PathDirectory);
                _ = ShowFileProperties(selPath);
            }
        }
        #endregion File properties

        #endregion Context Menu

        #region Mouse events
        private void ToolboxToggle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            UserSettings.Setting.ShowToolbox = !UserSettings.Setting.ShowToolbox;
        }
        #endregion Mouse events

        #region Key press events
        private void Window_Keydown(object sender, KeyEventArgs e)
        {
            // Number Pad + and - change font size
            if (e.Key == Key.Add)
            {
                if (Keyboard.Modifiers != ModifierKeys.Control)
                    return;
                FontLarger();
            }
            if (e.Key == Key.Subtract)
            {
                if (Keyboard.Modifiers != ModifierKeys.Control)
                    return;
                FontSmaller();
            }
            if (e.Key == Key.NumPad0 && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                FontSizeReset();
            }
            if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                CopytoClipBoard();
            }
            if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                CopyToTextfile();
            }
            if (e.Key == Key.T && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                UserSettings.Setting.ShowToolbox = !UserSettings.Setting.ShowToolbox;
            }
            if (e.Key == Key.F1)
            {
                About about = new About
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                _ = about.ShowDialog();
            }
            if (e.Key == Key.F5)
            {
                Refresh();
            }
        }
        #endregion Key press events

        #region Font size
        // Increase / Decrease font size
        private void FontSmaller()
        {
            double curZoom = UserSettings.Setting.GridZoom;
            if (curZoom > .7)
            {
                curZoom -= .05;
                dgPath.LayoutTransform = new ScaleTransform(curZoom, curZoom);
                buttonPanel.LayoutTransform = new ScaleTransform(curZoom, curZoom);
                headerPanel.LayoutTransform = new ScaleTransform(curZoom, curZoom);
                UserSettings.Setting.GridZoom = Math.Round(curZoom, 2);
            }
        }

        private void FontLarger()
        {
            double curZoom = UserSettings.Setting.GridZoom;
            if (curZoom < 2)
            {
                curZoom += .05;
                dgPath.LayoutTransform = new ScaleTransform(curZoom, curZoom);
                buttonPanel.LayoutTransform = new ScaleTransform(curZoom, curZoom);
                headerPanel.LayoutTransform = new ScaleTransform(curZoom, curZoom);
                UserSettings.Setting.GridZoom = Math.Round(curZoom, 2);
            }
        }

        private void FontSizeReset()
        {
            UserSettings.Setting.GridZoom = 1.0;
            dgPath.LayoutTransform = new ScaleTransform(1, 1);
            buttonPanel.LayoutTransform = new ScaleTransform(1, 1);
            headerPanel.LayoutTransform = new ScaleTransform(1, 1);
        }

        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            double curZoom = UserSettings.Setting.GridZoom;

            if (e.Delta > 0)
            {
                if (curZoom < 2)
                {
                    curZoom += .05;
                    dgPath.LayoutTransform = new ScaleTransform(curZoom, curZoom);
                    buttonPanel.LayoutTransform = new ScaleTransform(curZoom, curZoom);
                    headerPanel.LayoutTransform = new ScaleTransform(curZoom, curZoom);
                }
            }
            else if (e.Delta < 0)
            {
                if (curZoom > .7)
                {
                    curZoom -= .05;
                    dgPath.LayoutTransform = new ScaleTransform(curZoom, curZoom);
                    buttonPanel.LayoutTransform = new ScaleTransform(curZoom, curZoom);
                    headerPanel.LayoutTransform = new ScaleTransform(curZoom, curZoom);
                }
            }

            UserSettings.Setting.GridZoom = Math.Round(curZoom, 2);
        }
        #endregion Font size

        #region First run log file setup
        private void FirstRunSetup()
        {
            if (UserSettings.Setting.FirstRun)
            {
                Visibility = Visibility.Visible;

                MessageBoxResult result = TKMessageBox.Show("It looks like this is the first run of PathTools.\n\n" +
                                                            "Would you like to keep a log file?",
                                                            "PathTools - Keep a Log File?",
                                                            MessageBoxButton.YesNoCancel,
                                                            MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    LogFileWindow log = new LogFileWindow
                    {
                        Owner = this
                    };
                    log.cbKeepLog.IsChecked = true;
                    _ = log.ShowDialog();
                }
                else if (result == MessageBoxResult.No)
                {
                    UserSettings.Setting.KeepLogFile = false;
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    Application.Current.Shutdown();
                }

                UserSettings.Setting.FirstRun = false;
            }
        }
        #endregion

        #region Write saved path to file
        private void WriteSavedPathFile()
        {
            // Write the current path to disk for next time
            try
            {
                JsonSerializerOptions opts = new JsonSerializerOptions
                {
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(GetCurrentPath(), opts);
                File.WriteAllText(savedPathFile, json);
            }
            catch (Exception)
            {
                TKMessageBox.Show("Error saving file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Open file properties dialog
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

        public static bool ShowFileProperties(string Filename)
        {
            SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
            info.cbSize = Marshal.SizeOf(info);
            info.lpVerb = "properties";
            info.lpFile = Filename;
            info.nShow = SW_SHOW;
            info.fMask = SEE_MASK_INVOKEIDLIST;
            return ShellExecuteEx(ref info);
        }
        #endregion Open file properties dialog

        #region Setting change
        private void UserSettingChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyInfo prop = sender.GetType().GetProperty(e.PropertyName);
            var newValue = prop?.GetValue(sender, null);
            switch (e.PropertyName)
            {
                case "ShadeAltRows":
                    if ((bool)newValue)
                    {
                        AltRowShadingOn();
                    }
                    else
                    {
                        AltRowShadingOff();
                    }
                    break;

                case "KeepOnTop":
                    Topmost = (bool)newValue;
                    break;

                case "ShowGridLines":
                    if ((bool)newValue)
                    {
                        dgPath.GridLinesVisibility = DataGridGridLinesVisibility.All;
                    }
                    else
                    {
                        dgPath.GridLinesVisibility = DataGridGridLinesVisibility.None;
                    }
                    break;

                case "ShowToolbox":
                    if ((bool)newValue)
                    {
                        buttonPanel.Visibility = Visibility.Visible;
                        headerPanel.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        buttonPanel.Visibility = Visibility.Collapsed;
                        headerPanel.Visibility = Visibility.Visible;
                    }
                    break;
            }
            logTemp.Debug($"***Setting change: {e.PropertyName} New Value: {newValue}");
        }
        #endregion Setting change

        #region Unhandled Exception Handler
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            logTemp.Error("Unhandled Exception");
            Exception e = (Exception)args.ExceptionObject;
            logTemp.Error(e.Message);
            if (e.InnerException != null)
            {
                logTemp.Error(e.InnerException.ToString());
            }
            logTemp.Error(e.StackTrace);

            _ = TKMessageBox.Show($"An unexpected error has occurred. \n{e.Message}\nSee the log file for more information.",
                                  "PathTools Error",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
        }
        #endregion Unhandled Exception Handler

        #region Get temp file name
        public static string GetTempLogFile()
        {
            // Ask NLog what the file name is
            FileTarget target = LogManager.Configuration.FindTargetByName("logTemp") as FileTarget;
            var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
            return target.FileName.Render(logEventInfo);
        }
        #endregion
    }
}
