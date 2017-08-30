using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Telegram.Bot.Types;
using TelegramBot.DataConnection;

namespace TelegramBot.GUI
{
    public partial class MainWindow
    {
        public Chat CurrentChat { get; set; }
        public User Bot { get; set; }

        public volatile bool IsStop;
        public MainWindow()
        {

            InitializeComponent();
            ThemeBox.SelectionChanged += ThemeChange;
            ThemeBox.ItemsSource = Enum.GetValues(typeof(Settings.Theme)).Cast<Settings.Theme>();
            ThemeBox.SelectedItem = Settings.Current.AppTheme;
            LangBox.SelectionChanged += LanguageChange;
            LangBox.ItemsSource = Enum.GetValues(typeof(Settings.Language)).Cast<Settings.Language>();
            LangBox.SelectedItem = Settings.Current.AppLanguage;
            foreach (var com in BotSettings.Current.AvailableCommands)
            {
                AvailableCommandsPanel.Children.Add(new CheckBox
                {
                    Content = com.Key,
                    IsChecked = com.Value,
                    Margin = new Thickness(5)
                });
            }
            if (Data.InitConnection(BotSettings.Current.ConnectionString)) return;
            Message.Show("Cannot connect to database");
            Close();
        }

        #region GUI 

        private void BExit_Click(object sender, RoutedEventArgs e)
        {
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

        private void ThemeChange(object sender, SelectionChangedEventArgs e)
        {
            Settings.Current.AppTheme = (Settings.Theme)Enum.Parse(typeof(Settings.Theme), ThemeBox.SelectedItem.ToString());//(Settings.Theme)ThemeBox.SelectedItem;
            CustomThemePicker.Visibility = Settings.Current.AppTheme == Settings.Theme.Custom ? Visibility.Visible : Visibility.Hidden;
            PopSettings.Width = Settings.Current.AppTheme == Settings.Theme.Custom ? 700 : 400;
        }

        private void LanguageChange(object sender, SelectionChangedEventArgs e)
        {
            Settings.Current.AppLanguage = (Settings.Language)LangBox.SelectedItem;
        }
        private void PopupClose_OnClick(object sender, RoutedEventArgs e)
        {
            PopSettings.IsOpen = false;
        }

        #region ResizeWindows

        private bool _resizeInProcess;


        private void Resize_Init(object sender, MouseButtonEventArgs e)
        {
            var senderRect = sender as Rectangle;
            if (senderRect == null) return;
            _resizeInProcess = true;
            senderRect.CaptureMouse();
        }

        private void Resize_End(object sender, MouseButtonEventArgs e)
        {
            var senderRect = sender as Rectangle;
            if (senderRect == null) return;
            _resizeInProcess = false;
            senderRect.ReleaseMouseCapture();
        }

        private void Resizeing_Form(object sender, MouseEventArgs e)
        {
            const int step = 1;
            if (!_resizeInProcess) return;
            var senderRect = sender as Rectangle;
            var mainWindow = senderRect?.Tag as Window;
            if (mainWindow == null) return;
            double width = e.GetPosition(mainWindow).X;
            double height = e.GetPosition(mainWindow).Y;
            senderRect.CaptureMouse();
            if (senderRect.Name.ToLower().Contains("right"))
            {
                width += step;
                if (width > 0)
                    mainWindow.Width = width;
            }
            if (senderRect.Name.ToLower().Contains("left") && mainWindow.Width - width - step >= MinWidth)
            {

                width -= step;
                mainWindow.Left += width;
                width = mainWindow.Width - width;
                if (width > 0)
                {
                    mainWindow.Width = width;
                }
            }
            if (senderRect.Name.ToLower().Contains("bottom"))
            {
                height += step;
                if (height > 0)
                    mainWindow.Height = height;
            }
            if (senderRect.Name.ToLower().Contains("top") && mainWindow.Height - height - step > MinHeight)
            {
                height -= step;
                mainWindow.Top += height;
                height = mainWindow.Height - height;
                if (height > 0)
                {
                    mainWindow.Height = height;
                }
            }
        }
        #endregion


        private void OnMenuOpening(object sender, object e)
        {
            BodyGrid.Effect = new BlurEffect { Radius = 5 };
        }

        private void OnMenuClosing(object sender, object e)
        {
            BodyGrid.Effect = null;
        }
        #endregion

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
            Chats.Children.Clear();
            var chats = new List<ChatView>();
            if(ClientCheckChat.IsChecked==true)
                chats.AddRange(from t in await Data.Current.GetOpenedDialogChatsAsync() select new ChatView(t){Margin = new Thickness(5)});
            if (ManagerCheckChat.IsChecked == true)
                chats.AddRange(from t in await Data.Current.GetManagerChatsAsync() select new ChatView(t,false) { Margin = new Thickness(5) });
            chats.ForEach(x=> {
                if(CurrentChat?.Id==x.Chat?.Id) x.IsChecked.Visibility = Visibility.Visible;
                x.MouseDown+= (s, e) =>
                {
                    CurrentChat = x.Chat;
                };
                Chats.Children.Add(x); });
            CChatView.Visibility = CurrentChat == null ? Visibility.Hidden : Visibility.Visible;
            ManagersList.ItemsSource = await Data.Current.GetAllManagerIdsAsync();
            try
            {
                ChatPanel.Children.Clear();
                if (CurrentChat != null)
                {
                    var messages = await Data.Current.GetMessagesFromChatAsync(CurrentChat);
                    foreach (var mes in messages)
                    {
                        if (mes?.From?.Id == null) continue;
                        ChatPanel.Children.Add(new MessageView(mes, mes.From.Id == Bot?.Id));
                    }
                }
            }
            catch
            {

            }
            if (!(Process.GetProcesses().Any(proc => proc.ProcessName == "TelegramBot")))
            {
                Process.Start("TelegramBot.exe");
                Message.Show("Бот упал и был перезапущен");
            }

        }

