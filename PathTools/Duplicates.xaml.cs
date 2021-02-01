// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using System.Windows;
using System.Windows.Media;

namespace PathTools
{
    public partial class Duplicates : Window
    {
        public Duplicates()
        {
            InitializeComponent();

            // Datagrid font size
            double curZoom = UserSettings.Setting.GridZoom;
            dgDupes.LayoutTransform = new ScaleTransform(curZoom, curZoom);
        }
    }
}
