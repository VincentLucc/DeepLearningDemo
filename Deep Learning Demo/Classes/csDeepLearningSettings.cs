using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deep_Learning_Demo.Classes
{



    public class csDeepLearningRequest
    {
        public long CommandID;
        /// <summary>
        /// Timeout in ms
        /// </summary>
        public int Timeout;
    }

    public class csDeepLearningCloudParameters: csDeepLearningRequest
    {
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }

        public List<csDeepLearningSettings> ModelSettings { get; set; } = new List<csDeepLearningSettings>();

    }

    public class csDeepLearningSettings
    {
        public int ModelIndex { get; set; }

        public int Threadhold { get; set; }

    }
}
