using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Telegram.Bot.Types;
using TelegramBot.DAL;

namespace TelegramBot.GUI
{
    /// <summary>
    /// Логика взаимодействия для ChatView.xaml
    /// </summary>
    public partial class ChatView
    {
        public volatile bool IsStop;
        private Chat _currentChat;

        public Chat CurrentChat
        {
            get { return _currentChat; }
            set
            {
                if(_currentChat == value) return;
                _currentChat = value;
                Visibility = _currentChat == null ? Visibility.Hidden : Visibility.Visible;
                Messages.Clear();
                ChatPanel.Children.Clear();
                UpdateView();
            }
        }

        public List<Telegram.Bot.Types.Message> Messages { get; } = new List<Telegram.Bot.Types.Message>();
        public ChatView()
        {
            InitializeComponent();
            Media.MediaEnded += (sender, e) => { Media.Position = new TimeSpan(0); Media.Pause(); };
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ChatTextMessage.Text) || CurrentChat == null) return;
            Data.Current.InsertMessage(new Telegram.Bot.Types.Message
            {
                MessageId = -1,
                Date = DateTime.Now,
                Chat = CurrentChat,
                From = App.BUser,
                Text = ChatTextMessage.Text
            }, false);

            ChatTextMessage.Text = "";
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            StartUpdating();
        }

        private async void StartUpdating()
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
                if(CurrentChat==null)return;
                var messages = await Data.Current.GetMessagesFromChatAsync(CurrentChat);
                messages = messages.Skip(Messages.Count - ChatPanel.Children.Cast<UIElement>().Count(x => x.Visibility == Visibility.Collapsed)).ToList();
                if (Messages.Any() && messages.Any(x => x.From.Id != App.BUser?.Id))
                    Media.Play();
                Messages.AddRange(messages);
                foreach (var mes in messages)
                {
                    if (mes?.From?.Id == null) continue;
                    ChatPanel.Children.Add(new MessageView(mes, mes.From.Id == App.BUser?.Id));
                }
                if (messages.Any())
                    ChatScroll.ScrollToEnd();
            }
            catch
            {

            }
        }
    }
}
