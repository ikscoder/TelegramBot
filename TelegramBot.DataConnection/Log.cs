using System;
using System.IO;
using System.Text;

namespace TelegramBot.DAL
{
    public static class Log
    {
        public static void Add(LogMessage mes)
        {
            if (!Directory.Exists("Logs")) Directory.CreateDirectory("Logs");
            using (var sw = new StreamWriter("Logs/log " + DateTime.Now.ToString("yyyy-MM-dd") + ".txt", true, Encoding.UTF8))
            {
                sw.Write(mes.Time.ToString("H:mm:ss") + "\t" + mes.Type + "\t" + mes.Message + "\n");
            }
        }

        public enum MessageType
        {
            OK,
            ERROR,
            WARNING
        }

        public class LogMessage
        {
            public LogMessage(DateTime time, MessageType type, string message)
            {
                Time = time;
                Type = type;
                Message = message;
            }

            public LogMessage(MessageType type, string message)
            {
                Time = DateTime.Now;
                Type = type;
                Message = message;
            }

            public DateTime Time { get; set; }
            public MessageType Type { get; set; }
            public string Message { get; set; }
        }
    }
}
