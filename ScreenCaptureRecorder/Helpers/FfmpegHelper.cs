using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenCaptureRecorder
{
    // -f gdigrab -framerate 20 -i desktop -f dshow -i audio="@devic5" -rtbufsize 160M -c:v h264_amf -qp 0 output.mp4
    //例如intel集成显卡是“-c:v h264_qsv”，n卡“-c:v h264_nvenc”，a卡“-c:v h264_amf”


    /// <summary>
    /// https://trac.ffmpeg.org/wiki/Capture/Desktop
    /// </summary>
    class FfmpegHelper
    {

        public static DeviceInfo[] GetHardwareEncoders()
        {
            return new DeviceInfo[] { 
                new DeviceInfo{ 
                    Name = "英特尔",
                    AlternativeName = "h264_qsv"
                },
                new DeviceInfo{
                    Name = "AMD",
                    AlternativeName = "h264_amf"
                },
                 new DeviceInfo{
                    Name = "英伟达显卡",
                    AlternativeName = "h264_nvenc"
                },
            };
        }

        static Process CurrentCapturingProcess;
        public static void StartCapture(Config config,string savePath,Action<string> outputInfoHandler)
        {

            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

           if(Directory.Exists( Path.GetDirectoryName(savePath) ) == false)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            }

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo("./ffmpeg.exe");
            string audioArg = "";
            if(!string.IsNullOrEmpty(config.AudioInput))
            {
                audioArg = $"-f dshow -i audio=\"{config.AudioInput}\" ";
            }
            startInfo.Arguments = $"-f gdigrab -rtbufsize 160M -framerate 20 -i desktop {audioArg}-c:v libx264rgb -crf 0 -preset ultrafast \"{savePath}\"";
            //startInfo.Arguments = $"-thread_queue_size {Environment.ProcessorCount/2} -f dshow -framerate 20 -i video=\"screen-capture-recorder\" -f dshow -i audio=\"{config.AudioInput}\" -rtbufsize 160M -c:v libx264rgb -crf 0 -preset ultrafast \"{savePath}\"";
            //-thread_queue_size 8 -f dshow -framerate 20 -i video="screen-capture-recorder"
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            var process = new System.Diagnostics.Process();
            process.StartInfo = startInfo;

            process.Start();
            process.ErrorDataReceived += (s, e) => {
                if (e.Data == null)
                    return;

                string text = e.Data;
                outputInfoHandler?.Invoke(text);
            };
            process.BeginErrorReadLine();
            CurrentCapturingProcess = process;
        }

        public static void StopCapture()
        {
            if(CurrentCapturingProcess!= null)
            {
                CurrentCapturingProcess.StandardInput.Write('q');
                CurrentCapturingProcess = null;
            }
        }

        public static DirectShowDeviceInfo GetDirectShowDevices()
        {
            DirectShowDeviceInfo info = new DirectShowDeviceInfo();
           var process = Way.Lib.Runner.OpenProcess("./ffmpeg.exe", "-list_devices true -f dshow -i dummy");
            process.Start();
            process.WaitForExit();

            while(true)
            {
                var line = process.StandardError.ReadLine();
                if (line == null)
                    break;

                if(line.Contains("DirectShow video devices"))
                {
                    GetVideoDevices(process.StandardError , info);
                    break;
                }
            }

            var ret = process.StandardOutput.ReadToEnd();
            ret = process.StandardError.ReadToEnd();
            return info;
        }

        static void GetVideoDevices(StreamReader reader, DirectShowDeviceInfo info)
        {
            while (true)
            {
                var name = reader.ReadLine();
                if (name == null)
                    break;
                if (name.StartsWith("dummy:") || name.Contains("DirectShow audio devices"))
                {
                    GetAudioDevices(reader, info);
                    break;
                }
                var alternativeName = reader.ReadLine();
                if (alternativeName == null)
                    break;

                var m = Regex.Match(name, @"\""(?<n>.+)\""");
                name = m.Groups["n"].Value;

                m = Regex.Match(alternativeName, @"\""(?<n>.+)\""");
                alternativeName = m.Groups["n"].Value;

                info.Videos.Add(new DeviceInfo() { 
                Name = name,
                AlternativeName = alternativeName
                });
            }
        }

        static void GetAudioDevices(StreamReader reader, DirectShowDeviceInfo info)
        {
            while (true)
            {
                var name = reader.ReadLine();
                if (name == null)
                    break;
                if (name.StartsWith("dummy:") || name.Contains("DirectShow video devices"))
                    break;
                var alternativeName = reader.ReadLine();
                if (alternativeName == null)
                    break;

                var m = Regex.Match(name, @"\""(?<n>.+)\""");
                name = m.Groups["n"].Value;

                m = Regex.Match(alternativeName, @"\""(?<n>.+)\""");
                alternativeName = m.Groups["n"].Value;

                info.Audios.Add(new DeviceInfo()
                {
                    Name = name,
                    AlternativeName = alternativeName
                });
            }
        }
    }

    class DirectShowDeviceInfo
    {
        public List<DeviceInfo> Videos { get; set; }
        public List<DeviceInfo> Audios { get; set; }
        public DirectShowDeviceInfo()
        {
            this.Videos = new List<DeviceInfo>();
            this.Audios = new List<DeviceInfo>();
        }
    }

    class DeviceInfo
    {
        public string Name { get; set; }
        public string AlternativeName { get; set; }
    }
}
