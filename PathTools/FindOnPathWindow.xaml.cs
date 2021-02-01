// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

#region Using directives
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
#endregion Using directives

namespace PathTools
{
    public partial class FindOnPathWindow : Window
    {
        public FindOnPathWindow()
        {
            InitializeComponent();
            SetZoom();
        }

        #region Set grid size
        private void SetZoom()
        {
            double curZoom = UserSettings.Setting.GridZoom;
            GriddyMcGridFace.LayoutTransform = new ScaleTransform(curZoom, curZoom);
        }
        #endregion Set grid size

        #region Search PATH for the file
        private static string[] FindOnPath(string fn)
        {
            return Array.FindAll(Environment.GetEnvironmentVariable("PATH")
                                 .Split(';'), s => File.Exists(Path.Combine(s, fn)));
        }
        #endregion Search PATH for the file

        #region Button events
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnFind_Click(object sender, RoutedEventArgs e)
        {
            FindIt();
        }
        #endregion Button events

        #region Find then format message
        private void FindIt()
        {
            if (string.IsNullOrWhiteSpace(txtFind.Text))
            {
                tbResult.Text = "Enter a file name in the box above";
                return;
            }

            string[] foundDir = FindOnPath(txtFind.Text);

            if (foundDir.Length > 1)
            {
                StringBuilder sb = new StringBuilder();
                _ = sb.Append(txtFind.Text).AppendLine(" was found in multiple directories in the PATH:\n");
                foreach (string dir in foundDir)
                {
                    _ = sb.Append('\t').AppendLine(dir);
                }
                tbResult.Text = sb.ToString();
                if (chkClipboard.IsChecked == true)
                {
                    Clipboard.Clear();
                    Clipboard.SetText(foundDir[0]);
                }
            }
            else if (foundDir.Length == 1)
            {
                tbResult.Text = foundDir.Length < 30 ?
                    $"{txtFind.Text} was found in {foundDir[0]}" : $"Found in {foundDir[0]}";
                if (chkClipboard.IsChecked == true)
                {
                    Clipboard.Clear();
                    Clipboard.SetText(foundDir[0]);
                }
            }
            else
            {
                tbResult.Text = $"{txtFind.Text} was not found in the PATH";
            }
        }
        #endregion Find then format message

        #region Enter or Tab will trigger FindIt
        private void TxtFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                FindIt();
            }
        }
        #endregion Enter or Tab will trigger FindIt

        #region Set focus to TextBox
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtFind.Focus();
        }
        #endregion Set focus to TextBox

        #region Move/Drag the window
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        #endregion Move the window
    }
}
