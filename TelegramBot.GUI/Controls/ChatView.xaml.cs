using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using Telegram.Bot.Types;
using TelegramBot.DataConnection;

namespace TelegramBot.GUI
{
    /// <summary>
    /// Логика взаимодействия для ChatView.xaml
    /// </summary>
    public partial class ChatView
    {
        public Chat Chat { get; set; }
        public ChatView(Chat chat,bool isClient=true)
        {
            Chat = chat;
            InitializeComponent();
            if (!isClient)
            {
                LabelPath.SetResourceReference(Path.DataProperty, "Manager");
                LabelPath.SetResourceReference(Shape.FillProperty, "TextOnDarkColor");
            }
            else
            {
                var item = new MenuItem
                {
                    Header = "Закрыть чат",
                };
                item.Click +=(s,e) => { Data.Current.InsertOrUpdateChat(Chat, isDialogOpened: false); };
                ContextMenu =new ContextMenu
                {
                    Items = { item }
                };
            }
            Label.Content = string.IsNullOrWhiteSpace(Chat.LastName + Chat.FirstName)
                    ? Chat.Username ?? Chat.Id.ToString()
                    : Chat.LastName + " " + Chat.FirstName;
        }
    }
}
