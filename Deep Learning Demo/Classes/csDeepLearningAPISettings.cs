using DevExpress.XtraLayout.Customization;
using HalconDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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


    public class csDeepLearningAPIRequest: csDeepLearningAPISettings
    {
        public string CommandID { get; set; } = csDateTimeHelper.DateTime_fff;

        public csDeepLearningAPIRequest(csDeepLearningAPISettings apiSettings)
        {
            this.Timeout = apiSettings.Timeout;
            this.ProfileIndex = apiSettings.ProfileIndex;
            this.Models= apiSettings.Models;
            
        }

    }

    public class csDeepLearningAPISettings
    {
        /// <summary>
        /// Timeout in ms
        /// </summary>
        public int Timeout { get; set; } = 5000;
 
        public int ProfileIndex { get; set; } = 0;

        [TypeConverter(typeof(CollectionConverter))] //Show sub-class properties
        public List<csModelSettings> Models { get; set; } = new List<csModelSettings>();

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



    public class csModelSettings
    {
        public int ModelIndex { get; set; } = 1;

        public int Threadhold { get; set; } = 20;

    }
}
