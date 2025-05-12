using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Deep_Learning_Demo.Classes
{
    [XmlType("Config")]
    public class csConfigModel
    {
        public string ServerUrl { get; set; } = "http://10.1.2.202:8000";

        public csDeepLearningAPISettings APISettings { get; set; } = new csDeepLearningAPISettings();

        public csConfigModel()
        {

        }
 
    }
}
