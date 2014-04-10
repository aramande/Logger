using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lib1
{
    public abstract class LogHandler : ILogHandler
    {
        private LogDisplay m_mode = LogDisplay.Time | LogDisplay.Debug | LogDisplay.Warning | LogDisplay.Error;
        private string m_dateFormat = "yyMMdd";
        private string m_timeFormat = "HHmmss.fff";
        private string m_date_suffix = " > ";
        private string m_date_prefix = "";

        public LogDisplay Mode
        {
            get { return m_mode; }
            set { m_mode = value; }
        }

        public string DateFormat
        {
            get { return m_dateFormat; }
            set { m_dateFormat = value; }
        }

        public string TimeFormat
        {
            get { return m_timeFormat; }
            set { m_timeFormat = value; }
        }

        public string DatePrefix
        {
            get { return m_date_prefix; }
            set { m_date_prefix = value; }
        }

        public string DateSuffix
        {
            get { return m_date_suffix; }
            set { m_date_suffix = value; }
        }

        public abstract string TypeFormat(MessageType type);

        public abstract void WriteLine(MessageType type, string msg);


        public virtual void flush(){}
        public virtual void close(){}
    }
}
