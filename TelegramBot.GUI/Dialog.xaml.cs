using System;
using System.Collections.Generic;
using System.Linq;
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

        public List<Telegram.Bot.Types.Message> Messages { get; }=new List<Telegram.Bot.Types.Message>();

        public User Bot { get; set; }

        public Dialog(Chat chat)
        {
            Chat = chat;
            Bot = Data.Current.GetBotAsync().Result;
            InitializeComponent();
            Media.MediaEnded += (sender, e) => { Media.Position = new TimeSpan(0);Media.Pause(); };
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
                //ChatPanel.Children.Clear();
                messages=messages.Skip(Messages.Count).ToList();
                if (Messages.Any() && messages.Any(x => x.From.Id != Bot?.Id))
                    Media.Play();
                Messages.AddRange(messages);
                foreach (var mes in messages)
                {
                    if(mes?.From?.Id == null)continue;
                    ChatPanel.Children.Add(new MessageInDialogView(mes, mes.From.Id == Bot?.Id));
                }
                if(messages.Any())
                    ChatScroll.ScrollToEnd();        
            }
            catch(Exception e)
            {
                //Message.Show(e.Message);
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
