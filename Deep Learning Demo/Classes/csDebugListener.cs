using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Write debug info into the logging class
/// </summary>
public class csDebugListener : TraceListener
{

    internal csLogging logHelper;

    /// <summary>
    /// Don't change
    /// </summary>
    public TextWriter TextWriter { get; set; }

    internal csDebugListener(csLogging _logHelper)
    {
        logHelper = _logHelper;
    }

    public override void Write(string message)
    {
        logHelper.AddLogMessage(message, _logType.Debug);
    }

    public override void WriteLine(string message)
    {
        logHelper.AddLogMessage(message, _logType.Debug);
    }
}