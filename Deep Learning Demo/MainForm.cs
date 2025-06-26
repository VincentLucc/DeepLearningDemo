using Deep_Learning_Demo.Classes;
using Deep_Learning_Demo.Forms;
using DevExpress.XtraEditors;
using HalconDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Python.Runtime;

namespace Deep_Learning_Demo
{
    public partial class MainForm : XtraForm
    {
        internal csHalconWindow HalconWindow = new csHalconWindow();
        public bool IsFormLoad;
        public csDevMessage MessageHelper;


        public MainForm()
        {
            InitializeComponent();
            InitEvents();
        }



        private void InitEvents()
        {
            this.Shown += MainForm_Shown;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            //Init Halcon window
            HalconWindow.LinkWindow(hWindowControl1);
            Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} App started");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MessageHelper = new csDevMessage(this);
            InitLogging();

            if (!csConfigHelper.LoadOrCreateConfig(out string sMessage))
            {
                if (!string.IsNullOrWhiteSpace(sMessage)) MessageHelper.Info(sMessage);
                this.Close();
                return;
            }

            InitWorkModeLookupEdit();
            int iTimeout = csConfigHelper.config.APISettings.Timeout;
            csDeepLearningServerHelper.InitServices(iTimeout);


            //Complete
            timer1.Start();
            IsFormLoad = true;
        }

        private void InitWorkModeLookupEdit()
        {
            var workModeOptions = Enum.GetValues(typeof(_workMode));
            workModeLookUpEdit.DataSource = workModeOptions;
            workModeLookUpEdit.DropDownRows = workModeOptions.Length;
            workModeLookUpEdit.ShowFooter = false;
            WorkModeBarEditItem.EditValue = csConfigHelper.config.WorkMode;
            WorkModeBarEditItem.EditValueChanged += WorkModeBarEditItem_EditValueChanged;
        }

        private void WorkModeBarEditItem_EditValueChanged(object sender, EventArgs e)
        {
            if (!IsFormLoad) return;
            if (WorkModeBarEditItem.EditValue is _workMode workMode)
            {
                csConfigHelper.config.WorkMode = workMode;
                $"WorkModeChanged:{csConfigHelper.config.WorkMode}".TraceRecord();
            }
        }


        private void InitLogging()
        {
            csPublic.DebugLogger = new csLogging(this);
            csPublic.DebugLogger.LogFolder = Application.StartupPath + "\\Log";

            //Init debug log
            csDebugListener traceListener = new csDebugListener(csPublic.DebugLogger);
            Debug.Listeners.Add(traceListener);
        }

