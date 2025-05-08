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


            csDeepLearningServerHelper.InitServices(5000);


            //Complete
            timer1.Start();
            IsFormLoad = true;
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
            //Verify the image
            if (!HalconWindow.View.IsViewImageValid)
            {//
                MessageHelper.Info("Please load a valid image.");
                return;
            }

            var image = HalconWindow.View.GetViewImage();

            
            var requestAction = await csDeepLearningServerHelper.RequestInspection(new HImage(image));
            if (!requestAction.IsSuccess)
            {
                MessageHelper.Info(requestAction.Message);
                return;
            }

            //Check result
            if (requestAction.responseImage != null)
            {
                //Show image
                HalconWindow.DisplayImage(requestAction.responseImage);
            }




        }
    }
}
