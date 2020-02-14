using GitWitWpf.Models;
using GitWitWpf.Services;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static GitWitWpf.Models.SettingsModel;

namespace GitWitWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private static readonly uint WM_WINDOWPOSCHANGING = 0x0046;
        private static readonly uint SWP_NOZORDER = 0x0004;
        private static readonly int INIT_WIDTH = 200;
        private static readonly int INIT_HEIGHT = 275;
        private static readonly int MIN_WIDTH = 140;
        private static readonly ScreenPosition DEFAULT_WINDOW_POSITION = ScreenPosition.TopRight;
        private SettingsModel _settingsModel;
        private CommitCalendarModel _commitCalendarModel;

        public MainWindow()
        {
            this.DataContext = this;
            _settingsModel = new SettingsModel();
            _commitCalendarModel = new CommitCalendarModel(_settingsModel);
            InitializeComponent();
        }

        public CommitCalendarModel CommitCalendar
        {
            get { return _commitCalendarModel;  }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;

            if (hwndSource != null)
                hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(KeepWindowBack);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _settingsModel.PropertyChanged += this.OnSettingsChanged;
            await _settingsModel.Init();
            CustomWidth = GetWindowWidth();
            SystemEvents.DisplaySettingsChanged += new
            EventHandler(this.OnDisplayChange);
            _commitCalendarModel.Init();
        }

        private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            //If WindowPosition changed, reset windowposition
            if (e.PropertyName == "WindowPosition")
            {
                this.SetWindowPosition();
            } else if (e.PropertyName == "NumWeeks")
            {
                CustomWidth = GetWindowWidth();
            }
        }

        private int GetWindowWidth()
        {
            int width = (int)(30 * _settingsModel.NumWeeks + 40);
            if (width < MIN_WIDTH)
            {
                return MIN_WIDTH;
            }
            return width;
        }

        private int _customWidth = INIT_WIDTH;
        public int CustomWidth
        {
            get { return _customWidth; }
            set
            {
                if (value != _customWidth)
                {
                    _customWidth = value;
                    OnPropertyChanged("CustomWidth");
                    //window position must be adjusted for chaning size
                    this.SetWindowPosition();
                }
            }
        }

        private int _customHeight = INIT_HEIGHT;
        public int CustomHeight
        {
            get { return _customHeight; }
            set
            {
                if (value != _customHeight)
                {
                    _customHeight = value;
                    OnPropertyChanged("CustomHeight");
                    //window position must be adjusted for changing size
                    this.SetWindowPosition();
                }
            }
        }

        public SettingsModel Settings
        {
            get { return _settingsModel; }
        }

        private void OnDisplayChange(object sender, EventArgs e)
        {
            this.SetWindowPosition();
        }

        private void SetWindowPosition()
        {
            ScreenPosition windowPosition = (_settingsModel.WindowPosition != null) ? (ScreenPosition)_settingsModel.WindowPosition : DEFAULT_WINDOW_POSITION;
            int screenWidth = (int)SystemParameters.WorkArea.Width;
            int screenHeight = (int)SystemParameters.WorkArea.Height;
            this.Top = (
                windowPosition == ScreenPosition.BottomLeft 
                || windowPosition == ScreenPosition.BottomRight)? 
                screenHeight - CustomHeight: 0;
            this.Left = (
                windowPosition == ScreenPosition.TopRight
                || windowPosition == ScreenPosition.BottomRight)?
                screenWidth - CustomWidth: 0;
        }

        //Hook to WndProc that ensures the Z position of the window is 0, so clicking it will not hide other windows
        private IntPtr KeepWindowBack(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_WINDOWPOSCHANGING)
            {
                WINDOWPOS mwp = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));
                mwp.flags |= SWP_NOZORDER;

                Marshal.StructureToPtr(mwp, lParam, true);
            }
            return lParam;
        }

        //Allow binding to CustomWidth & CustomHeight properties
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.DataContext = _settingsModel;
            settingsWindow.ShowDialog();
        }
    }
}
