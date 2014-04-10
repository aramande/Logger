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

        public class LogHandlerCollection : List<ILogHandler>, IDictionary<string, ILogHandler> 
        {
            Dictionary<string, int> innerHashtable = new Dictionary<string, int>();
            public void Add(string key, ILogHandler value)
            {
                base.Add(value);
                innerHashtable.Add(key, base.Count - 1);
            }

            public bool ContainsKey(string key)
            {
                return innerHashtable.ContainsKey(key);
            }

            public ICollection<string> Keys
            {
                get { return innerHashtable.Keys; }
            }

            public bool Remove(string key)
            {
                if (ContainsKey(key))
                {
                    int i = innerHashtable[key];
                    base[i] = null;
                    innerHashtable.Remove(key);
                    return true;
                }
                return false;
            }

            public bool TryGetValue(string key, out ILogHandler value)
            {
                value = null;
                try{
                    value = base[innerHashtable[key]];
                    return true;
                }
                catch(Exception){
                    return false;
                }
            }

            public ICollection<ILogHandler> Values
            {
                get { return this; }
            }

            public ILogHandler this[string key]
            {
                get { return base[innerHashtable[key]]; }
                set { base[innerHashtable[key]] = value; }
            }

            public void Add(KeyValuePair<string, ILogHandler> item)
            {
                this.Add(item.Key, item.Value);
            }

            public bool Contains(KeyValuePair<string, ILogHandler> item)
            {
                return innerHashtable.ContainsKey(item.Key) && base.Contains(item.Value);
            }

            public void CopyTo(KeyValuePair<string, ILogHandler>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(KeyValuePair<string, ILogHandler> item)
            {
                bool result = Remove(item.Key);
                return true;
            }

            public new IEnumerator<KeyValuePair<string, ILogHandler>> GetEnumerator()
            {
                return new Dictionary<string, ILogHandler>.Enumerator();
            }
        }
        LogHandlerCollection m_handlers = new LogHandlerCollection();
        private int logMsgCount;
        //List<ILogHandler> m_handlers = new List<ILogHandler>();
        public LogHandlerCollection Handlers { get { return m_handlers; } }


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

    [Obsolete("This logger has been outdated, please use Log instead.")]
    public class Logger
    {
        //private string filename = "";
        private StreamWriter stream = null;
        private string timestamp { get { return DateTime.Now.ToString(m_timeFormat); } }
        private bool prevWasNoLine = false;
        private bool includeTimestamp;
        private bool writeToConsole;
        private bool streamOpen = false;
        
        private string m_filename;
        private string m_path;
        private string m_dateFormat;
        private string m_timeFormat;
        public delegate void Callback(string message);

        public List<Callback> WriteLineCallback;
        public List<Callback> WriteCallback;
        public enum Mode
        {
            Output, Warning, Error
        }
        public Logger(string filename, string path="", bool timestamp = true, string dateFormat="yyMMdd", string timeFormat = "HHmmss.fff", bool console = true)
        {
            m_filename = filename;
            m_path = path;
            m_dateFormat = dateFormat;
            m_timeFormat = timeFormat;
            includeTimestamp = timestamp;
            writeToConsole = console;
            WriteCallback = new List<Callback>();
            WriteLineCallback = new List<Callback>();
        }
        ~Logger()
        {
            close();
        }
        public void open()
        {
            if (!m_filename.Equals("") && !streamOpen)
            {
                try
                {
                    if (!m_path.Equals("") && !Directory.Exists(m_path))
                    {
                        Directory.CreateDirectory(m_path);
                    }
                    FileStream fs = new FileStream(m_path + DateTime.UtcNow.ToString(m_dateFormat) + m_filename, FileMode.Append, FileAccess.Write);
                    stream = new StreamWriter(fs, Encoding.Unicode);
                    //stream.WriteLine("---------------------------");
                    streamOpen = true;
                }
                catch (FileLoadException)
                {
                    stream = null;
                    msg("Couldn't load file '" + m_path + DateTime.UtcNow.ToString(m_dateFormat) + m_filename + "'", Mode.Error);
                }
                catch (IOException)
                {
                    stream = null;
                    msg("Couldn't create file '" + m_path + DateTime.UtcNow.ToString(m_dateFormat) + m_filename + "'", Mode.Error);
                }
            }
        }

        public void close()
        {
            if (stream != null && streamOpen)
            {
                try
                {
                    stream.Flush();
                    stream.Close();
                    stream.Dispose();
                    streamOpen = false;
                }
                catch(ObjectDisposedException e)
                {
                    Console.WriteLine(e.Message);
                }
                stream = null;
            }
        }
        public void flush()
        {
            stream.Flush();
        }

        private string prefix(Mode mode)
        {
            StringBuilder builder = new StringBuilder();
            if (!prevWasNoLine)
            {
                if (includeTimestamp)
                {
                    builder.Append(timestamp);
                    builder.Append(">> ");
                }

                builder.Append(mode.ToString());
                builder.Append(": ");
            }
            return builder.ToString();
        }
        private void WriteLine(char data)
        {
            WriteLine(data.ToString());
        }
        private void WriteLine(string data)
        {
            if (stream != null)
            {
                //byte[] bytes = new byte[data.Length * sizeof(char)];
                //Buffer.BlockCopy(data.ToCharArray(), 0, bytes, 0, bytes.Length);
                //stream.Write(bytes,0, data.Length);
                stream.WriteLine(data);
            }
            if (writeToConsole)
            {
                Console.WriteLine(data);
            }
            foreach (var call in WriteLineCallback)
            {
                call(data);
            }
        }
        private void Write(char data)
        {
            Write(data.ToString());
        }
        private void Write(string data)
        {
            if (stream != null)
            {
                //byte[] bytes = new byte[data.Length * sizeof(char)];
                //Buffer.BlockCopy(data.ToCharArray(), 0, bytes, 0, bytes.Length);
                //stream.Write(bytes,0, data.Length);
                stream.Write(data);
            }
            if (writeToConsole)
            {
                Console.Write(data);
            }
            foreach (var call in WriteCallback)
            {
                call(data);
            }
        }

        //Writes a symbol to the a line, by default without creating a newline character
        [Obsolete("This logger has been outdated, please use Logger2 instead.")]
        public void msg(char symbol, Mode mode = Mode.Output, bool newLine=false)
        {
            string message = symbol.ToString();
            if (!prevWasNoLine) message = prefix(mode) + message;
            if (newLine)
            {
                WriteLine(message);
                prevWasNoLine = false;
            }
            else
            {
                Write(message);
                prevWasNoLine = true;
            }
        }
        //Writes a whole line as a message
        [Obsolete("This logger has been outdated, please use Log instead.")]
        public void msg(String message, Mode mode = Mode.Output, bool newLine=true)
        {
            if (message.Length != 0) message = prefix(mode) + message; //Print a blank row if message is empty.
            if (newLine)
            {
                WriteLine(message);
                prevWasNoLine = false;
            }
            else
            {
                Write(message);
                prevWasNoLine = true;
            }
        }
    }
}
