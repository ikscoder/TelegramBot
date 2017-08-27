using System.Windows;
using Telegram.Bot;

namespace TelegramBot.GUI
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static TelegramBotClient Bot { get; set; }
    }
}
