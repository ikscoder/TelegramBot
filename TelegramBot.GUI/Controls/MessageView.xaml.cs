using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TelegramBot.DataConnection;

namespace TelegramBot.GUI
{
    /// <summary>
    /// Логика взаимодействия для MessageInDialogView.xaml
    /// </summary>
    public partial class MessageView
    {
        public Telegram.Bot.Types.Message Message { get; }
        public MessageView(Telegram.Bot.Types.Message message, bool isYour)
        {
            Message = message;
            InitializeComponent();
            var photoid = Data.Current.GetPhotoFromMessageAsync(message).Result;
            if (photoid?.FileId != null)
            {
                try
                {
                    var photo = App.Bot.GetFileAsync(photoid.FileId).Result;

                    var img = new BitmapImage();
                    img.BeginInit();
                    img.StreamSource = photo.FileStream;
                    img.EndInit();
                    MessageLabel.Content = new Image { Source = img, Stretch = Stretch.Uniform, MaxWidth = 400 };

                }
                catch
                {
                    MessageLabel.Content ="{Ссылка на файл уже не действительна}";
                }
            }
            if (!string.IsNullOrWhiteSpace(message.Sticker?.FileId))
            {
                MessageLabel.Content = "{Стикер}";
            }
            if (isYour)
            {
                Pane.FlowDirection = FlowDirection.LeftToRight;
                MessageLabel.SetResourceReference(ForegroundProperty, "TextOnLightColor");
                MessageBorder.SetResourceReference(BackgroundProperty, "AlternativeBackgroundColor");
                MessageBorder.BorderThickness = new Thickness(1);
            }
            else
            {
                Pane.FlowDirection = FlowDirection.RightToLeft;
                MessageLabel.SetResourceReference(ForegroundProperty, "TextOnDarkColor");
                MessageBorder.SetResourceReference(BackgroundProperty, "PrimaryLightColor");
            }
        }

        private void CopyMessage(object sender, RoutedEventArgs e)
        {
            if(Message.Text!=null)
                Clipboard.SetText(Message.Text);
            if(MessageLabel.Content is Image)
                Clipboard.SetImage(((Image)MessageLabel.Content).Source as BitmapImage);
        }


        private async void DeleteMessage(object sender, RoutedEventArgs e)
        {
            if (await App.Bot.DeleteMessageAsync(Message.Chat.Id, Message.MessageId))
                await Data.Current.DeleteMessageAsync(Message.MessageId);
        }
    }

}
