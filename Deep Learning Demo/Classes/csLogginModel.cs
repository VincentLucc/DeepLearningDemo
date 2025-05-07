using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;


[Serializable]
public class OperationLog
{
    [Browsable(false)]
    public DateTime Time { get; set; }

    [DisplayName("Time")]
    public string TimeView => Time.ToString("yyyy'/'MM'/'dd' 'HH':'mm");

    [Browsable(false)]
    public string TimeLog => Time.ToString("yyyy'/'MM'/'dd' 'HH':'mm':'ss':'fff");

    [Browsable(false)]
    public _logType Type { get; set; }


    [Browsable(false)]
    public bool IsSaved { get; set; }

    [XmlIgnore]
    public string Message { get; set; }

    public OperationLog()
    {
        Time = DateTime.Now;
    }

    public virtual string GetMessage()
    {
        string sMessage = string.Empty;

        if (Type == _logType.Debug)
        {//Only show the row message
            if (!string.IsNullOrEmpty(Message))
                sMessage += Message;
        }
        else
        {
            sMessage = $"{TimeLog} {(int)Type} ";
            if (!string.IsNullOrEmpty(Message)) sMessage += Message;
        }

        return sMessage;
    }


}

public enum _logType
{
    General = 0,
    Exception = 1,
    AlarmChange = 2,
    Debug = 3,
}