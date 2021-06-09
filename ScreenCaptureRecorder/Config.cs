using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Way.Lib;

namespace ScreenCaptureRecorder
{
    class Config
    {
        public string AudioInput { get; set; }
        public string HardwareEngine { get; set; }
        public string SaveFolder { get; set; }

        public static Config GetInstance()
        {


            if(File.Exists("./config.json"))
            {
                return  File.ReadAllText("./config.json", Encoding.UTF8).FromJson<Config>();
            }
            return  new Config();
        }

        public void Save()
        {
            File.WriteAllText("./config.json", this.ToJsonString(true), Encoding.UTF8);
        }
    }
}
