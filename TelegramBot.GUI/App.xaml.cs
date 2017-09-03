using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.GUI
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App
    {
        public static TelegramBotClient Bot { get; set; }
        public static User BUser { get; set; }

        public static MapWindow Map { get; set; }
    }
}
