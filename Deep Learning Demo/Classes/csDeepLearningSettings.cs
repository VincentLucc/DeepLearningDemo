using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deep_Learning_Demo.Classes
{
    public class csDeepLearningAPIResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        public HObject ResponseImage { get; set; }

        public DateTime CreateTime { get; set; } = csDateTimeHelper.CurrentTime;

        public void Dispose()
        {
            IsSuccess = false;
            Message = null;
            if (ResponseImage != null)
            {
                ResponseImage.Dispose();
                ResponseImage = null;
            }
        }

        public double GetDuration()
        {
           return (csDateTimeHelper.CurrentTime - CreateTime).TotalMilliseconds;
        }

    }


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