        private async void window_Loaded(object sender, RoutedEventArgs e)
        {
            ListTables.ItemsSource = await Data.Current.GetTablesAsync();
            Bot = Data.Current.GetBotAsync().Result;
            ShedulingUpdatingAsync();
        }

        private void GenerateToken_Click(object sender, RoutedEventArgs e)
        {
            if (!(ManagersList.SelectedItem is int))
            {
                Message.Show("Надо выбрать мэнеджера");
                return;
            }
            string t = RandomString(5);
            TokenText.Text = t;
            Data.Current.SetToken((int)ManagersList.SelectedItem, t);
            Clipboard.SetText(t);
            IsClipboarded.IsOpen = true;
        }


        private static readonly Random Random = new Random();
        private static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        private async void SendAllManagers_Click(object sender, RoutedEventArgs e)
        {
            if (CheckManager.IsChecked != true && CheckClient.IsChecked != true) return;

            if (string.IsNullOrEmpty(TextMessage.Text?.Trim())) return;

            var chats = new List<Chat>();
            if (CheckManager.IsChecked == true)
                chats.AddRange(await Data.Current.GetManagerChatsAsync());
            if (CheckClient.IsChecked == true)
                chats.AddRange(await Data.Current.GetAllChatsAsync());
            if (chats.Any())
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
            if (ListTables.SelectedItem == null) return;
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

        private void TabChange(object sender, RoutedEventArgs e)
        {
            if (!((sender as MenuItem)?.Tag is string)) return;
            int.TryParse((string)((FrameworkElement) sender).Tag, out int j);
            TabControlMain.SelectedIndex = j;
        }

        private void NewTokenOpen(object sender, ExecutedRoutedEventArgs e)
        {
            GenerateTokenView.IsOpen = true;
        }

        private void NewSpam(object sender, ExecutedRoutedEventArgs e)
        {
            SendSpam.IsOpen = true;
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ChatTextMessage.Text)|| CurrentChat == null) return;
            Data.Current.InsertMessage(new Telegram.Bot.Types.Message
            {
                MessageId = -1,
                Date = DateTime.Now,
                Chat = CurrentChat,
                From = Bot,
                Text = ChatTextMessage.Text
            }, false);

            TextMessage.Text = null;
        }

        private void BOpenChat_Click(object sender, RoutedEventArgs e)
        {
            if(CurrentChat!=null)new Dialog(CurrentChat).Show();
        }
    }
}
