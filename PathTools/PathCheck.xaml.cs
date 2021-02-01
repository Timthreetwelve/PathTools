// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

#region Using directives
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
#endregion Using directives

namespace PathTools
{
    public partial class PathCheck : Window
    {
        public PathCheck()
        {
            InitializeComponent();

            SetZoomLevel();

            CheckPath();
        }

        private void SetZoomLevel()
        {
            double curZoom = UserSettings.Setting.GridZoom;
            listBox1.LayoutTransform = new ScaleTransform(curZoom, curZoom);
        }

        private void CheckPath()
        {
            string enviromentPath = Environment.GetEnvironmentVariable("PATH");
            if (enviromentPath != null)
            {
                int longest = 0;
                foreach (string item in enviromentPath.Split(';'))
                {
                    if (item.Length > longest)
                    {
                        longest = item.Length;
                    }
                }

                int counter = 0;
                foreach (string pathPart in enviromentPath.Split(';'))
                {
                    if (!string.IsNullOrEmpty(pathPart))
                    {
                        counter++;
                        string msg;
                        if (Directory.Exists(pathPart))
                        {
                            msg = string.Format("{0,3} {1, -" + longest + "}  <- Valid", counter, pathPart);
                        }
                        else
                        {
                            msg = string.Format("{0,3} {1, -" + longest + "}  <- Not Found", counter, pathPart);
                        }
                        listBox1.Items.Add(msg);
                    }
                }
            }
        }

        // Close window when ESC is pressed
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
