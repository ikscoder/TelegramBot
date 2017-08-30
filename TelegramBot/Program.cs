using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TelegramBot.DataConnection;

namespace TelegramBot
{
    internal class Program
    {
        public static volatile bool IsStop;
        private static void Main()
        {
            Settings.Load();
            if(!Data.InitConnection(Settings.Current.ConnectionString))
                return;
            if (!TBot.Load())
                return;
            TBot.Bot.StartReceiving();
            ShedulingSendingAsync();
            var icon = new NotifyIcon
            {
                Icon = new Icon(typeof(Program), "Robot.ico"),
                Text = TBot.Bot.GetMeAsync().Result.Username,
                Visible = true
            };
            var exit = new MenuItem { Text = "Выход" };
            exit.Click += (s, e) =>
            {
                TBot.Bot.StopReceiving();
                Data.Current.Dispose();
                icon.Visible = false;
                IsStop = true;
                Environment.Exit(0);
            };
            icon.ContextMenu = new ContextMenu(new[] { exit });
            Application.Run();

            Log.Add(new Log.LogMessage(Log.MessageType.OK, "Bot started"));

            while (true);

        }


        private static async void ShedulingSendingAsync()
        {
            await Task.Run(() =>
            {
                while (!IsStop)
                {
                    try
                    {
                        SendUnsended();
                    }
                    catch (Exception e){ Log.Add(new Log.LogMessage(Log.MessageType.ERROR, e.Message));}
                    Thread.Sleep(1000);
                }
            });
        }

        private static async void SendUnsended()
        {
            TBot.SendMessages(await Data.Current.GetUnsendedMessages());
        }

    }
}

