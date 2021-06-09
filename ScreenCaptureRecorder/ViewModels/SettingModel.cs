using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenCaptureRecorder.ViewModels
{
    class SettingModel:Way.Lib.DataModel
    {

        private DirectShowDeviceInfo _DeviceInfo;
        public DirectShowDeviceInfo DeviceInfo
        {
            get => _DeviceInfo;
            set
            {
                if (_DeviceInfo != value)
                {
                    _DeviceInfo = value;
                    this.OnPropertyChanged("DeviceInfo", null, null);
                }
            }
        }


        private DeviceInfo[] _HardEncoders;
        public DeviceInfo[] HardEncoders
        {
            get => _HardEncoders;
            set
            {
                if (_HardEncoders != value)
                {
                    _HardEncoders = value;
                    this.OnPropertyChanged("HardEncoders", null, null);
                }
            }
        }


        public string SelectedAudio
        {
            get => _config.AudioInput;
            set
            {
                if (_config.AudioInput != value)
                {
                    _config.AudioInput = value;
                    this.OnPropertyChanged("SelectedAudio", null, null);
                }
            }
        }


        public string SelectedHardware
        {
            get => _config.HardwareEngine;
            set
            {
                if (_config.HardwareEngine != value)
                {
                    _config.HardwareEngine = value;
                    this.OnPropertyChanged("SelectedHardware", null, null);
                }
            }
        }


        public string SaveFolder
        {
            get => _config.SaveFolder;
            set
            {
                if (_config.SaveFolder != value)
                {
                    _config.SaveFolder = value;
                    this.OnPropertyChanged("SaveFolder", null, null);
                }
            }
        }

        Setting _window;
        Config _config;
        public SettingModel(Setting window)
        {
            _window = window;
            _DeviceInfo = FfmpegHelper.GetDirectShowDevices();
            _DeviceInfo.Audios.Insert(0, new ScreenCaptureRecorder.DeviceInfo { 
                Name = "",
            AlternativeName = ""
            });
            _HardEncoders = FfmpegHelper.GetHardwareEncoders();
            _config = Config.GetInstance();
        }

        public void SelectSaveFolder()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if(result == CommonFileDialogResult.Ok)
            {
                this.SaveFolder = dialog.FileName;
            }
        }

        public void OK()
        {
            if (string.IsNullOrEmpty(SelectedHardware))
            {
                MessageBox.Show("please select hardware");
                return;
            }
            if (string.IsNullOrEmpty(this.SaveFolder))
            {
                MessageBox.Show("please set save folder");
                return;
            }

            _config.Save();

            _window.DialogResult = true;
            _window.Close();
        }
    }
}
