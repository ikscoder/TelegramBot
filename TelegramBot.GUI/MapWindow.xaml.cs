using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Telegram.Bot.Types;

namespace TelegramBot.GUI
{
    public partial class MapWindow
    {

        public MapWindow()
        {
            InitializeComponent();
            Gmap.OnMapZoomChanged += () => { ZoomSlider.Value = Gmap.Zoom; };
        }

        #region GUI 

        private void BExit_Click(object sender, RoutedEventArgs e)
        {       
            Hide();
        }

        private void BMaximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void BMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
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
        #endregion

        public void AddPoint(Location location)
        {
            if(location==null)return;
            var shape = new Ellipse
            {
                Width = 20,
                Height = 20,
                Margin = new Thickness(-10),
                StrokeThickness = 0
            };
            shape.SetResourceReference(Shape.FillProperty, "SecondaryColor");
            var marker = new GMapMarker(new PointLatLng(location.Latitude, location.Longitude))
            {
                Shape = shape
            };
            var cm = new ContextMenu();
            var delitem = new MenuItem { Header = "Удалить" };
            delitem.Click += (s, e) => { Gmap.Markers.Remove(marker); };
            cm.Items.Add(delitem);
            shape.ContextMenu = cm;
            Gmap.Markers.Add(marker);
            Gmap.Position = new PointLatLng(location.Latitude, location.Longitude);
        }

        public void AddPoint(Venue venue)
        {
            if(venue==null)return;
            #region Tooltip

            var grid = new Grid();
            grid.SetResourceReference(BackgroundProperty, "AlternativeBackgroundColor");
            grid.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(1, GridUnitType.Star)
            });
            grid.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(1, GridUnitType.Star)
            });
            var text = new TextBlock
            {
                Margin = new Thickness(4),
                Text = venue.Title
            };
            Grid.SetRow(text, 0);
            grid.Children.Add(text);

            text = new TextBlock
            {
                Margin = new Thickness(4),
                Text = venue.Address
            };
            Grid.SetRow(text, 1);
            grid.Children.Add(text);
            #endregion

            var shape = new Ellipse
            {
                Width = 20,
                Height = 20,
                Margin = new Thickness(-10),
                ToolTip = grid,
                StrokeThickness = 0,
            };
            shape.SetResourceReference(Shape.FillProperty, "SecondaryColor");
            var marker = new GMapMarker(new PointLatLng(venue.Location.Latitude, venue.Location.Longitude))
            {
                Shape = shape
            };
            var cm = new ContextMenu();
            var delitem = new MenuItem { Header = "Удалить" };
            delitem.Click += (s,e)=> { Gmap.Markers.Remove(marker); };
            cm.Items.Add(delitem);
            shape.ContextMenu = cm;
            Gmap.Markers.Add(marker);
            Gmap.Position = new PointLatLng(venue.Location.Latitude, venue.Location.Longitude);
        }

        public void AddOffice(Venue venue)
        {
            if (venue == null) return;
            #region Tooltip

            var grid = new Grid();
            grid.SetResourceReference(BackgroundProperty, "AlternativeBackgroundColor");
            grid.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(1, GridUnitType.Star)
            });
            grid.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(1, GridUnitType.Star)
            });
            var text = new TextBlock
            {
                Margin = new Thickness(4),
                Text = venue.Title
            };
            Grid.SetRow(text, 0);
            grid.Children.Add(text);

            text = new TextBlock
            {
                Margin = new Thickness(4),
                Text = venue.Address
            };
            Grid.SetRow(text, 1);
            grid.Children.Add(text);
            #endregion

            var shape = new Path
            {
                Width = 40,
                Height = 40,
                Margin = new Thickness(-20),
                ToolTip = grid,
                StrokeThickness = 0,
            };
            shape.SetResourceReference(Shape.FillProperty, "PrimaryColor");
            shape.SetResourceReference(Path.DataProperty, "Office");
            Gmap.Markers.Add(new GMapMarker(new PointLatLng(venue.Location.Latitude, venue.Location.Longitude))
            {
                Shape = shape
            });
            Gmap.Position = new PointLatLng(venue.Location.Latitude, venue.Location.Longitude);
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
    
            Gmap.MapProvider = GoogleMapProvider.Instance;
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            Gmap.CacheLocation= Environment.CurrentDirectory;
            Gmap.EmptyMapBackground = Brushes.Transparent;
            Gmap.ShowCenter = false;
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Math.Abs(Gmap.Zoom - (int)ZoomSlider.Value) > 0.5)
            Gmap.Zoom = (int) ZoomSlider.Value;
        }
    }
}
