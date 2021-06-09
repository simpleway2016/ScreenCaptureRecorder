using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace ScreenCaptureRecorder.ViewModels
{
    class MainWindowModel : Way.Lib.DataModel
    {
        MainWindow _mainWindow;


        private Status _CurrentStatus;
        public Status CurrentStatus
        {
            get => _CurrentStatus;
            set
            {
                if (_CurrentStatus != value)
                {
                    _CurrentStatus = value;
                    this.OnPropertyChanged("CurrentStatus", null, null);
                }
            }
        }
        KeyboardListener KeyboardListener;
        public MainWindowModel(MainWindow mainWindow)
        {
            this._mainWindow = mainWindow;
            KeyboardListener = new KeyboardListener();
            KeyboardListener.KeyUp += KeyboardListener_KeyUp;
        }

        private void KeyboardListener_KeyUp(object sender, RawKeyEventArgs args)
        {
            //System.Diagnostics.Debug.WriteLine("up:" + args.Key.ToString());
            if (args.Key == System.Windows.Input.Key.D8)
            {               
                if(Keyboard.Modifiers == ModifierKeys.Alt)
                {
                    this.Start();
                }
            }
            else if (args.Key == System.Windows.Input.Key.D9)
            {
                if (Keyboard.Modifiers == ModifierKeys.Alt)
                {
                    this.Stop();
                }
            }
            else if (args.Key == System.Windows.Input.Key.D0)
            {
                if (Keyboard.Modifiers == ModifierKeys.Alt)
                {
                    this.DeleteThisOne();
                }
            }
        }

        int _currentFileIndex = 0;
        string _currentFilePath = null;
        List<string> _fileList = new List<string>();
        Config _currentConfig;
        public void Start()
        {
            try
            {
                _mainWindow.Hide();
                this.CurrentStatus = Status.ReadyToRun;
                var config = _currentConfig =Config.GetInstance();
                _currentFileIndex++;
                _currentFilePath = $"{config.SaveFolder}\\output{_currentFileIndex}.ts";
                _fileList.Add(_currentFilePath);

                FfmpegHelper.StartCapture(config, _currentFilePath, text=> {

                    if (text.Contains("error"))
                    {
                        _mainWindow.Dispatcher.Invoke(() =>
                        {
                            Paragraph p = new Paragraph(new Run(text));
                            _mainWindow.txtInfo.Document.Blocks.Add(p);
                            _mainWindow.txtInfo.ScrollToEnd();
                        });
                    }
                    else if(text.StartsWith("frame="))
                    {
                        //running
                        this.CurrentStatus = Status.Running;
                    }
                    else if (text.Contains("video:"))
                    {
                        this.CurrentStatus = Status.Stop;
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
        public void Stop()
        {
            FfmpegHelper.StopCapture();
        }

        public void Merge()
        {
            if (_fileList.Count == 0)
                return;

            var targetpath = $"{_currentConfig.SaveFolder}\\output.mp4";
            if (File.Exists(targetpath))
            {
                try
                {
                    File.Delete(targetpath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
              
            }

            StringBuilder filedesc = new StringBuilder();
            foreach (var path in _fileList)
            {
                if (filedesc.Length > 0)
                    filedesc.Append('|');
                filedesc.Append($"{path}");
            }

            var arg = $"-i \"concat:{filedesc}\" -c copy \"{targetpath}\"";
            Way.Lib.Runner.Exec("./ffmpeg.exe", arg);
            if(File.Exists(targetpath))
            {
                MessageBox.Show("合并成功");
            }
            else
            {
                File.WriteAllText($"{_currentConfig.SaveFolder}\\output.log", arg, Encoding.UTF8);
                MessageBox.Show("合并失败，请查看目标目录的output.log");
            }
        }
        public void DeleteThisOne()
        {
            if(this.CurrentStatus != Status.Stop)
            {
                MessageBox.Show("Stop first");
                return;
            }
            if (_fileList.Count == 0)
                return;

            if (MessageBox.Show("确定删除最后这个片段吗？", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {

                try
                {
                    File.Delete(_fileList.Last());
                    _fileList.RemoveAt(_fileList.Count - 1);
                    _currentFileIndex = _fileList.Count;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }

    enum Status
    {
        Stop = 0,
        ReadyToRun = 1,
        Running = 2,
        Pause = 3
    }
}
