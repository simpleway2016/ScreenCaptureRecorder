using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScreenCaptureRecorder
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewModels.MainWindowModel Model;
        System.Windows.Forms.NotifyIcon _notifyIcon;
        System.Drawing.Icon _runningIcon;
        System.Drawing.Icon _normalIcon;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this.Model = new ViewModels.MainWindowModel(this);

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/ScreenCaptureRecorder;component/icon.ico")).Stream;
            _runningIcon = new System.Drawing.Icon(iconStream);

            iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/ScreenCaptureRecorder;component/icon2.ico")).Stream;
            _normalIcon = new System.Drawing.Icon(iconStream);
            _notifyIcon.Icon = _normalIcon;
            _notifyIcon.Visible = true;
            _notifyIcon.Click += _notifyIcon_Click;
            this.Model.PropertyChanged += Model_PropertyChanged;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _notifyIcon.Dispose();
            base.OnClosing(e);
        }

        private void _notifyIcon_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
           if(e.PropertyName == nameof(this.Model.CurrentStatus))
            {
                if(this.Model.CurrentStatus == ViewModels.Status.Running)
                {
                    _notifyIcon.Icon = _runningIcon;
                }
                else
                {
                    _notifyIcon.Icon = _normalIcon;
                }
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            this.Model.Start();
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            new Setting() { 
                Owner = this
            }.ShowDialog();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            this.Model.Stop();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            this.Model.DeleteThisOne();
        }

        private void Merge_Click(object sender, RoutedEventArgs e)
        {
            this.Model.Merge();
        }

        private void ResetHook_Click(object sender, RoutedEventArgs e)
        {
            this.Model.ResetHook();
        }
    }
}
