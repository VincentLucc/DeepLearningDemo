using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deep_Learning_Demo.Classes
{



    public class csDeepLearningAPIRequest
    {
        public string CommandID { get; set; } = csDateTimeHelper.DateTime_fff;

        /// <summary>
        /// Timeout in ms
        /// </summary>
        public int Timeout { get; set; } = 5000;

        public List<csModelSettings> Models { get; set; } = new List<csModelSettings>();

        
    }

 

    public class csModelSettings
    {
        public int ModelIndex { get; set; } = 1;

        public int Threadhold { get; set; } = 20;

    }
}
