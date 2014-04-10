using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lib1
{
    [Flags]
    public enum LogDisplay
    {
        None = 0,
        Time = 1 << 0,
        Date = 1 << 1,
        Debug = 1 << 2,
        Warning = 1 << 3,
        Error = 1 << 4
    }

    public enum MessageType
    {
        Debug = LogDisplay.Debug,
        Warning = LogDisplay.Warning,
        Error = LogDisplay.Error
    }

    public interface ILogHandler
    {
        LogDisplay Mode { get; set; }   // = LogDisplay.Time | LogDisplay.Debug | LogDisplay.Warning | LogDisplay.Error;
        string DateFormat { get; set; } // = "yyMMdd";
        string TimeFormat { get; set; } // = "HHmmss.fff";
        
        //---- Stream helpers
        //- Flush will be called every so often, to allow for any buffers to be emtied
        void flush();
        //- Close will be called when the program exits (hopefully)
        void close();

        void WriteLine(MessageType MsgType, string message);

        string DatePrefix { get; set; }

        string DateSuffix { get; set; }

        string TypeFormat(MessageType type);
    }
}
