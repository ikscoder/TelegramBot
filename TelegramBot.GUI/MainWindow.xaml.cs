using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Navigation;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.DataConnection;

namespace TelegramBot.GUI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Для скачивания фалов из телеграма
        public volatile bool IsStop;

        public MainWindow()
        {
            //if (!Directory.Exists("Files")) Directory.CreateDirectory("Files");
            if (!(Process.GetProcesses().Any(proc => proc.ProcessName == "TelegramBot")))
            {
                Process.Start("TelegramBot.exe");
            }         
            Settings.Load();
            BotSettings.Load();
            App.Bot=new TelegramBotClient(BotSettings.Current.APIKey);
            InitializeComponent();

            foreach (var com in BotSettings.Current.AvailableCommands)
            {
                AvailableCommandsPanel.Children.Add(new CheckBox
                {
                    Content = com.Key,
                    IsChecked = com.Value,
                    Margin = new Thickness(5)
                });
            }
            PopSettings.Closed += (s, e) => { TabControlMain.Effect = null; };
            if (Data.InitConnection(BotSettings.Current.ConnectionString)) return;
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
            ChatList.ItemsSource = await Data.Current.GetOpenedDialogChatsAsync();
            ManagerChatList.ItemsSource = await Data.Current.GetManagerChatsAsync();
            ManagersList.ItemsSource = await Data.Current.GetAllManagerIdsAsync();
            if (!(Process.GetProcesses().Any(proc => proc.ProcessName == "TelegramBot")))
            {
                Process.Start("TelegramBot.exe");
                Message.Show("Бот упал и был перезапущен");
            }
  
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

        private async void window_Loaded(object sender, RoutedEventArgs e)
        {
            ListTables.ItemsSource = await Data.Current.GetTablesAsync();
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
            if (CheckManager.IsChecked != true && CheckClient.IsChecked != true)return;
            
            if (string.IsNullOrEmpty(TextMessage.Text?.Trim()))return;

            var chats = new List<Chat>();
            if(CheckManager.IsChecked==true)
                chats.AddRange(await Data.Current.GetManagerChatsAsync());
            if(CheckClient.IsChecked==true)
                chats.AddRange(await Data.Current.GetAllChatsAsync());
            if(chats.Any())
                foreach (var chat in chats.GroupBy(p => p.Id).Select(g => g.First()))
                    {
                        Data.Current.InsertMessage(new Telegram.Bot.Types.Message
                        {
                            MessageId = -1,
                            Date = DateTime.Now,
                            Chat = chat,
                            From = await Data.Current.GetBotAsync(),
                            Text = TextMessage.Text?.Trim()
                        }, false);     
                    }

            TextMessage.Text = null;
        }

        private void ManagersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TokenText.Text = null;
        }

        private async void Refresh_OnClick(object sender, RoutedEventArgs e)
        {
            if(ListTables.SelectedItem == null)return;
            DBTable.ItemsSource = (await Data.Current.GetTableByNameAsync(ListTables.SelectedItem.ToString())).DefaultView;
        }

        private async void ListTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListTables.SelectedItem == null) return;
            DBTable.ItemsSource = (await Data.Current.GetTableByNameAsync(ListTables.SelectedItem.ToString())).DefaultView;
        }

        private void SetBotSettings_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in AvailableCommandsPanel.Children)
            {
                var cb = child as CheckBox;
                if (!string.IsNullOrEmpty(cb?.Content.ToString()))
                {
                    BotSettings.Current.AvailableCommands[cb.Content.ToString()] = cb.IsChecked ?? false;
                }
            }
            BotSettings.Save();
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }

}
