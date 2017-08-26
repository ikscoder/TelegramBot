using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ImageProcessor.Plugins.WebP.Imaging.Formats;

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
            if (message.Sticker?.FileId != null)
            {
                MessageLabel.Content = "{Стикер}";
                try
                {
                    var sticker = MainWindow.Bot.GetFileAsync(message.Sticker.FileId).Result;
                    sticker.FileStream.Seek(0, SeekOrigin.Begin);
                    using (var ms=new MemoryStream())
                    {
                        var webp = new WebPFormat().Load(sticker.FileStream);   
                        webp.Save(ms, ImageFormat.MemoryBmp);
                        ms.Position = 0;
                        var img = new BitmapImage();
                        img.BeginInit();
                        img.StreamSource = ms;
                        img.EndInit();
                        MessageLabel.Content = new Image { Source = img };
                    }

                }
                catch (Exception e)
                {
                    GUI.Message.Show(e.Message + e.InnerException?.Message);
                }
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
