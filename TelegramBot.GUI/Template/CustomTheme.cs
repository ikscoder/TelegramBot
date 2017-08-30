using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

namespace TelegramBot.GUI
{
    public class CustomTheme
    {
        #region CustomTheme

        private SolidColorBrush _primaryColor = Brushes.Gray;
        private SolidColorBrush _backgroundColor = Brushes.LightGray;
        private SolidColorBrush _alternativeBackgroundColor = Brushes.White;
        private SolidColorBrush _secondaryColor = Brushes.LawnGreen;
        private SolidColorBrush _textOnLightColor = Brushes.Black;
        private SolidColorBrush _textOnDarkColor = Brushes.White;
        private Color _shadowColor = Colors.Black;
        private string _fontFamilyMain = "Segoe UI";
        private string _fontFamilyHighlight = "Segoe UI";
        private double _fontSizeSmall = 8;
        private double _fontSizeNormal = 12;
        private double _fontSizeBig = 16;
        private double _scrollWidth = 4;


        [XmlElement(Type = typeof(XmlSolidColor))]
        public SolidColorBrush PrimaryColor
        {
            get { return _primaryColor; }
            set { _primaryColor = value; SetCustomTheme(); }
        }
        [XmlElement(Type = typeof(XmlSolidColor))]
        public SolidColorBrush BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; SetCustomTheme(); }
        }
        [XmlElement(Type = typeof(XmlSolidColor))]
        public SolidColorBrush AlternativeBackgroundColor
        {
            get { return _alternativeBackgroundColor; }
            set { _alternativeBackgroundColor = value; SetCustomTheme(); }
        }
        [XmlElement(Type = typeof(XmlSolidColor))]
        public SolidColorBrush SecondaryColor
        {
            get { return _secondaryColor; }
            set { _secondaryColor = value; SetCustomTheme(); }
        }
        [XmlElement(Type = typeof(XmlSolidColor))]
        public SolidColorBrush TextOnLightColor
        {
            get { return _textOnLightColor; }
            set { _textOnLightColor = value; SetCustomTheme(); }
        }
        [XmlElement(Type = typeof(XmlSolidColor))]
        public SolidColorBrush TextOnDarkColor
        {
            get { return _textOnDarkColor; }
            set { _textOnDarkColor = value; SetCustomTheme(); }
        }

        [XmlElement(Type = typeof(XmlColor))]
        public Color ShadowColor
        {
            get { return _shadowColor; }
            set { _shadowColor = value; SetCustomTheme(); }
        }

        public string FontFamilyMain
        {
            get { return _fontFamilyMain; }
            set { _fontFamilyMain = value; SetCustomTheme(); }
        }

        public string FontFamilyHighlight
        {
            get { return _fontFamilyHighlight; }
            set { _fontFamilyHighlight = value; SetCustomTheme(); }
        }

        public double FontSizeSmall
        {
            get { return _fontSizeSmall; }
            set { _fontSizeSmall = value; SetCustomTheme(); }
        }

        public double FontSizeNormal
        {
            get { return _fontSizeNormal; }
            set { _fontSizeNormal = value; SetCustomTheme(); }
        }

        public double FontSizeBig
        {
            get { return _fontSizeBig; }
            set { _fontSizeBig = value; SetCustomTheme(); }
        }

        public double ScrollWidth
        {
            get { return _scrollWidth; }
            set { _scrollWidth = value; SetCustomTheme(); }
        }

        #endregion

        public void SetCustomTheme()
        {
            try
            {
                Application.Current.Resources["PrimaryColor"] = PrimaryColor ?? Brushes.Gray;
                Application.Current.Resources["PrimaryCColor"] = PrimaryColor?.Color ?? Colors.Gray;
                Application.Current.Resources["BackgroundColor"] = BackgroundColor ?? Brushes.White;
                Application.Current.Resources["AlternativeBackgroundColor"] = AlternativeBackgroundColor ?? Brushes.White;

                var darkMain = PrimaryColor?.Color.Mix(Colors.Black, 0.7) ?? Colors.Gray;
                var lightMain = PrimaryColor?.Color.Mix(Colors.White, 0.7) ?? Colors.Gray;
                Application.Current.Resources["PrimaryLightColor"] = new SolidColorBrush(lightMain);
                Application.Current.Resources["PrimaryDarkColor"] = new SolidColorBrush(darkMain);

                Application.Current.Resources["DisabledColor"] = new SolidColorBrush(PrimaryColor?.Color ?? Colors.Gray) { Opacity = 0.5 };
                Application.Current.Resources["SecondaryColor"] = SecondaryColor ?? Brushes.Green;
                Application.Current.Resources["SecondaryLightColor"] = new SolidColorBrush(SecondaryColor?.Color.Mix(Colors.White, 0.7) ?? Colors.Green);
                Application.Current.Resources["SecondaryDarkColor"] = new SolidColorBrush(SecondaryColor?.Color.Mix(Colors.Black, 0.7) ?? Colors.Green);

                Application.Current.Resources["CheckedColor"] = Brushes.LightGreen;
                Application.Current.Resources["UncheckedColor"] = Brushes.Red;

                Application.Current.Resources["TextOnLightColor"] = TextOnLightColor ?? Brushes.Black;
                Application.Current.Resources["TextOnDarkColor"] = TextOnDarkColor ?? Brushes.White;

                Application.Current.Resources["ShadowColor"] = ShadowColor;
                Application.Current.Resources["FontFamilyMain"] = string.IsNullOrEmpty(FontFamilyMain) ? new FontFamilyConverter().ConvertFrom("Segoe UI") : new FontFamilyConverter().ConvertFrom(FontFamilyMain);
                Application.Current.Resources["FontFamilyHighlight"] = string.IsNullOrEmpty(FontFamilyHighlight) ? new FontFamilyConverter().ConvertFrom("Segoe UI") : new FontFamilyConverter().ConvertFrom(FontFamilyHighlight);
                Application.Current.Resources["FontSizeSmall"] = FontSizeSmall < 4 ? 8 : FontSizeSmall;
                Application.Current.Resources["FontSizeNormal"] = FontSizeNormal < 4 ? 10 : FontSizeNormal;
                Application.Current.Resources["FontSizeBig"] = FontSizeBig < 4 ? 12 : FontSizeBig;

                Application.Current.Resources["ScrollWidth"] = ScrollWidth < 1 ? 1 : ScrollWidth;
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "SetCustomTheme: " + e.Message));
            }
        }
    }


    public class XmlColor
    {
        private Color _color = Colors.Black;

        public XmlColor() { }
        public XmlColor(Color c) { _color = c; }


        public Color ToColor()
        {
            return _color;
        }

        public void FromColor(Color c)
        {
            _color = c;
        }

        public static implicit operator Color(XmlColor x)
        {
            return x.ToColor();
        }

        public static implicit operator XmlColor(Color c)
        {
            return new XmlColor(c);
        }

        [XmlAttribute]
        public string Web
        {
            get { return ColorTranslator.ToHtml(System.Drawing.Color.FromArgb(_color.A, _color.R, _color.G, _color.B)); }
            set
            {
                try
                {
                    _color = Alpha == 0xFF ? Color.FromRgb(ColorTranslator.FromHtml(value).R, ColorTranslator.FromHtml(value).G, ColorTranslator.FromHtml(value).B) : Color.FromArgb(Alpha, ColorTranslator.FromHtml(value).R, ColorTranslator.FromHtml(value).G, ColorTranslator.FromHtml(value).B);
                }
                catch
                {
                    _color = Colors.Black;
                }
            }
        }

        [XmlAttribute]
        public byte Alpha
        {
            get { return _color.A; }
            set
            {
                if (value != _color.A)
                    _color = Color.FromArgb(value, _color.R, _color.G, _color.B);
            }
        }

        public bool ShouldSerializeAlpha() { return Alpha < 0xFF; }
    }

    public class XmlSolidColor
    {
        private SolidColorBrush _color = Brushes.Black;

        public XmlSolidColor() { }
        public XmlSolidColor(SolidColorBrush c) { _color = c; }


        public SolidColorBrush ToColor()
        {
            return _color;
        }

        public void FromColor(SolidColorBrush c)
        {
            _color = c;
        }

        public static implicit operator SolidColorBrush(XmlSolidColor x)
        {
            return x.ToColor();
        }

        public static implicit operator XmlSolidColor(SolidColorBrush c)
        {
            return new XmlSolidColor(c);
        }

        [XmlAttribute]
        public string Web
        {
            get { return ColorTranslator.ToHtml(System.Drawing.Color.FromArgb(_color.Color.A, _color.Color.R, _color.Color.G, _color.Color.B)); }
            set
            {
                try
                {
                    _color = new SolidColorBrush(Alpha == 0xFF ? Color.FromRgb(ColorTranslator.FromHtml(value).R, ColorTranslator.FromHtml(value).G, ColorTranslator.FromHtml(value).B) : Color.FromArgb(Alpha, ColorTranslator.FromHtml(value).R, ColorTranslator.FromHtml(value).G, ColorTranslator.FromHtml(value).B));
                }
                catch
                {
                    _color = Brushes.Black;
                }
            }
        }

        [XmlAttribute]
        public byte Alpha
        {
            get { return _color.Color.A; }
            set
            {
                if (value != _color.Color.A)
                    _color = new SolidColorBrush(Color.FromArgb(value, _color.Color.R, _color.Color.G, _color.Color.B));
            }
        }

        public bool ShouldSerializeAlpha() { return Alpha < 0xFF; }
    }


    public static class ColorExtension
    {
        public static Color Mix(this Color color, Color backColor, double amount)
        {
            byte a = (byte)((color.A * amount) + backColor.A * (1 - amount));
            byte r = (byte)((color.R * amount) + backColor.R * (1 - amount));
            byte g = (byte)((color.G * amount) + backColor.G * (1 - amount));
            byte b = (byte)((color.B * amount) + backColor.B * (1 - amount));
            return Color.FromArgb(a, r, g, b);
        }
    }
}