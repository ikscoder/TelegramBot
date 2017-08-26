using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Navigation;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.DataConnection;

namespace TelegramBot.GUI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public volatile bool IsStop;

        public MainWindow()
        {
            InitializeComponent();
            PopSettings.Closed += (s, e) => { TabControlMain.Effect = null; };
            Settings.Load();
            if (Data.InitConnection(Settings.Current.ConnectionString)) return;
            Message.Show("Cannot connect to database");
            Close();
        }

        private async void ShedulingUpdatingAsync()
        {
            await Task.Run(async () =>
            {
                while (!IsStop)
                {
                    try
                    {
                        await Dispatcher.BeginInvoke(new Action(UpdateView));
                    }
                    catch { }
                    Thread.Sleep(1500);
                }
            });
        }

        private async void UpdateView()
        {
            MessagesList.ItemsSource = await Data.Current.GetAllMessagesAsync();
            ChatList.ItemsSource = await Data.Current.GetOpenedChatsAsync();
            ManagerChatList.ItemsSource = await Data.Current.GetManagerChatsAsync();
            ManagersList.ItemsSource = await Data.Current.GetAllManagerIdsAsync();
            TelegramManager.ItemsSource = (await Data.Current.GetTelegramManagerTableAsync()).DefaultView;
        }

        #region GUI

        private void BExit_Click(object sender, RoutedEventArgs e)
        {
            IsStop = true;
            Settings.Save();
            Application.Current.Shutdown();
        }

        private void BMaximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void BMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BSettings_Click(object sender, RoutedEventArgs e)
        {
            TabControlMain.Effect = new BlurEffect();
            PopSettings.IsOpen = true;
        }

        public void Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            Opacity = 0.5;
            DragMove();
            Opacity = 1;
        }

        private void BAlwaysOnTop_Click(object sender, RoutedEventArgs e)
        {
            BAlwaysOnTop.RenderTransform = Topmost ? null : new RotateTransform { Angle = -45 };
            Topmost = !Topmost;
        }

        #endregion

        private void ChatList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(!(ChatList.SelectedItem is Chat))return;
            new Dialog((Chat)ChatList.SelectedItem).Show();
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            ShedulingUpdatingAsync();      
        }

        private void GenerateToken_Click(object sender, RoutedEventArgs e)
        {
            if (!(ManagerId.Content is int))
            {
                Message.Show("Надо выбрать мэнеджера");
                return;
            }
            string t = RandomString(5);
            TokenText.Text = t;
            Data.Current.SetToken((int)ManagerId.Content, t);
        }


        private static readonly Random Random = new Random();
        private static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void ManagerChat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(ManagerChatList.SelectedItem is Chat)) return;
            new Dialog((Chat)ManagerChatList.SelectedItem).Show();
        }

        private async void SendAllManagers_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(AllManText.Text))return;

            foreach (var chat in await Data.Current.GetManagerChatsAsync())
            {
                Data.Current.InsertMessage(new Telegram.Bot.Types.Message
                {
                    MessageId = -1,
                    Date = DateTime.Now,
                    Chat = chat,
                    From = await Data.Current.GetBotAsync(),
                    Text = AllManText.Text
                }, false);     
            }
            AllManText.Text = null;
        }

        private void ManagersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TokenText.Text = null;
        }
    }

}
