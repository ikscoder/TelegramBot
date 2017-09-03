using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Telegram.Bot;
using TelegramBot.DAL;

namespace TelegramBot.GUI
{
    public partial class LoadingScreen
    {
        public LoadingScreen()
        {
            Settings.Load();
            InitializeComponent(); 
            Version.Content = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        }

        private async void LoadingScreen_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadingLabel.Content = "Загрузка настроек";
                if (!(Process.GetProcesses().Any(proc => proc.ProcessName == "TelegramBot")))
                {
                    Process.Start("TelegramBot.exe");
                }
                BotSettings.Load();
                App.Bot=new TelegramBotClient(BotSettings.Current.APIKey);
                App.BUser = await App.Bot.GetMeAsync();
                StartUpdating();
                App.Map = new MapWindow();

                LoadingLabel.Content = "Подключение к базе данных";
                if (!Data.InitConnection(BotSettings.Current.ConnectionString))
                    throw new Exception("Cannot connect to database");
                LoadingLabel.Content = "Загрузка главного окна";
                (await Data.Current.GetOfficesAsync()).ForEach(ven => App.Map.AddOffice(ven));
                var mw = new MainWindow {Visibility = Visibility.Visible };
                mw.Show();
                Hide();
                mw.Visibility = Visibility.Visible;
            }
            catch (Exception exception)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "Loading: " + exception.Message));
                Message.Show("Loading error: " + exception.Message, (string)Application.Current.Resources["LangError"]);
                Application.Current.Shutdown(1);
            }
        }

        private async void StartUpdating()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await Dispatcher.BeginInvoke(new Action(UpdateView));
                    }
                    catch { }
                    Thread.Sleep(5000);
                }
            });
        }

        private void Path_OnInitialized(object sender, EventArgs e)
        {
            var da = new DoubleAnimation(0, 359, new Duration(TimeSpan.FromMilliseconds(1200)));
            var rt = new RotateTransform();
            ((UIElement)sender).RenderTransform = rt;
            ((UIElement)sender).RenderTransformOrigin = new Point(0.5, 0.5);
            da.RepeatBehavior = RepeatBehavior.Forever;
            rt.BeginAnimation(RotateTransform.AngleProperty, da);
        }

        private static void UpdateView()
        {
            if (Process.GetProcesses().Any(proc => proc.ProcessName == "TelegramBot")) return;
            Process.Start("TelegramBot.exe");
            Message.Show("Бот упал и был перезапущен");
        }

    }
}
