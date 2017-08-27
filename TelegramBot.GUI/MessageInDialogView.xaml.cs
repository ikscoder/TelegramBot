using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TelegramBot.DataConnection;

namespace TelegramBot.GUI
{
    /// <summary>
    /// Логика взаимодействия для MessageInDialogView.xaml
    /// </summary>
    public partial class MessageInDialogView
    {
        public Telegram.Bot.Types.Message Message { get; }
        public MessageInDialogView(Telegram.Bot.Types.Message message, bool isYour)
        {
            Message = message;
            InitializeComponent();
            var photoid = Data.Current.GetPhotoFromMessageAsync(message).Result;
            if (photoid != null)
            {
                try
                {
                    var photo = App.Bot.GetFileAsync(photoid.FileId).Result;

                    var img = new BitmapImage();
                    img.BeginInit();
                    img.StreamSource = photo.FileStream;
                    img.EndInit();
                    MessageLabel.Content = new Image { Source = img };

                }
                catch (Exception e)
                {
                    GUI.Message.Show(e.Message + e.InnerException?.Message);
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
                //MessageBorder.Margin=new Thickness(150, 5,5,5);
            }
        }

    }

}
