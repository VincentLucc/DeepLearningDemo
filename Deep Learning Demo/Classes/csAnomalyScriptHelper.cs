using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.XtraRichEdit;
using HalconDotNet;
using static DevExpress.Diagram.Core.Native.Either;

namespace Deep_Learning_Demo
{
    internal class csAnomalyScriptHelper
    {
        private const string CommunicationNameBase = "DeltaX-Vision_Anomaly";

        private const long DefaultSize = 100 * 1024 * 1024;

        public const string pythonHome = @"C:\Program Files\Python312";

        public const string scriptPath =
            @"E:\Backup\Companies\PackSmart\Projects\DeepLearning\DeepLearning.Client.Git\Deep Learning Demo\PythonScripts\model_runner_parallel.py";

        public static List<Process> ScriptProcess = new List<Process>();

        public static void StartPythonProcesses(int iProfileIndex)
        {
            string sPythonExe = $"{pythonHome}\\python.exe";
            string sRequest = GetSystemName(iProfileIndex, _comDirection.Request);
            string sResponse = GetSystemName(iProfileIndex, _comDirection.Respoonse);
            string sArgument = $"\"{scriptPath}\" {sRequest} {sResponse} {iProfileIndex}";

            Process process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = sPythonExe,
                Arguments = sArgument,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = false //Make it visible for now
            };

            process.Start();
            ScriptProcess.Add(process);

        }

        public static void CloseAllProcesses()
        {
            foreach (var p in ScriptProcess)
            {
                try
                {
                    if (!p.HasExited)
                    {
                        p.Kill();      // 强制终止
                        p.WaitForExit(500);
                    }
                }
                catch { /*可记录日志*/ }
                finally
                {
                    p.Dispose();
                }
            }

            ScriptProcess.Clear();
        }


        /// <summary>
        /// The memory-mapped file name and event name can be the same 
        /// </summary>
        /// <param name="iProfileIndex"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string GetSystemName(int iProfileIndex, _comDirection content)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"{CommunicationNameBase}_{iProfileIndex}");
            //Add direction
            switch (content)
            {
                case _comDirection.Request:
                    builder.Append($"_Request");
                    break;
                case _comDirection.Respoonse:
                    builder.Append($"_Response");
                    break;
                default:
                    break;
            }

            return builder.ToString();
        }


        public static (HObject ResponseImage, string Message) Request(HObject image, int iProfileIndex, int iTimeout)
        {
            "AnomalyScriptHelper.Request.Enter".TraceRecord();
            try
            {
                if (!image.IsValid()) return (null, "Input is not a valid image.");

                //Check the image file
                var rawData = image.HobjectToRawByte();
                HOperatorSet.GetImageSize(image, out HTuple width, out HTuple height);

                //Write request data
                string sRequestName = GetSystemName(iProfileIndex, _comDirection.Request);
                using (var mmf = MemoryMappedFile.CreateOrOpen(sRequestName, DefaultSize))
                {
                    using (var viewAcc = mmf.CreateViewAccessor())
                    {
                        int iSize = rawData.Length;
                        //Write file size
                        viewAcc.Write(0, iSize);
                        //Write image size
                        viewAcc.Write(4, width.I);
                        viewAcc.Write(8, height.I);
                        //Write timeout
                        viewAcc.Write(12, iTimeout);
                        //Write actual file
                        viewAcc.WriteArray(16, rawData, 0, rawData.Length);
                    }
                }

                //Create request event
                var requestDataReady = new EventWaitHandle(initialState: false, EventResetMode.AutoReset, sRequestName);

                //Notice request is ready
                requestDataReady.Set();


                //Get response
                string sResponseName = GetSystemName(iProfileIndex, _comDirection.Respoonse);
                var responseDataReady = new EventWaitHandle(initialState: false, EventResetMode.AutoReset, sResponseName);

                //Wait for response
                if (!responseDataReady.WaitOne(iTimeout))
                {
                    return (null, "Request timeout(PSH56).");
                }

                //Read response data
                byte[] responseArray = null;
                using (var mmf = MemoryMappedFile.CreateOrOpen(sResponseName, DefaultSize))
                {
                    using (var viewAcc = mmf.CreateViewAccessor())
                    {
                        //Read file size
                        int iSize = viewAcc.ReadInt32(0);

                        responseArray = new byte[iSize];
                        viewAcc.ReadArray(0, responseArray, 0, iSize);
                    }
                }

                //To Halcon image
                var responseImage = responseArray.MonoBytesToHObject((int)width.D, (int)height.D);

                //Success
                return (responseImage, null);

            }
            catch (Exception e)
            {
                e.TraceException("Request");
                return (null, $"Response Error(PSH66),{e.Message}");
            }
        }



        public enum _comDirection
        {
            Request,
            Respoonse,
        }
    }
}
