using System;
using System.Windows;
using System.Windows.Media;

namespace TelegramBot.GUI.Themes
{
    public class Themes
    {
        private static Themes _instance;

        public static Themes Theme
        {
            get { return _instance ?? (_instance = new Themes()); }
            set
            {
                if (value == null) return;
                try
                {
                    Application.Current.Resources["PrimaryLightColor"] = _instance.PrimaryLightColor ?? Brushes.Gray;
                    Application.Current.Resources["PrimaryColor"] = _instance.PrimaryColor ?? Brushes.Gray;
                    Application.Current.Resources["PrimaryDarkColor"] = _instance.PrimaryDarkColor ?? Brushes.Gray;

                    Application.Current.Resources["SecondaryLightColor"] = _instance.SecondaryLightColor ?? Brushes.Gray;
                    Application.Current.Resources["SecondaryColor"] = _instance.SecondaryColor ?? Brushes.Gray;
                    Application.Current.Resources["SecondaryDarkColor"] = _instance.SecondaryDarkColor ?? Brushes.Gray;

                    Application.Current.Resources["BackgroundColor"] = _instance.BackgroundColor ?? Brushes.Black;
                    Application.Current.Resources["AlternativeBackgroundColor"] = _instance.AlternativeBackgroundColor ?? Brushes.White;

                    Application.Current.Resources["TextOnLightColor"] = _instance.TextOnLightColor ?? Brushes.Black;
                    Application.Current.Resources["TextOnDarkColor"] = _instance.TextOnDarkColor ?? Brushes.White;

                    Application.Current.Resources["ShadowColor"] = _instance.ShadowColor;
                    Application.Current.Resources["FontFamilyMain"] = string.IsNullOrEmpty(_instance.FontFamilyMain) ? new FontFamilyConverter().ConvertFrom("Segoe UI") : new FontFamilyConverter().ConvertFrom(_instance.FontFamilyMain);
                    Application.Current.Resources["FontFamilyHighlight"] = string.IsNullOrEmpty(_instance.FontFamilyHighlight) ? new FontFamilyConverter().ConvertFrom("Segoe UI") : new FontFamilyConverter().ConvertFrom(_instance.FontFamilyHighlight);
                    Application.Current.Resources["FontSizeSmall"] = _instance.FontSizeSmall < 4 ? 8 : _instance.FontSizeSmall;
                    Application.Current.Resources["FontSizeNormal"] = _instance.FontSizeNormal < 4 ? 10 : _instance.FontSizeNormal;
                    Application.Current.Resources["FontSizeBig"] = _instance.FontSizeBig < 4 ? 12 : _instance.FontSizeBig;

                    Application.Current.Resources["ScrollWidth"] = _instance.ScrollWidth < 1 ? 1 : _instance.ScrollWidth;
                }
                catch (Exception e)
                {
                    Message.Show(e.Message);
                }
            }
        }
        public SolidColorBrush PrimaryLightColor { get; set; }
        public SolidColorBrush PrimaryColor { get; set; }
        public SolidColorBrush PrimaryDarkColor { get; set; }

        public SolidColorBrush SecondaryLightColor { get; set; }
        public SolidColorBrush SecondaryColor { get; set; }
        public SolidColorBrush SecondaryDarkColor { get; set; }

        public SolidColorBrush TextOnDarkColor { get; set; }
        public SolidColorBrush TextOnLightColor { get; set; }

        public SolidColorBrush BackgroundColor { get; set; }
        public SolidColorBrush AlternativeBackgroundColor { get; set; }

        public SolidColorBrush DisabledColor { get; set; }

        public SolidColorBrush CheckedColor { get; set; }
        public SolidColorBrush UncheckedColor { get; set; }

        public Color ShadowColor { get; set; }

        public string FontFamilyMain { get; set; }
        public string FontFamilyHighlight { get; set; }
        public double FontSizeSmall { get; set; }
        public double FontSizeNormal { get; set; }
        public double FontSizeBig { get; set; }

        public double ScrollWidth { get; set; }


    }
}
