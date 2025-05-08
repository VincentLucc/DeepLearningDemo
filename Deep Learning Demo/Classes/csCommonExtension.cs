using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Deep_Learning_Demo
{
    public static class csCommonExtension
    {

        public static void ShowResponseInfo(this HttpResponseMessage response)
        {
            int iIndex = 1;
            foreach (var header in response.Headers)
            {
                Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} Header({iIndex}):[{header.Key}, {string.Join(", ", header.Value)}]");
                iIndex++;
            }

            iIndex = 1;
            foreach (var header in response.Content.Headers)
            {
                Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} Content({iIndex}):[{header.Key}, {string.Join(", ", header.Value)}]");
                iIndex++;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="response"></param>
        /// <returns>"application/json"</returns>
        public static string GetResponseContentType(this HttpResponseMessage response)
        {
            //Get the response type
            
            var contentType = response.Content.Headers.
                FirstOrDefault(a => a.Key == "Content-Type");
            if (contentType.Key == null || contentType.Value == null)
            {
                return null;
            }

            if (contentType.Key != null && contentType.Value != null)
            {
                string sType1 = contentType.Value.FirstOrDefault();
                return sType1;
            }

            //No matches
            return null;

        }

        public static void SetIntialFolder(this XtraFolderBrowserDialog dialog, string sCurrentFolder, string sDefaultFolder)
        {
            if (string.IsNullOrWhiteSpace(sCurrentFolder))
            {//Load default folder
                if (!string.IsNullOrWhiteSpace(sDefaultFolder) && Directory.Exists(sDefaultFolder))
                {
                    dialog.SelectedPath = sDefaultFolder;
                }
                else
                {
                    dialog.SelectedPath = Application.StartupPath;
                }
            }
            else
            {//Load existing folder
                if (Directory.Exists(sCurrentFolder))
                {
                    dialog.SelectedPath = sCurrentFolder;
                }
                else
                {
                    dialog.SelectedPath = Application.StartupPath;
                }
            }
        }




        /// <summary>
        /// Avoid un-needed update
        /// </summary>
        /// <param name="item"></param>
        /// <param name="bVisible"></param>
        public static void SetBarItemVisibility(this BarItem item, bool bVisible)
        {
            var visibility = bVisible ? BarItemVisibility.Always : BarItemVisibility.Never;
            if (item.Visibility != visibility) item.Visibility = visibility;
        }

        public static bool JsonPreCheck(this string sMessage)
        {
            if (string.IsNullOrWhiteSpace(sMessage)) return false;
            string sData = sMessage.Trim();
            //Check format
            if (!(sData.StartsWith("{") && sData.EndsWith("}")) &&
                !(sData.StartsWith("[") && sData.EndsWith("]")))
            {
                return false;
            }

            return true;
        }

        public static bool WriteToDisk<T>(this T Item, string sFilePath)
        {
            try
            {
                string sData = JsonConvert.SerializeObject(Item);
                File.WriteAllText(sFilePath, sData);
                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"WriteToDisk.Exception:{e.GetMessageDetail()}");
                return false;
            }

        }

        /// <summary>
        /// Delete extra files in a folder
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="folderFileLimit"></param>
        /// <returns></returns>
        public static int DeleteFolderFileAction(this DirectoryInfo directory, int folderFileLimit)
        {
            //Init
            int iDeletionCount = 0;

            try
            {
                //Get sorted files
                var files = directory.GetFiles();
                if (files.Length <= folderFileLimit) return iDeletionCount;
                //Sort the files
                var sortedFiles = files.OrderBy(a => a.CreationTime).ToList();

                //Start deletion
                for (int i = 0; i < sortedFiles.Count - folderFileLimit; i++)
                {
                    var fileInfo = sortedFiles[i];
                    fileInfo.Delete();
                    iDeletionCount += 1;
                }

                return iDeletionCount;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"DeleteFolderFileAction.Exception:\r\n{e.GetMessageDetail()}");
                return -1;
            }
        }

        public static bool? GetBoolValue(this object oValue)
        {
            if (oValue is string)
            {
                string sValue = oValue as string;
                if (bool.TryParse(sValue, out bool bResult)) return bResult;
                else if (sValue.ToLower() == "on") return true;
                else if (sValue.ToLower() == "off") return false;
                else if (sValue.ToLower() == "active") return true;
                else if (sValue.ToLower() == "inactive") return true;
                else if (sValue.ToLower() == "positive") return true;
                else if (sValue.ToLower() == "negative") return true;
                else if (sValue.ToLower() == "1") return true;
                else if (sValue.ToLower() == "0") return false;
                else return null;
            }
            else if (oValue is int || oValue is long)
            {
                if (!int.TryParse(oValue.ToString(), out int iValue)) return null;
                if (iValue == 0) return false;
                else if (iValue == 1) return true;
                else return null;
            }
            else if (oValue is bool)
            {
                return (bool)oValue;
            }
            else return null;
        }

        public static double GetDoubleValue(this object oValue)
        {
            if (oValue == null) return -1;
            if (!double.TryParse(oValue.ToString(), out double dValue)) return -1;
            return dValue;
        }

        public static int GetIntValue(this object oValue)
        {
            if (oValue == null) return -1;
            //Int try-parse might get -1 for decimal values, use double instead
            string sValue = oValue.ToString();
            if (!double.TryParse(sValue, out double dValue)) return -1;
            int iValue = (int)dValue;
            return iValue;
        }

        public static string GetValidString(this object oValue)
        {
            if (oValue == null) return string.Empty;
            return oValue.ToString();
        }

        public static int GetValidLength(this object oValue)
        {
            if (oValue is string sValue)
            {
                return sValue.Length;
            }
            else
            {
                return 0;
            }

        }

        public static string GetMessageDetail(this Exception exception)
        {
            if (exception == null) return string.Empty;
            string msg = exception.Message;
            if (exception.InnerException != null)
            {
                msg += $"\r\n{exception.InnerException.Message}";
            }

            //Show stack trace
            if (!string.IsNullOrWhiteSpace(exception.StackTrace))
            {
                msg += $"\r\n{exception.StackTrace}";
            }

            return msg;
        }

        public static bool IsFormVisible(this Form form)
        {
            return form == null || form.Disposing || form.IsDisposed || !form.Visible;
        }

        public static bool IsFormClosed(this Form form)
        {
            return form == null || form.Disposing || form.IsDisposed;
        }

        public static bool IsUserControlDisposed(this UserControl uc)
        {
            return uc == null || uc.Disposing || uc.IsDisposed || !uc.Visible;
        }
    }
}
