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
            this.FormClosed += MainForm_FormClosed;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
          
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
            FakeResponseBarToggleSwitchItem.Checked = csConfigHelper.config.FakeResponse;
            int iTimeout = csConfigHelper.config.APISettings.Timeout;
            csDeepLearningServerHelper.InitServices(iTimeout);
            //Make sure all process killed even crashed
            Task.Run(csAnomalyScriptHelper.ProcessKill);

            //Init local script service
            for (int i = 0; i < 2; i++)
            {
                csAnomalyScriptHelper.StartPythonProcesses(i);
                //Start the fake response
                int iLocal = i; //Avoid threading wrong value
                Task.Run(() => csAnomalyScriptHelper.ProcessFakeResponse(iLocal));
            }


             
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

                if (!csConfigHelper.SaveToDefault(out string sMessage))
                {
                    MessageHelper.Info(sMessage);
                }

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
            var watch = Stopwatch.StartNew();

            try
            {
   
                var requestAction = csAnomalyScriptHelper.Request(image, 0, 5000);

                if (requestAction.ResponseImage == null)
                {
                    MessageHelper.Error(requestAction.Message);
                    return;
                }


                ProcessTimeBarButtonItem.Caption = $"Last Request: {watch.ElapsedMilliseconds.ToString("f1")}ms";

                //Show the result
                HalconWindow.DisplayImage(requestAction.ResponseImage);

            }
            catch (Exception e)
            {

                Trace.WriteLine($"ProcessLocalPythonScript.Exception:{e.GetMessageDetail()}");
                MessageHelper.Error(e.Message);
            }
            finally
            {
                "ProcessLocalPythonScript.Complete".TraceRecord();
            }

        }

 

        private void WorkModeBarEditItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void FakeResponseBarToggleSwitchItem_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!IsFormLoad)return;

          csConfigHelper.config.FakeResponse=  FakeResponseBarToggleSwitchItem.Checked;
        }
    }
}
