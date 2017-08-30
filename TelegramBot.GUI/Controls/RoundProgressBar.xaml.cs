using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace TelegramBot.GUI
{
    public partial class RoundProgressBar
    {
        public RoundProgressBar()
        {
            InitializeComponent();
            UserControl.Loaded += (s, e) => { RenderArc(); };
            UserControl.LayoutUpdated += (s, e) => { RenderArc(); };
            Angle = Max > Min ? ((Value - Min) / (Max - Min) * 360) : 0;
            RenderArc();
        }

        public Brush SegmentColor
        {
            get { return (Brush)GetValue(SegmentColorProperty); }
            set { SetValue(SegmentColorProperty, value); }
        }

        public Brush EmptySegmentColor
        {
            get { return (Brush)GetValue(EmptySegmentColorProperty); }
            set { SetValue(EmptySegmentColorProperty, value); }
        }

        public int StrokeThickness
        {
            get { return (int)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }


        public double Value
        {
            get
            {
                var value = (double)GetValue(ValueProperty);
                value = value > Max ? Max : value < Min ? Min : value;
                return value;
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }


        public double Min
        {
            get
            {
                return (double)GetValue(MinProperty);
            }
            set
            {
                SetValue(MinProperty, value);
            }
        }

        public double Max
        {
            get { return (double)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
    DependencyProperty.Register("Text", typeof(string), typeof(RoundProgressBar), new PropertyMetadata("", OnPropertyChanged));

        public static readonly DependencyProperty MinProperty =
    DependencyProperty.Register("Min", typeof(double), typeof(RoundProgressBar), new PropertyMetadata(0d, OnValueChanged));

        public static readonly DependencyProperty MaxProperty =
    DependencyProperty.Register("Max", typeof(double), typeof(RoundProgressBar), new PropertyMetadata(100d, OnValueChanged));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(RoundProgressBar), new PropertyMetadata(0d, OnValueChanged));

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(int), typeof(RoundProgressBar), new PropertyMetadata(3));

        public static readonly DependencyProperty SegmentColorProperty =
            DependencyProperty.Register("SegmentColor", typeof(Brush), typeof(RoundProgressBar), new PropertyMetadata(new SolidColorBrush(Colors.GreenYellow)));

        public static readonly DependencyProperty EmptySegmentColorProperty =
    DependencyProperty.Register("EmptySegmentColor", typeof(Brush), typeof(RoundProgressBar), new PropertyMetadata(new SolidColorBrush(Colors.Red)));

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double), typeof(RoundProgressBar), new PropertyMetadata(120d, OnPropertyChanged));

        private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var circle = sender as RoundProgressBar;
            if (circle != null)
                circle.Angle = circle.Max > circle.Min ? ((circle.Value - circle.Min) / (circle.Max - circle.Min) * 360) : 0;
        }

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            (sender as RoundProgressBar)?.RenderArc();
        }


        public void RenderArc()
        {
            var startPoint = new Point(ActualWidth / 2, (double)StrokeThickness / 2);
            var endPoint = ComputeCartesianCoordinate(Angle);
            endPoint.X += ActualWidth / 2;
            endPoint.Y += ActualHeight / 2;
            PathFigure.StartPoint = startPoint;
            if (Math.Abs(startPoint.X - Math.Round(endPoint.X)) < 1 && Math.Abs(startPoint.Y - Math.Round(endPoint.Y)) < 1)
                endPoint.X -= 0.1;
            ArcSegment.Point = endPoint;
            ArcSegment.Size = new Size(Math.Abs(ActualWidth / 2 - (double)StrokeThickness / 2), Math.Abs(ActualHeight / 2 - (double)StrokeThickness / 2));
            ArcSegment.IsLargeArc = Angle > 180.0;
        }

        private Point ComputeCartesianCoordinate(double angle)
        {
            var angleRad = (Math.PI / 180.0) * (angle - 90);
            var x = (ActualWidth / 2 - (double)StrokeThickness / 2) * Math.Cos(angleRad);
            var y = (ActualHeight / 2 - (double)StrokeThickness / 2) * Math.Sin(angleRad);
            return new Point(x, y);
        }

    }

    public class DoubleDecreaseValueConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value?.GetType() != typeof(int) && value?.GetType() != typeof(double)) return null;
            return (int)value >= 1 ? (int)value - 1 : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
