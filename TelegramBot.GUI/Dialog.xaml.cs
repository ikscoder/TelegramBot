using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Telegram.Bot.Types;

namespace TelegramBot.GUI
{
    /// <summary>
    /// Логика взаимодействия для Dialog.xaml
    /// </summary>
    public partial class Dialog
    {     
        public Dialog(Chat chat)
        {
            InitializeComponent();
            ChatView.CurrentChat = chat;
            Title = string.IsNullOrWhiteSpace(chat.LastName + chat.FirstName)
                ? chat.Username ?? chat.Id.ToString()
                : chat.LastName +" "+ chat.FirstName;
        }

        private void BAlwaysOnTop_Click(object sender, RoutedEventArgs e)
        {
            BAlwaysOnTop.RenderTransform = Topmost ? null : new RotateTransform { Angle = -45 };
            Topmost = !Topmost;
        }

        private void BExit_Click(object sender, RoutedEventArgs e)
        {
            ChatView.IsStop = true;
            Close();
        }

        public void Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            Opacity = 0.5;
            DragMove();
            Opacity = 1;
        }
    }
}
