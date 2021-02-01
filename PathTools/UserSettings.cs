// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using TKUtils;

namespace PathTools
{
    public class UserSettings : SettingsManager<UserSettings>, INotifyPropertyChanged
    {
        #region Constructor
        public UserSettings()
        {
            // Set defaults
            CaseSensitive = false;
            FirstRun = true;
            GridZoom = 1;
            KeepLogFile = false;
            KeepOnTop = false;
            ShadeAltRows = true;
            ShowGridLines = true;
            ShowRightStatus = true;
            ShowToolbox = true;
            Sound = false;
            ViewerBG = "#FFF0FFFF";
            WindowLeft = 100;
            WindowTop = 100;
        }
        #endregion Constructor

        #region Properties
        public bool CaseSensitive
        {
            get => caseSensitive;
            set
            {
                caseSensitive = value;
                OnPropertyChanged();
            }
        }

        public bool FirstRun
        {
            get => firstRun;
            set
            {
                firstRun = value;
                OnPropertyChanged();
            }
        }

        public double GridZoom
        {
            get
            {
                if (gridZoom <= 0)
                {
                    gridZoom = 1;
                }
                return gridZoom;
            }
            set
            {
                gridZoom = value;
                OnPropertyChanged();
            }
        }

        public bool KeepLogFile
        {
            get => keepLogFile;
            set
            {
                keepLogFile = value;
                OnPropertyChanged();
            }
        }

        public bool KeepOnTop
        {
            get => keepOnTop;
            set
            {
                keepOnTop = value;
                OnPropertyChanged();
            }
        }

        public string LogFile
        {
            get => logFile;
            set
            {
                logFile = value;
                OnPropertyChanged();
            }
        }

        public bool ShadeAltRows
        {
            get => shadeAltRows;
            set
            {
                shadeAltRows = value;
                OnPropertyChanged();
            }
        }

        public bool ShowGridLines
        {
            get => showGridlines;
            set
            {
                showGridlines = value;
                OnPropertyChanged();
            }
        }

        public bool ShowRightStatus
        {
            get => showRightStatus;
            set
            {
                showRightStatus = value;
                OnPropertyChanged();
            }
        }

        public bool ShowToolbox
        {
            get => showToolbox;
            set
            {
                showToolbox = value;
                OnPropertyChanged();
            }
        }

        public bool Sound
        {
            get => sound;
            set
            {
                sound = value;
                OnPropertyChanged();
            }
        }

        public string ViewerBG
        {
            get => viewerBG;
            set
            {
                viewerBG = value;
                OnPropertyChanged();
            }
        }

        public double WindowLeft
        {
            get
            {
                if (windowLeft < 0)
                {
                    windowLeft = 0;
                }
                return windowLeft;
            }
            set => windowLeft = value;
        }

        public double WindowTop
        {
            get
            {
                if (windowTop < 0)
                {
                    windowTop = 0;
                }
                return windowTop;
            }
            set => windowTop = value;
        }
        #endregion Properties

        #region Private backing fields
        private bool caseSensitive;
        private bool firstRun;
        private double gridZoom;
        private bool keepLogFile;
        private bool keepOnTop;
        private string logFile;
        private bool shadeAltRows;
        private bool showGridlines;
        private bool showRightStatus;
        private bool showToolbox;
        private bool sound;
        private string viewerBG;
        private double windowLeft;
        private double windowTop;
        #endregion Private backing fields

        #region Handle property change event
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion Handle property change event
    }
}
