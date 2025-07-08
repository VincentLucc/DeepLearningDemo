using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Deep_Learning_Demo.Classes
{
    [XmlType("Config")]
    public class csConfigModel
    {
        public _workMode WorkMode { get; set; } = _workMode.API;

        /// <summary>
        /// API Mode
        /// </summary>
        public string ServerUrl { get; set; } = "http://10.1.2.202:8000";

        /// <summary>
        /// Local script
        /// </summary>
        public string PythonHome { get; set; } = "C:\\Program Files\\Python312";

        /// <summary>
        /// Local script
        /// </summary>
        public string ScriptFile { get; set; } = @"E:\Backup\Companies\PackSmart\Projects\DeepLearning\DeepLearning.Client.Git\Deep Learning Demo\PythonScripts\model_runner_parallel.py";


        public bool FakeResponse { get; set; }

        public csDeepLearningAPISettings APISettings { get; set; } = new csDeepLearningAPISettings();

        public csConfigModel()
        {

        }

    }

    public enum _workMode
    {
        [XmlEnum("0"), Display(Name = "API")]
        API = 0,
        [XmlEnum("1"), Display(Name = "Python Script")]
        PyhtonScript = 1,
    }
}
