using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TelegramBot.GUI
{
    public partial class WaitingButton
    {
        public enum WaitingPath { Wait1, Wait2, Wait3, Wait4, Wait5, Wait6, Wait7, Wait8, Wait9, Wait10, Wait11, Wait12, Wait13 };
        public WaitingButton()
        {
            InitializeComponent();
        }

        private static void OnPathChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            switch ((sender as WaitingButton)?.WaitingPathValue)
            {
                case WaitingPath.Wait1:
                    ((WaitingButton)sender).Waiting.Data = (Geometry)Application.Current.Resources["Wait1"];
                    break;
                case WaitingPath.Wait2:
                    ((WaitingButton)sender).Waiting.Data = (Geometry)Application.Current.Resources["Wait2"];
                    break;
                case WaitingPath.Wait3:
                    ((WaitingButton)sender).Waiting.Data = (Geometry)Application.Current.Resources["Wait3"];
                    break;
                case WaitingPath.Wait4:
                    ((WaitingButton)sender).Waiting.Data = (Geometry)Application.Current.Resources["Wait4"];
                    break;
                case WaitingPath.Wait5:
                    ((WaitingButton)sender).Waiting.Data = (Geometry)Application.Current.Resources["Wait5"];
                    break;
                case WaitingPath.Wait6:
                    ((WaitingButton)sender).Waiting.Data = (Geometry)Application.Current.Resources["Wait6"];
                    break;
                case WaitingPath.Wait7:
                    ((WaitingButton)sender).Waiting.Data = (Geometry)Application.Current.Resources["Wait7"];
                    break;
                case WaitingPath.Wait8:
                    ((WaitingButton)sender).Waiting.Data = (Geometry)Application.Current.Resources["Wait8"];
                    break;
                case WaitingPath.Wait9:
                    ((WaitingButton)sender).Waiting.Data = (Geometry)Application.Current.Resources["Wait9"];
                    break;
                case WaitingPath.Wait10:
                    ((WaitingButton)sender).Waiting.Data = (Geometry)Application.Current.Resources["Wait10"];
                    break;
                case WaitingPath.Wait11:
                    ((WaitingButton)sender).Waiting.Data = (Geometry)Application.Current.Resources["Wait11"];
                    break;
                case WaitingPath.Wait12:
                    ((WaitingButton)sender).Waiting.Data = (Geometry)Application.Current.Resources["Wait12"];
                    break;
                case WaitingPath.Wait13:
                    ((WaitingButton)sender).Waiting.Data = (Geometry)Application.Current.Resources["Wait13"];
                    break;
                case null:
                    break;
            }
        }

        public event RoutedEventHandler Click;

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }

        private void Path_Initialized(object sender, EventArgs e)
        {
            var da = new DoubleAnimation(0, 359, new Duration(TimeSpan.FromMilliseconds(1200)));
            var rt = new RotateTransform();
            ((UIElement)sender).RenderTransform = rt;
            ((UIElement)sender).RenderTransformOrigin = new Point(0.5, 0.5);
            da.RepeatBehavior = RepeatBehavior.Forever;
            rt.BeginAnimation(RotateTransform.AngleProperty, da);
        }

        public bool IsWaiting
        {
            get { return (bool)GetValue(IsWaitingProperty); }
            set { SetValue(IsWaitingProperty, value); }
        }

        public object ButtonContent
        {
            get { return GetValue(ButtonContentProperty); }
            set { SetValue(ButtonContentProperty, value); }
        }

        public WaitingPath WaitingPathValue
        {
            get { return (WaitingPath)GetValue(WaitingPathValueProperty); }
            set { SetValue(IsWaitingProperty, value); }
        }

        public static readonly DependencyProperty IsWaitingProperty =
    DependencyProperty.Register("IsWaiting", typeof(bool), typeof(WaitingButton), new PropertyMetadata(false, null));

        public static readonly DependencyProperty WaitingPathValueProperty =
    DependencyProperty.Register("WaitingPathValue", typeof(WaitingPath), typeof(WaitingButton), new PropertyMetadata(WaitingPath.Wait1, OnPathChanged));

        public static readonly DependencyProperty ButtonContentProperty =
DependencyProperty.Register("ButtonContent", typeof(object), typeof(WaitingButton), new PropertyMetadata(null, null));

    }
}
