using HalconDotNet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Deep_Learning_Demo.Classes
{
    public class csDeepLearningServerHelper
    {
        static HttpClient httpClient = new HttpClient();
        static csDeepLearningCloudParameters LocalParameters = new csDeepLearningCloudParameters();

        public static void InitServices(int iTimeout = 1000)
        {
            //This value can only be set once
            httpClient.Timeout = TimeSpan.FromMilliseconds(iTimeout);
            //Prepare request
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "CSharpHttpClient/1.0");
        }

        public static async Task<(bool IsSuccess, string Message)> RequestInspection(HImage image, int iTimeout)
        {
            try
            {
                //Get image array
                var bData = GetImageBytes(image);
                if (bData == null) return (false, "The input image is invalid.");

                //Check server
                if (string.IsNullOrEmpty(csConfigHelper.config.ServerUrl))
                {
                    return (false, "The server URL is not set.");
                }


                string sUrl = $"{csConfigHelper.config.ServerUrl}//upload-image";

                //Create request
                var content = new ByteArrayContent(bData);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                //Add request
                //string sValue = JsonConvert.SerializeObject(LocalParameters);
                //content.Headers.Add("Parameters", sValue);

                //Send request
                HttpResponseMessage response = await httpClient.PostAsync(sUrl, content);
                //response.EnsureSuccessStatusCode();

                //Pass all steps
                return (true, $"Success");
            }
            catch (Exception ex)
            {

                Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} Request.Exception {ex.GetMessageDetail()}");
                return (false, $"Exception.\r\n {ex.Message}");
            }

        }

        public static byte[] GetImageBytes(HImage hObject)
        {
            //Verify type and channel
            if (hObject == null ||
                !hObject.IsInitialized() ||
                hObject.CountObj() == 0 ||
                hObject.CountChannels() != 1)
            {
                Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} GetImageBytes.Invalid Input Image.");
                return null;
            }

            //Get byte array 
            var imagePointer = hObject.GetImagePointer1(out string sType, out int iWidth, out int iHeight);
            int iSize = iWidth * iHeight;

            //Copy all data to local
            byte[] sourceBuffer = new byte[iSize];
            Marshal.Copy(imagePointer, sourceBuffer, 0, iSize);

            return sourceBuffer;
        }
    }
}
