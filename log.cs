using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace Lib1
{
    public class Log : IDisposable
    {
        //LogHandlerCollection m_handlers = new LogHandlerCollection();
        //public LogHandlerCollection Handlers { get { return m_handlers; } }
        private int logMsgCount;
        List<ILogHandler> m_handlers = new List<ILogHandler>();
        public List<ILogHandler> Handlers { get { return m_handlers; } }

        public void debug(string message)
        {
            format(MessageType.Debug, message);
        }

        public void warning(string message)
        {
            format(MessageType.Warning, message);
        }

        public void error(string message)
        {
            format(MessageType.Error, message);
        }
        private void format(MessageType prefix, string message)
        {
            for (int i = 0; i < m_handlers.Count; i++)
            {
                if (m_handlers[i].Mode == LogDisplay.None) continue;
                if (m_handlers[i].Mode.HasFlag((LogDisplay)prefix))
                {
                    StringBuilder builder = new StringBuilder();
                    if ((m_handlers[i].Mode & (LogDisplay.Date | LogDisplay.Time)) != 0)
                    {
                        builder.Append(m_handlers[i].DatePrefix);
                        if (m_handlers[i].Mode.HasFlag(LogDisplay.Date)) builder.Append(DateTime.UtcNow.ToString(m_handlers[i].DateFormat));
                        if ((m_handlers[i].Mode & (LogDisplay.Date | LogDisplay.Time)) == (LogDisplay.Date | LogDisplay.Time)) builder.Append(" ");
                        if (m_handlers[i].Mode.HasFlag(LogDisplay.Time)) builder.Append(DateTime.UtcNow.ToString(m_handlers[i].TimeFormat));
                        builder.Append(m_handlers[i].DateSuffix);
                    }
                    builder.Append(m_handlers[i].TypeFormat(prefix)); //"{0}: ", prefix.ToString().ToUpper());

                    builder.Append(message);
                    m_handlers[i].WriteLine(prefix, builder.ToString());
                }

                if (logMsgCount % 5 == 0)
                {
                    m_handlers[i].flush();
                    logMsgCount = 0;
                }
            }
            ++logMsgCount;
        }

        public void Dispose()
        {
            for (int i = 0; i < m_handlers.Count; i++)
            {
                m_handlers[i].close();
            }
        }
    }
}