        private void OpenImageBarButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            using (XtraOpenFileDialog openFileDialog = new XtraOpenFileDialog())
            {
                openFileDialog.Filter = csPublic.ImageFileFilter;
                if (openFileDialog.ShowDialog() != DialogResult.OK) return;

                //Read image file
                var halconImage = csHalconHelper.ReadImageFile(openFileDialog.FileName, out string sMessage);
                if (halconImage == null)
                {
                    MessageHelper.Error(sMessage);
                    return;
                }

                //Show image
                HalconWindow.DisplayImage(halconImage);

            }
        }



        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.IsFormClosed())
            {
                timer1.Enabled = false;
                return;
            }

            try
            {
                //Overflow protection
                timer1.Enabled = false;

                if (HalconWindow.View.IsViewImageValid)
                {
                    ImageSizeBarButtonItem.Caption = $"Image Size: {HalconWindow.LastImageSize.Width}x{HalconWindow.LastImageSize.Height}";
                }
                else
                {
                    ImageSizeBarButtonItem.Caption = "Image Size: N/A";
                }



            }
            catch (Exception ex)
            {

                Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} timer1_Tick.Exception:{ex.GetMessageDetail()}");
            }
            finally
            {
                timer1.Enabled = true;
            }
        }

        private void ResetViewBarButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            HalconWindow.ZoomOrigin();
        }

        private async void RequestBarButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                this.Enabled = false;

                //Verify the image
                if (!HalconWindow.View.IsViewImageValid)
                {//
                    MessageHelper.Info("Please load a valid image.");
                    return;
                }

                MessageHelper.ShowMainLoading();
                var image = HalconWindow.View.GetViewImage();

                switch (csConfigHelper.config.WorkMode)
                {
                    case _workMode.API:
                        await ProcessAPIRequest(image);
                        break;
                    case _workMode.PyhtonScript:
                        ProcessLocalPythonScript(image);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"RequestBarButtonItem_ItemClick.exception:{ex.GetMessageDetail()}");
            }
            finally
            {
                MessageHelper.CloseLoadingForm();
                this.Enabled = true;
            }

        }



        private async Task ProcessAPIRequest(HObject image)
        {
            var apiResponse = await csDeepLearningServerHelper.RequestInspection(new HImage(image));
            ProcessTimeBarButtonItem.Caption = $"Last Request: {apiResponse.GetDuration().ToString("f1")}ms";
            if (!apiResponse.IsSuccess)
            {
                MessageHelper.Info(apiResponse.Message);
                return;
            }

            //Show image
            if (apiResponse.ResponseImage != null)
            {
                HalconWindow.DisplayImage(apiResponse.ResponseImage);
            }

            //Success
            MessageHelper.Info(apiResponse.Message);
            apiResponse.Dispose();
        }

        private void ProcessLocalPythonScript(HObject image)
        {
            //Ignore the init time
            string sMessage = String.Empty;
            if (!csConfigHelper.IsPythonEngineInit)
            {//Make sure only init once
                if (!InitPythonEngine(out sMessage))
                {
                    MessageHelper.Error(sMessage);
                    return;
                }
            }

            //Start the operation
            "ProcessLocalPythonScript.Enter".TraceRecord();
            Stopwatch watch = Stopwatch.StartNew();
            var rawImage = image.HobjectToRawByte();
            HOperatorSet.GetImageSize(image, out HTuple width, out HTuple height);
            "ProcessLocalPythonScript.RawBytesReady".TraceRecord();

            using (Py.GIL())
            {
                try
                {
                    dynamic sys = Py.Import("sys");
                    $"Python version: {sys.version}".TraceRecord();

                    //Apply module folders
                    foreach (var folder in csConfigHelper.config.PythonModuleFolders)
                    {
                        sys.path.append(folder);
                        $"Python module folder: {folder}".TraceRecord();
                    }

                    //Get module entry
                    dynamic runnerModule = Py.Import("model_runner");
                    //Create instance
                    dynamic myRunner = runnerModule.ModelRunner();

                    //Perform inference
                    dynamic inferResult = myRunner.infer_image(rawImage);

                    //Check result
                    byte[] resultBytes = null;
                    if (inferResult is byte[])
                    {
                        resultBytes = (byte[])inferResult;
                    }
                    else if (inferResult is PyObject pyObj)
                    {
                        resultBytes = (byte[])pyObj.AsManagedObject(typeof(byte[]));
                    }
                    else
                    {
                        MessageHelper.Error("Unexpected result");
                        return;
                    }

                    //Assuming the result is an image, convert it back to HObject
                    var responseImage = resultBytes.MonoBytesToHObject((int)width.D, (int)height.D);
                    watch.Stop();
                    ProcessTimeBarButtonItem.Caption = $"Last Request: {watch.ElapsedMilliseconds.ToString("f1")}ms";

                    //Show the result
                    HalconWindow.DisplayImage(responseImage);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"ProcessLocalPythonScript.Exception:{ex.GetMessageDetail()}");
                    MessageHelper.Error(ex.Message);
                }
                finally
                {
                    //Don't call this, leave engine running!!!
                    //PythonEngine.Shutdown();
                    "ProcessLocalPythonScript.Complete".TraceRecord();
                }
            }
        }

        private bool InitPythonEngine(out string sMessage)
        {
            sMessage = String.Empty;
            csConfigHelper.IsPythonEngineInit = false;

            try
            {
                "InitPythonEngine.Start".TraceRecord();
                //Prepare
                string sPythonHome = csConfigHelper.config.PythonHome;
                //Must set dll
                Runtime.PythonDLL = $"{sPythonHome}\\python312.dll";
                PythonEngine.PythonHome = sPythonHome;
                //python path (Any call like [PythonEngine.PythonHome] must run after [Initialize])
                StringBuilder pathBuilder = new StringBuilder();
                pathBuilder.Append($"{sPythonHome}\\Lib");
                pathBuilder.Append($";{sPythonHome}\\Lib\\site-packages");
                PythonEngine.PythonPath = pathBuilder.ToString();

                //Start init
                PythonEngine.Initialize();

                //Complete
                csConfigHelper.IsPythonEngineInit = true;
                return true;
            }
            catch (Exception ex)
            {
                ex.TraceException("InitPythonEngine");
                sMessage = $"Init Python Environment Error\r\n{ex.Message}";
                return false;
            }
            finally
            {
                "InitPythonEngine.Complete".TraceRecord();
            }
        }

        private void WorkModeBarEditItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }
    }
}
