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
        public string ServerUrl
        { get; set; } = "http://10.1.2.202:8000";



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
