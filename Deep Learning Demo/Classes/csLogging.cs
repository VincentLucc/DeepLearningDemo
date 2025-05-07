using Deep_Learning_Demo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;


class csLogging
{
    public List<OperationLog> LocalLogs { get; set; }
    public object lockLocalLogs;

    /// <summary>
    /// Number of logs to save
    /// </summary>
    public int BufferNumLimit { get; set; }

    public event EventHandler LogChanged;

    public string LogFolder { get; set; }
    public int SizeLimit { get; set; }
    public Form ParentControl { get; set; }

    static string PreFix = "log_";
    static string Suffix = ".log";

    public string DateString => csDateTimeHelper.CurrentTime.ToString("yyMMdd");

    /// <summary>
    /// Whether write the log to the file
    /// </summary>
    public bool EnableWrite { get; set; }

    public bool UIExit => ParentControl == null || ParentControl.IsDisposed || ParentControl.IsDisposed;
    public csLogging(Form _parentForm)
    {
        LocalLogs = new List<OperationLog>();
        lockLocalLogs = new object();
        BufferNumLimit = 5000;
        ParentControl = _parentForm;
        LogFolder = Application.StartupPath + @"\Log\";
        EnableWrite = true;
        SizeLimit = 1024 * 1024 * 1024; //Size limit 1024 MB
        Thread tWrite = new Thread(ProcessWriteLog);
        tWrite.IsBackground = true;
        tWrite.Start();
    }

    public void ProcessWriteLog()
    {
        //Init variables
        string sFileName = "";
        string sFilePath = "";

        while (!UIExit && EnableWrite)
        {
            Thread.Sleep(1000);
            try
            {
                //Check log existence
                IEnumerable<OperationLog> unSavedLogs;
                lock (lockLocalLogs)
                {
                    unSavedLogs = LocalLogs.Where(l => !l.IsSaved);
                    if (unSavedLogs == null || unSavedLogs.Count() < 1) continue;
                }

                //Check Today's log
                sFileName = $"\\{PreFix}{DateString}.log";
                sFilePath = LogFolder + sFileName;
                if (!File.Exists(sFilePath))
                {
                    if (!Directory.Exists(LogFolder)) Directory.CreateDirectory(LogFolder);
                    using (File.Create(sFilePath)) { }
                }


                //Check log file size
                var fileInfo = new FileInfo(sFilePath);
                long length = fileInfo.Length; //Get file size in byte
                if (length > SizeLimit)
                {
                    Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} The log file size limit reached({sFilePath}).");
                    return;
                }

                //Write to file
                using (FileStream fs = new FileStream(sFilePath, FileMode.Append, FileAccess.Write,
                           FileShare.Read, 8192, FileOptions.WriteThrough))
                {
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        foreach (var log in unSavedLogs)
                        {
                            string sMessage = log.GetMessage();
                            if (string.IsNullOrEmpty(sMessage)) continue;
                            writer.WriteLine(sMessage);
                            log.IsSaved = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} ProcessWriteLog.Exception:{ex.GetMessageDetail()}");
            }
        }
    }

    public void AddLogMessage(string sMessage, _logType logType = _logType.General)
    {
        OperationLog log = new OperationLog()
        {
            Type = logType,
            Message = sMessage
        };

        AddLog(log);
    }


    private void AddLog(OperationLog log)
    {
        lock (lockLocalLogs)
        {//Remove one at a time, for better visual result
            while (LocalLogs.Count > BufferNumLimit) LocalLogs.RemoveAt(0);
            LocalLogs.Add(log);
        }

        //Event must put outside of the lock!!!
        LogChanged?.Invoke(null, null);
    }

}

