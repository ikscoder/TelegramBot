using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Telegram.Bot.Types;
using TelegramBot.DataConnection;

namespace TelegramBot.GUI
{
    /// <summary>
    /// Логика взаимодействия для Dialog.xaml
    /// </summary>
    public partial class Dialog : Window
    {
        public volatile bool IsStop;
        public Chat Chat { get; }
        public Dialog(Chat chat)
        {
            Chat = chat;
            InitializeComponent();
            ShedulingUpdating();
            
        }

        private async void ShedulingUpdating()
        {
            await Task.Run(async () =>
            {
                while (!IsStop)
                {
                    await Dispatcher.BeginInvoke(new Action(UpdateView));
                    Thread.Sleep(2000);
                }
            });
        }

        private async void UpdateView()
        {
            try
            {  
                var messages = await Data.Current.GetMessagesFromChatAsync(Chat);
                ChatPanel.Children.Clear();
                foreach (var mes in messages)
                {
                    if(mes?.From?.Id == null)continue;
                    ChatPanel.Children.Add(new MessageInDialogView(mes, mes.From.Id == (await Data.Current.GetBotAsync())?.Id));
                }
                ChatScroll.ScrollToEnd();
            }
            catch
            {
            }
        }

        private void BExit_Click(object sender, RoutedEventArgs e)
        {
            IsStop = true;
            Close();
        }

        public void Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            Opacity = 0.5;
            DragMove();
            Opacity = 1;
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(TextMessage.Text))return;
            Data.Current.InsertMessage(new Telegram.Bot.Types.Message
            {
                MessageId = -1,
                Date = DateTime.Now,
                Chat = Chat,
                From = await Data.Current.GetBotAsync(),
                Text = TextMessage.Text
            }, false);

            TextMessage.Text = null;
        }
    }
}
