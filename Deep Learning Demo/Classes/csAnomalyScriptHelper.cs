using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.DataAccess.Native.Web;
using DevExpress.LookAndFeel;
using DevExpress.XtraRichEdit;
using HalconDotNet;
using static DevExpress.Diagram.Core.Native.Either;

namespace Deep_Learning_Demo
{
    internal class csAnomalyScriptHelper
    {
        private const string CommunicationNameBase = "DeltaXVision_Anomaly";

        /// <summary>
        /// Size is won't be changed once created
        /// </summary>

        private const long DefaultSize = 20 * 1024 * 1024;

        public static List<csScriptContent> ScriptContents = new List<csScriptContent>();

        public static void StartPythonProcesses(int iProfileIndex)
        {
            try
            {
                csScriptContent scriptContent = new csScriptContent();
                scriptContent.ModelIndex = iProfileIndex;
                string sPythonHome = csConfigHelper.config.PythonHome;
                string sPythonExe = $"{sPythonHome}\\python.exe";
                string sScriptPath = csConfigHelper.config.ScriptFile;

                //Memory file name
                string sRequestFile = GetSystemName(iProfileIndex, _comItem.RequestFile);
                string sResponseFile = GetSystemName(iProfileIndex, _comItem.RespoonseFile);
                string sArgument = $"\"{sScriptPath}\" {sRequestFile} {sResponseFile} {iProfileIndex}";

                //Create the memory file, make sure keep in memory
                scriptContent.RequestFile = MemoryMappedFile.CreateOrOpen(sRequestFile, DefaultSize);
                scriptContent.ResponseFile = MemoryMappedFile.CreateOrOpen(sResponseFile, DefaultSize);

                //Process
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
                scriptContent.ScriptProcess = process;
                ScriptContents.Add(scriptContent);
            }
            catch (Exception e)
            {
                e.TraceException($"ScriptHelper.StartPythonProcesses[{iProfileIndex}]");
            }


        }

        public static void CloseAllProcesses()
        {
            foreach (var content in ScriptContents)
            {
                try
                {
                    //Close the response thread
                    content.QuitFakeResponse = false;//Flag to close the response task
                    string sRequestEvent = GetSystemName(content.ModelIndex, _comItem.RequestEvent);
                    var requestDataReady = new EventWaitHandle(initialState: false, EventResetMode.AutoReset, sRequestEvent);
                    requestDataReady.Set();//Remove the event blocking

                    //Close the python task
                    if (!content.ScriptProcess.HasExited)
                    {
                        content.ScriptProcess.Kill();      // Force close
                        content.ScriptProcess.WaitForExit(500);
                    }
                }
                catch { /*可记录日志*/ }
                finally
                {
                    content.ScriptProcess.Dispose();
                }
            }

            ScriptContents.Clear();
        }


        /// <summary>
        /// The memory-mapped file name and event name can be the same 
        /// </summary>
        /// <param name="iProfileIndex"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string GetSystemName(int iProfileIndex, _comItem content)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"Local\\{CommunicationNameBase}_{iProfileIndex}");
            //Add direction
            switch (content)
            {
                case _comItem.RequestFile:
                    builder.Append($"_Request_File");
                    break;
                case _comItem.RequestEvent:
                    builder.Append($"_Request_Event");
                    break;
                case _comItem.RespoonseFile:
                    builder.Append($"_Response_File");
                    break;
                case _comItem.ResponseEvent:
                    builder.Append($"_Response_Event");
                    break;
                default:
                    break;
            }

            return builder.ToString();
        }


