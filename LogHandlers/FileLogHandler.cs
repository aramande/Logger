using System;
using System.IO;

namespace Lib1
{
    public class FileLogHandler : LogHandler
    {
        StreamWriter stream = null;
        readonly string original_filename;
        DateTime currentFileDate;
        private int counter;
        private bool initialized;

        public FileLogHandler(StreamWriter stream)
        {
            this.stream = stream;
            setDefaultBehaviour();
        }

        public FileLogHandler(string filename)
        {
            this.original_filename = filename;
            setDefaultBehaviour();
            //OpenStream();
        }

        protected void OpenStream()
        {
            if (original_filename == null) return; //- File opened with stream, cannot reopen
            if(this.stream != null){
                this.stream.Close();
                this.stream = null;
            }
            string filename = original_filename.Replace("{date}", DateTime.UtcNow.Date.ToString("yyMMdd"));
            currentFileDate = DateTime.UtcNow.Date;

            if (!Directory.Exists(Path.GetDirectoryName(filename))) 
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
            try
            {
                this.stream = new StreamWriter(filename, true);
            }
            catch(Exception){
            
            }
        }

        protected void setDefaultBehaviour()
        {
            this.counter = 0;
            this.initialized = false;
            this.DatePrefix = "[";
            this.DateSuffix = "] ";
            this.DateFormat = "yy-MM-dd";
            this.TimeFormat = "HH:mm:ss.fff";
            this.Mode = LogDisplay.Date | LogDisplay.Time | LogDisplay.Debug | LogDisplay.Warning | LogDisplay.Error;
        }

        public override void WriteLine(MessageType type, string message)
        {
            if (!initialized)
            {
                OpenStream();
                initialized = true;
            }
            if (stream != null)
            {
                if (currentFileDate.Date < DateTime.UtcNow.Date) OpenStream();
                if (counter == 15)
                {
                    counter = 0;
                    flush();
                }
                ++counter;
                this.stream.WriteLine(message);
            }
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

        public override void flush()
        {
            if (stream != null)
            {
                this.stream.Flush();
            }
        }

        public override void close()
        {
            if (stream != null)
            {
                this.stream.Close();
                this.stream.Dispose();
            }
        }
    }
}
