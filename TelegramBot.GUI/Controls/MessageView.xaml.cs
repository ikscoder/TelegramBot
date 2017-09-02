using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using TelegramBot.DAL;

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

            #region Styling&CM

            var cm = new ContextMenu();
            var copyitem = new MenuItem { Header = "Копировать" };
            copyitem.Click += CopyMessage;
            cm.Items.Add(copyitem);
            if (isYour)
            {
                Pane.FlowDirection = FlowDirection.LeftToRight;
                MessageLabel.SetResourceReference(ForegroundProperty, "TextOnLightColor");
                MessageBorder.SetResourceReference(BackgroundProperty, "AlternativeBackgroundColor");
                MessageBorder.BorderThickness = new Thickness(1);
                var delitem = new MenuItem { Header = "Удалить" };
                delitem.Click += DeleteMessage;
                cm.Items.Add(delitem);
            }
            else
            {
                Pane.FlowDirection = FlowDirection.RightToLeft;
                MessageLabel.SetResourceReference(ForegroundProperty, "TextOnDarkColor");
                MessageBorder.SetResourceReference(BackgroundProperty, "PrimaryLightColor");
            }
            MessageLabel.ContextMenu = cm;

            #endregion

            if (message.Photo!=null)
            {
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
                
            }
            if (!string.IsNullOrWhiteSpace(message.Sticker?.FileId))
            {
                MessageLabel.Content = "{Стикер}";
            }
            if (message.Contact != null)
            {
                MessageLabel.Content =
                    $"Контакт:\n+{message.Contact.PhoneNumber}\n{message.From.LastName} {message.From.FirstName}";
        
            }
            if (message.Document != null)
            {
                var doc = Data.Current.GetDocumentAsync(message.Document.FileId).Result;
                Message.Document = doc;
                var p = new System.Windows.Shapes.Path
                {
                    Stretch = Stretch.Uniform
                };
                p.SetResourceReference(System.Windows.Shapes.Path.DataProperty, "Download");
                p.SetResourceReference(Shape.FillProperty, "SecondaryColor");
                var b=new Button();
                b.SetResourceReference(StyleProperty, "TranspButton");
                b.Content=new Border{Width = 40,Child = p};
                b.Click +=async (s, e) =>
                {
                    var d = new SaveFileDialog {FileName = doc.FileName};
                    if (d.ShowDialog() != true) return;

                    var file =await App.Bot.GetFileAsync(message.Document.FileId);
                    file.FileStream.CopyTo(d.OpenFile());
                };
                var dp=new DockPanel();
                DockPanel.SetDock(b,Dock.Right);
                dp.Children.Add(b);
                var l1=new Label{VerticalContentAlignment = VerticalAlignment.Center,HorizontalContentAlignment = HorizontalAlignment.Center,Content = doc?.FileName};
                l1.SetResourceReference(ForegroundProperty, "TextOnDarkColor");
                DockPanel.SetDock(l1, Dock.Top);
                dp.Children.Add(l1);
                var l2 = new Label { VerticalContentAlignment = VerticalAlignment.Center, HorizontalContentAlignment = HorizontalAlignment.Center, Content = $"{doc?.MimeType}\n{HRS(doc?.FileSize??0)}" };
                l2.SetResourceReference(FontSizeProperty, "FontSizeSmall");
                l2.SetResourceReference(ForegroundProperty, "TextOnDarkColor");
                DockPanel.SetDock(l2, Dock.Bottom);
                dp.Children.Add(l2);
                MessageLabel.Content = dp;
            }
            if (message.Text != null) MessageLabel.Content = message.Text;

        }

        private void CopyMessage(object sender, RoutedEventArgs e)
        {
            if(Message.Text!=null)
                Clipboard.SetText(Message.Text);
            if(Message.Contact!=null)
                Clipboard.SetText($"Контакт:\n+{Message.Contact.PhoneNumber}\n{Message.From.LastName} {Message.From.FirstName}");
            if(Message.Document!=null)
                Clipboard.SetText(Message.Document?.FileName);
            var image = MessageLabel.Content as Image;
            if (image?.Source != null) Clipboard.SetImage((BitmapImage)image.Source);
        }


        private async void DeleteMessage(object sender, RoutedEventArgs e)
        {
            try
            {
                if (await App.Bot.DeleteMessageAsync(Message.Chat.Id, Message.MessageId))
                {
                    await Data.Current.DeleteMessageAsync(Message.MessageId);
                    Visibility=Visibility.Collapsed;
                }
            }
            catch (Exception exception)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, exception.Message));
            }
        }

        private static string HRS(int fileSize)
        {
            string[] sizes = { "Byte", "KB", "MB", "GB", "TB" };
            double len = fileSize;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }

}
