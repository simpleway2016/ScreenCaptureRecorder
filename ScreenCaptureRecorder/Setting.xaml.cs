using ScreenCaptureRecorder.ViewModels;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace ScreenCaptureRecorder
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Setting : Window
    {
        SettingModel Model;
        public Setting()
        {
            InitializeComponent();
            this.DataContext = Model = new SettingModel(this);
        }
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.Model.OK();
        }

        private void SaveFolder_click(object sender, RoutedEventArgs e)
        {
            this.Model.SelectSaveFolder();
        }
    }
}
