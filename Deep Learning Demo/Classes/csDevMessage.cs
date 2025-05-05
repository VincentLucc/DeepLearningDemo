using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Camera;
using DevExpress.XtraSplashScreen;
using OpenCV_Halcon_Demo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenCV_Halcon_Demo
{
    /// <summary>
    /// Handle devExpress message box 
    /// </summary>
    public class csDevMessage
    {
        public Form ParentForm;

        /// <summary>
        /// Indicate whether message box exist in current folder
        /// </summary>
        public bool IsMessageBoxExist { get; set; }

        public csDevMessage(Form _parentForm)
        {
            ParentForm = _parentForm;
        }

        /// <summary>
        /// Some cases the SplashScreenManager.ShowDefaultWaitForm() won't show message, use this wrapper to show message instead 
        /// </summary>
        /// <param name="sMessage"></param>
        public void ShowMainLoading(string sCaptain = null, string sDescription = null)
        {
            SplashScreenManager.ShowDefaultWaitForm(ParentForm, true, false, sCaptain, sDescription);
        }

        public void ShowMainLoading()
        {
            SplashScreenManager.ShowDefaultWaitForm(ParentForm, true, false, "Loading...", "Please wait.");
        }
        public void CloseLoadingForm()
        {
            try
            {
                SplashScreenManager.CloseForm(false);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"CloseLoadingForm.Exception:\r\n{e.GetMessageDetail()}");
            }

        }

        public void Error(string text, string caption = "Error")
        {
            SplashScreenManager.CloseForm(false);
            IsMessageBoxExist = true;
            XtraMessageBox.Show(ParentForm, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            IsMessageBoxExist = false;
        }

 
        public DialogResult ErrorConfirmOK(string text, string caption = "Error")
        {
            SplashScreenManager.CloseForm(false);
            IsMessageBoxExist = true;
            var result = XtraMessageBox.Show(ParentForm, text, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
            IsMessageBoxExist = false;
            return result;
        }

        public DialogResult ErrorConfirmYes(string text, string caption = "Error")
        {
            SplashScreenManager.CloseForm(false);
            IsMessageBoxExist = true;
            var result = XtraMessageBox.Show(ParentForm, text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            IsMessageBoxExist = false;
            return result;
        }

        public DialogResult ErrorConfirmOK(string text)
        {
            SplashScreenManager.CloseForm(false);
            IsMessageBoxExist = true;
            var result = XtraMessageBox.Show(ParentForm, text, "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
            IsMessageBoxExist = false;
            return result;
        }

        public void Warning(string text, string caption = "Warning")
        {
            SplashScreenManager.CloseForm(false);
            IsMessageBoxExist = true;
            var result = XtraMessageBox.Show(ParentForm, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            IsMessageBoxExist = false;
        }

        public DialogResult WarningConfirmOK(string text, string caption = "Warning")
        {
            SplashScreenManager.CloseForm(false);
            IsMessageBoxExist = true;
            var result = XtraMessageBox.Show(ParentForm, text, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            IsMessageBoxExist = false;
            return result;
        }

        public void Info(string text, string caption = "Info")
        {
            SplashScreenManager.CloseForm(false);
            IsMessageBoxExist = true;
            XtraMessageBox.Show(ParentForm, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            IsMessageBoxExist = false;
        }

        /// <summary>
        /// Show message only once and won't affect current loop
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        public void InfoAsyncNoRepeat(string text, string caption = "Info")
        {

            if (IsMessageBoxExist) return;

            Task.Run(() =>
            {
                ParentForm.Invoke(new Action(() =>
                {
                    SplashScreenManager.CloseForm(false);
                    IsMessageBoxExist = true;
                    CloseLoadingForm();
                    XtraMessageBox.Show(ParentForm, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    IsMessageBoxExist = false;
                }));
            });
        }


        /// <summary>
        /// Show message only once and won't affect current loop
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        public void ErrorAsyncNoRepeat(string text, string caption = "Error")
        {

            if (IsMessageBoxExist) return;

            Task.Run(() =>
            {
                ParentForm.Invoke(new Action(() =>
                {
                    SplashScreenManager.CloseForm(false);
                    IsMessageBoxExist = true;
                    CloseLoadingForm();
                    XtraMessageBox.Show(ParentForm, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    IsMessageBoxExist = false;
                }));
            });
        }


        public DialogResult InfoConfirmOK(string text, string caption = "Info")
        {
            SplashScreenManager.CloseForm(false);
            IsMessageBoxExist = true;
            var result = XtraMessageBox.Show(ParentForm, text, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            IsMessageBoxExist = false;
            return result;
        }
    }
}
