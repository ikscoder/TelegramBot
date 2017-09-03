using System;
using System.Linq;
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

            var photoid = message.Photo?.FirstOrDefault(x => x.Width<300);
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
            if (message.Sticker!=null)
            {
                MessageLabel.Content = "{Стикер}";
            }
            if (message.Audio != null)
            {
                MessageLabel.Content = "{Audio}";
            }
            if (message.Video != null)
            {
                MessageLabel.Content = "{Video}";
            }
            if (message.Contact != null)
            {
                MessageLabel.Content =
                    $"Контакт:\n+{message.Contact.PhoneNumber}\n{message.Contact.LastName} {message.Contact.FirstName}";
        
            }
            if (message.Document != null)
            {
                var p = new Path
                {
                    Stretch = Stretch.Uniform
                };
                p.SetResourceReference(Path.DataProperty, "Download");
                p.SetResourceReference(Shape.FillProperty, "SecondaryColor");
                var b=new Button();
                b.SetResourceReference(StyleProperty, "TranspButton");
                b.Content=new Border{Width = 40,Child = p};
                b.Click +=async (s, e) =>
                {
                    var d = new SaveFileDialog {FileName = Message.Document.FileName};
                    if (d.ShowDialog() != true) return;

                    var file =await App.Bot.GetFileAsync(message.Document.FileId);
                    file.FileStream.CopyTo(d.OpenFile());
                };
                var dp=new DockPanel();
                DockPanel.SetDock(b,Dock.Right);
                dp.Children.Add(b);
                var l1=new Label{VerticalContentAlignment = VerticalAlignment.Center,HorizontalContentAlignment = HorizontalAlignment.Center,Content = Message.Document?.FileName};
                l1.SetResourceReference(ForegroundProperty, "TextOnDarkColor");
                DockPanel.SetDock(l1, Dock.Top);
                dp.Children.Add(l1);
                var l2 = new Label { VerticalContentAlignment = VerticalAlignment.Center, HorizontalContentAlignment = HorizontalAlignment.Center, Content = $"{Message.Document?.MimeType}\n{Hrs(Message.Document?.FileSize??0)}" };
                l2.SetResourceReference(FontSizeProperty, "FontSizeSmall");
                l2.SetResourceReference(ForegroundProperty, "TextOnDarkColor");
                DockPanel.SetDock(l2, Dock.Bottom);
                dp.Children.Add(l2);
                MessageLabel.Content = dp;
            }
            if (message.Venue != null)
            {
                var p = new Path
                {
                    Stretch = Stretch.Uniform
                };
                p.SetResourceReference(Path.DataProperty, "Map");
                p.SetResourceReference(Shape.FillProperty, "SecondaryColor");
                var b = new Button();
                b.SetResourceReference(StyleProperty, "TranspButton");
                b.Content = new Border { Width = 40, Child = p };
                b.Click += (s, e) =>
                {
                    App.Map.AddPoint(message.Venue);
                    App.Map.Show();
                };
                var dp = new DockPanel();
                DockPanel.SetDock(b, Dock.Right);
                dp.Children.Add(b);
                var l1 = new Label { VerticalContentAlignment = VerticalAlignment.Center, HorizontalContentAlignment = HorizontalAlignment.Center, Content = Message.Venue?.Title };
                l1.SetResourceReference(ForegroundProperty, "TextOnDarkColor");
                DockPanel.SetDock(l1, Dock.Top);
                dp.Children.Add(l1);
                var l2 = new Label { VerticalContentAlignment = VerticalAlignment.Center, HorizontalContentAlignment = HorizontalAlignment.Center, Content = Message.Venue?.Address };
                l2.SetResourceReference(FontSizeProperty, "FontSizeSmall");
                l2.SetResourceReference(ForegroundProperty, "TextOnDarkColor");
                DockPanel.SetDock(l2, Dock.Bottom);
                dp.Children.Add(l2);
                MessageLabel.Content = dp;
            }
            if (message.Location != null)
            {
                var p = new Path
                {
                    Stretch = Stretch.Uniform
                };
                p.SetResourceReference(Path.DataProperty, "Map");
                p.SetResourceReference(Shape.FillProperty, "SecondaryColor");
                var b = new Button();
                b.SetResourceReference(StyleProperty, "TranspButton");
                b.Content = new Border { Width = 40, Child = p };
                b.Click += (s, e) =>
                {
                    App.Map.AddPoint(message.Location);
                    App.Map.Show();
                };
                var dp = new DockPanel();
                DockPanel.SetDock(b, Dock.Right);
                dp.Children.Add(b);
                var l1 = new Label { VerticalContentAlignment = VerticalAlignment.Center, HorizontalContentAlignment = HorizontalAlignment.Center, Content = $"lat: {Message.Location?.Latitude} lng: {Message.Location?.Longitude}" };
                l1.SetResourceReference(ForegroundProperty, "TextOnDarkColor");
                DockPanel.SetDock(l1, Dock.Top);
                dp.Children.Add(l1);
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
            if(Message.Venue!=null)
                Clipboard.SetText(Message.Venue?.Address);
            if(Message.Location!=null)
                Clipboard.SetText($"lat: {Message.Location?.Latitude} lng: {Message.Location?.Longitude}");
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

        private static string Hrs(int fileSize)
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