        public static (HObject ResponseImage, string Message) Request(HObject image, int iProfileIndex, int iTimeout)
        {
            $"ScriptHelper.Request[{iProfileIndex}].Enter".TraceRecord();
            try
            {
                if (!image.IsValid()) return (null, "Input is not a valid image.");

                //Check the image file
                var rawData = image.HobjectToRawByte();
                HOperatorSet.GetImageSize(image, out HTuple width, out HTuple height);

                //Get current content
                var currentContent = ScriptContents.FirstOrDefault(a => a.ModelIndex == iProfileIndex);
                if (currentContent == null) return (null, $"The profile[{iProfileIndex}] is not yet ready.");

                //Write request data
                using (var viewAcc = currentContent.RequestFile.CreateViewAccessor())
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


                //Create request event
                string sRequestEvent = GetSystemName(iProfileIndex, _comItem.RequestEvent);
                var requestDataReady = new EventWaitHandle(initialState: false, EventResetMode.AutoReset, sRequestEvent);

                //Notice request is ready
                requestDataReady.Set();
                "ScriptHelper.Request.RequestEventReady".TraceRecord();

                //Get response
                string sResponseEvent = GetSystemName(iProfileIndex, _comItem.ResponseEvent);
                var responseDataReady = new EventWaitHandle(initialState: false, EventResetMode.AutoReset, sResponseEvent);

                //Wait for response
                if (!responseDataReady.WaitOne(iTimeout))
                {
                    return (null, $"Request timeout[{iTimeout}ms], PSH56.");
                }

                //Read response data
                byte[] responseArray = null;
                using (var viewAcc = currentContent.ResponseFile.CreateViewAccessor())
                {
                    //Read file size
                    int iSize = viewAcc.ReadInt32(0);

                    responseArray = new byte[iSize];
                    viewAcc.ReadArray(0, responseArray, 0, iSize);
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

        public static void ProcessFakeResponse(int iIndex)
        {
            var mainForm = Application.OpenForms[0];
            while (!mainForm.IsFormClosed())
            {
                try
                {
                    //Wait for request
                    string sRequestEvent = GetSystemName(iIndex, _comItem.RequestEvent);
                    var requestDataReady = new EventWaitHandle(initialState: false, EventResetMode.AutoReset, sRequestEvent);

                    //Wait for request
                    if (!requestDataReady.WaitOne())
                    {//Rare condition
                        Thread.Sleep(20);
                        continue;
                    }

                    //Check content ready
                    var currentContent = ScriptContents.FirstOrDefault(a => a.ModelIndex == iIndex);
                    if (currentContent == null)
                    {
                        Thread.Sleep(20);
                        continue;
                    }

                    //Check quit signal
                    if (currentContent.QuitFakeResponse) break;
      

                    //Check fake flag
                    if (!csConfigHelper.config.FakeResponse)
                    {
                        Thread.Sleep(20);
                        continue;
                    }



                    //Request is ready
                    $"ScriptHelper.FakeResponse({iIndex}).StartRead".TraceRecord();
                    byte[] rawData = null;
                    int iWidth = 0, iHeight = 0, iTimeout = 0;
                    using (var viewAcc = currentContent.RequestFile.CreateViewAccessor())
                    {
                        //Read file size
                        int iSize = viewAcc.ReadInt32(0);
                        if (iSize <= 0) continue;

                        //Read image size
                        iWidth = viewAcc.ReadInt32(4);
                        iHeight = viewAcc.ReadInt32(8);
                        //Read timeout
                        iTimeout = viewAcc.ReadInt32(12);

                        rawData = new byte[iSize];
                        viewAcc.ReadArray(16, rawData, 0, iSize);
                    }
                    $"ScriptHelper.FakeResponse({iIndex}).ReadEnd".TraceRecord();

                    //Prepare fake image
                    var imageReceive = rawData.MonoBytesToHObject(iWidth, iHeight);
                    if (imageReceive == null)
                    {
                        $"ScriptHelper.FakeResponse({iIndex}).RequestImageNull".TraceRecord();
                        continue;
                    }
                    HOperatorSet.InvertImage(imageReceive, out HObject imageInvert);
                    imageReceive.Dispose();
                    var responseArrary = imageInvert.HobjectToRawByte();
                    imageInvert.Dispose();
                    $"ScriptHelper.FakeResponse({iIndex}).ResponseImageReady".TraceRecord();

                    //Write the file back
                    using (var viewAcc = currentContent.ResponseFile.CreateViewAccessor())
                    {
                        //Write file size
                        viewAcc.Write(0, responseArrary.Length);
                        //Write actual file
                        viewAcc.WriteArray(4, responseArrary, 0, responseArrary.Length);
                    }

                    //Create response event
                    string sResponseEvent = GetSystemName(iIndex,_comItem.ResponseEvent);
                    var responseDataReady = new EventWaitHandle(initialState: false, EventResetMode.AutoReset, sResponseEvent);

                    //Notice response is ready
                    responseDataReady.Set();
                }
                catch (Exception ex)
                {
                    ex.TraceException("ScriptHelper.FakeResponse(ASH186)");
                    //Error Protection
                    Thread.Sleep(500);
                }
            }

            $"ScriptHelper.FakeResponse[{iIndex}].Complete".TraceRecord();
        }

        /// <summary>
        /// Make sure the app always being killed 
        /// </summary>
        public static void ProcessKill()
        {
            var mainForm = Application.OpenForms[0];
            while (!mainForm.IsFormClosed())
            {
                Thread.Sleep(100);
            }

            CloseAllProcesses();
        }

        public enum _comItem
        {
            RequestFile,
            RequestEvent,
            RespoonseFile,
            ResponseEvent
        }

        /// <summary>
        /// Event is short
        /// </summary>
        public class csScriptContent
        {
            public int ModelIndex;
            public Process ScriptProcess;
            public MemoryMappedFile RequestFile;
            public MemoryMappedFile ResponseFile;
            /// <summary>
            /// Used to close the response task
            /// </summary>
            public bool QuitFakeResponse;
        }
    }



}
