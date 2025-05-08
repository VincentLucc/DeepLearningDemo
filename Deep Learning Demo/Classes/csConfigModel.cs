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

        public List<csModelSettings> Models { get; set; } = new List<csModelSettings>();

        public csConfigModel()
        {

        }

        public void InitData()
        {
            Models.Add(new csModelSettings()
            {
                ModelIndex = 1,
                Threadhold = 20,
            });
            Models.Add(new csModelSettings()
            {
                ModelIndex = 2,
                Threadhold = 20,
            });
            Models.Add(new csModelSettings()
            {
                ModelIndex = 3,
                Threadhold = 20,
            });
        }
    }
}
