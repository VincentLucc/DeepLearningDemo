using HalconDotNet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using DevExpress.DataAccess.Native.Web;

namespace Deep_Learning_Demo.Classes
{
    public class csDeepLearningServerHelper
    {
        static HttpClient httpClient = new HttpClient();

        public static void InitServices(int iTimeout = 1000)
        {
            //This value can only be set once
            httpClient.Timeout = TimeSpan.FromMilliseconds(iTimeout);
            //Prepare request
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "CSharpHttpClient/1.0");
        }

        public static async Task<csDeepLearningAPIResponse> RequestInspection(HImage image)
        {
            var result = new csDeepLearningAPIResponse();
            try
            {
                
                Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} RequestInspection.Enter");
                var bData = GetImageTiffByte(image);
                if (bData == null)
                {
                    result.Message = "The input image is invalid.";
                    return result;
                }
    
                //Check server
                if (string.IsNullOrEmpty(csConfigHelper.config.ServerUrl))
                {
                    result.Message = "The server URL is not set.";
                    return result;
                }

                //Notice: must keep only one [/], [//] won't work
                string sUrl = $"{csConfigHelper.config.ServerUrl}/upload-image";

                //Create request
                var content = new ByteArrayContent(bData);
                //When use raw image data
                //content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                //Must set to image
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/tiff");
                //Debug info
                content.Headers.Add("parameters", GetAPIRequestParameterString());
                //content.Headers.Add("parameters", "GetAPIRequestParameterString()");

                //Send request
                HttpResponseMessage response = await httpClient.PostAsync(sUrl, content);
                response.ShowResponseInfo();
                string sContentType = response.GetResponseContentType();

                if (!response.IsSuccessStatusCode)
                {
                    if (sContentType.Contains("text/plain"))
                    {
                        string sContent = await response.Content.ReadAsStringAsync();
                        Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} Response Text:\r\n{sContent}");
                        result.Message = $"Error {(int)response.StatusCode}\r\n{sContent}";
                        return result;
                    }
                    else
                    {
                        result.Message = $"Error {(int)response.StatusCode}\r\n{response.ReasonPhrase}";
                        return result;
                    }
               
                }
               
                HObject responseImage = null;
             
                if (sContentType == "application/json")
                {
                    string sContent = await response.Content.ReadAsStringAsync();
                    Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} Response Json:\r\n{sContent}");
                }
                else if (sContentType.Contains("text/plain"))
                {
                    string sContent = await response.Content.ReadAsStringAsync();
                    Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} Response Text:\r\n{sContent}");
                }
                else if (sContentType == "image/png")
                {//Convert png to the Halcon image
                    var dataStream = await response.Content.ReadAsStreamAsync();

                    responseImage = PngStreamToHObject((MemoryStream)dataStream);

                    string sFilePath = $"{Application.StartupPath}\\{csDateTimeHelper.DateTime_fff}.tiff";

                    responseImage.SaveImage(sFilePath);
                }


                //Pass all steps
                Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} RequestInspection.Success");
                result.ResponseImage = responseImage;
                result.Message = $"Success.";
                result.IsSuccess = true;
                return result;
            }
            catch (Exception ex)
            {

                Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} Request.Exception: {ex.GetMessageDetail()}");
                result.Message = $"Exception.\r\n {ex.Message}";
                return result;
            }

        }


        public static string GetAPIRequestParameterString()
        {
            var apiRequest = new csDeepLearningAPIRequest();
            apiRequest.Models = csConfigHelper.config.Models;

            string sData= JsonConvert.SerializeObject(apiRequest);
            return sData;
        }

        /// <summary>
        /// No format, pure data
        /// </summary>
        /// <param name="hObject"></param>
        /// <returns></returns>
        public static byte[] GetImageRawBytes(HImage hObject)
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

        public static byte[] GetImageTiffByte(HImage hObject)
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


            string sFilePath = $"{Application.StartupPath}\\{csDateTimeHelper.DateTime_fff}.tiff";

            HOperatorSet.WriteImage(hObject, "tiff", 0, sFilePath);
            var bData = File.ReadAllBytes(sFilePath);
            File.Delete(sFilePath);
            return bData;

        }

        public static HObject PngStreamToHObject(MemoryStream pngStream)
        {
            //create image from stream
            using (Bitmap bitmap = new Bitmap(pngStream))
            {
                //Check format
                if (bitmap.PixelFormat != PixelFormat.Format24bppRgb &&
                    bitmap.PixelFormat != PixelFormat.Format8bppIndexed)
                {
                    Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} PngStreamToHObject.Invalid Format:{bitmap.PixelFormat}");
                    return null;
                }

                //Lock the data
                BitmapData bmpData = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly,
                    bitmap.PixelFormat
                );

                try
                {
                    //Create hobject
                    HObject image = new HObject();
                    int width = bitmap.Width;
                    int height = bitmap.Height;

                    if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
                    {
                        // gray image
                        HOperatorSet.GenImage1(out image, "byte", width, height, bmpData.Scan0);
                    }
                    else
                    {
                        // rgb image RGB
                        HOperatorSet.GenImageInterleaved(out image, bmpData.Scan0, "rgb",
                            width, height, -1, "byte", width, height, 0, 0, 8, 0);
                    }

                    return image;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} PngStreamToHObject.Exception:{ex.GetMessageDetail()}");
                    return null;
                }
                finally
                {
                    bitmap.UnlockBits(bmpData);
                }
            }
        }
    }
}
