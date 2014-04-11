
namespace Lib1
{
    public class DynamicLogHandler : LogHandler
    {
        public delegate void Callback(string message);
        public Callback WriteLineCallback;
        public DynamicLogHandler()
        {
            //this.DatePrefix = "[";
            //this.DateSuffix = "]";
            //this.DateFormat = "yy-MM-dd";
            //this.TimeFormat = "hh:mm:ss.fff";
            this.Mode = LogDisplay.Debug | LogDisplay.Warning | LogDisplay.Error;
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
                    return "Debug: ";
            }
            return "";
        }

        public override void WriteLine(MessageType type, string message)
        {
            WriteLineCallback(message);
        }
    }
}
