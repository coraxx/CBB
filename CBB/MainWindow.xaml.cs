using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
//using System.Diagnostics;
//using CBB.Properties;

namespace CBB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private readonly DispatcherTimer _cursorTimer = new DispatcherTimer();
        private readonly TimeSpan _timeoutToHide = TimeSpan.FromSeconds(2);
        private DateTime _lastMouseMove;
        private bool _isHidden;


        public MainWindow()
        {
            InitializeComponent();
            _cursorTimer.Interval = TimeSpan.FromSeconds(1);
            _cursorTimer.IsEnabled = true;
            _cursorTimer.Tick += CursorTimer_Tick;
            MouseMove += MainWindow_OnMouseMove;

            //if (Debugger.IsAttached)
            //    Settings.Default.Reset();
        }

        #region OnPropertyChangedEvent

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Fullscreen mode handling

        private bool _textboxEnabled = true;
        public bool TextboxEnabled
        {
            get => _textboxEnabled;
            set
            {
                _textboxEnabled = value;
                OnPropertyChanged("TextboxEnabled");
            }
        }

        private bool _fullscreenEnabled;
        public bool FullscreenEnabled
        {
            get => _fullscreenEnabled;
            set
            {
                _fullscreenEnabled = value;
                TextboxEnabled = !value;
                OnPropertyChanged("FullscreenEnabled");
            }
        }

        private void ButtonFullscreenOn_OnClick(object sender, RoutedEventArgs e)
        {
            FullScreen(true);
        }

        private void FullScreen(bool enabled)
        {
            if (enabled)
            {
                FullscreenEnabled = true;
                BulletinBoardOverlayGrid.Visibility = Visibility.Visible;
                // first collapse windowo to cover the taskbar
                RootWindow.Visibility = Visibility.Collapsed;
                RootWindow.WindowStyle = WindowStyle.None;
                RootWindow.ResizeMode = ResizeMode.NoResize;
                RootWindow.WindowState = WindowState.Maximized;
                RootWindow.Topmost = true;
                RootWindow.Visibility = Visibility.Visible;
            }
            else
            {
                FullscreenEnabled = false;
                BulletinBoardOverlayGrid.Visibility = Visibility.Hidden;
                RootWindow.WindowStyle = WindowStyle.None;
                RootWindow.ResizeMode = ResizeMode.CanResize;
                RootWindow.Topmost = false;
                RootWindow.WindowState = WindowState.Normal;
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) FullScreen(!FullscreenEnabled);
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                FullScreen(false);
            }
        }

        #endregion

        #region Basic Buttons and windows handling
        // Rectangle to move borderless window
        private void rectangle1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        // Close Button
        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Minimize Button
        private void buttonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // Maximize and restore to normal Button
        private void buttonMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowMaximized)
            {
                WindowState = WindowState.Normal;
                WindowMaximized = false;
            }
            else
            {
                WindowState = WindowState.Maximized;
                WindowMaximized = true;
            }
        }


        // init value for window status. maybe read window status directly in the future
        private bool _windowMaximized;
        // boolean: _windowMaximized - trigger porperty change if bool is changed
        public bool WindowMaximized
        {
            get => _windowMaximized;
            set
            {
                _windowMaximized = value;
                OnPropertyChanged("WindowMaximized");
            }
        }
        #endregion

        #region Hide cursor in fullscreen mode

        private void CursorTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan elaped = DateTime.Now - _lastMouseMove;
            if (elaped >= _timeoutToHide && !_isHidden && FullscreenEnabled)
            {
                Cursor = Cursors.None;
                _isHidden = true;
            }
        }

        private void MainWindow_OnMouseMove(object sender, MouseEventArgs e)
        {
            _lastMouseMove = DateTime.Now;

            if (_isHidden)
            {
                Cursor = Cursors.Arrow;
                _isHidden = false;
            }
        }

        #endregion

        protected override void OnClosing(CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            base.OnClosing(e);
        }
    }
}
