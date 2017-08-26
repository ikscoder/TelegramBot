using System.Windows;
using System.Windows.Controls;

namespace TelegramBot.GUI
{
    /// <summary>
    /// Логика взаимодействия для MessageInDialogView.xaml
    /// </summary>
    public partial class MessageInDialogView : UserControl
    {
        public Telegram.Bot.Types.Message Message { get; }
        public MessageInDialogView(Telegram.Bot.Types.Message message, bool isYour)
        {
            Message = message;   
            InitializeComponent();
            if (isYour)
            {
                MessageLabel.SetResourceReference(ForegroundProperty, "TextOnLightColor");
                MessageBorder.SetResourceReference(BackgroundProperty, "AlternativeBackgroundColor");
                MessageBorder.BorderThickness = new Thickness(1);
            }
            else
            {
                MessageLabel.SetResourceReference(ForegroundProperty, "TextOnDarkColor");
                MessageBorder.SetResourceReference(BackgroundProperty, "PrimaryLightColor");
                MessageBorder.Margin=new Thickness(150, 5,5,5);
            }
        }
    }
}
