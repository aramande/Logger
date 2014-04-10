using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lib1
{
    public class ConsoleLogHandler : LogHandler
    {
        public ConsoleLogHandler()
        {
            this.DatePrefix = "";
            this.DateSuffix = " > ";
            this.TimeFormat = "HH:mm:ss.fff";
            this.Mode = LogDisplay.Time | LogDisplay.Debug | LogDisplay.Warning | LogDisplay.Error;
        }

        public override void WriteLine(MessageType type, string message)
        {
            Console.WriteLine(message);
        }

        public override string TypeFormat(MessageType type)
        {
            switch (type)
            {
                case MessageType.Error:
                    return "ERROR: ";
                case MessageType.Warning:
                    return "Warning: ";
                case MessageType.Debug:
                    return "";
            }
            return "";
        }
    }

}
