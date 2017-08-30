using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Xml.Serialization;


namespace TelegramBot.GUI
{
    public sealed class Settings
    {
        private const string SettingsFileName = "programm.settings";

        private static Settings _instance;

        public static Settings Current
        {
            get { return _instance ?? (_instance = new Settings()); }
            set { _instance = value; }
        }

        private bool _isLoaded;

        private Language _language;
        private Theme _theme;
        private CustomTheme _customTheme;


        public enum Theme { DDarkBlue, LBlue, LCyan, LDarkBlue, LGreen, LGrey, LMetal, LPurple, Custom }
        public enum Language { English, Russian }

        public CustomTheme CustomTheme
        {
            get { return _customTheme ?? (_customTheme = new CustomTheme()); }
            set
            {
                _customTheme = value;
                if (_isLoaded)
                    Save();
            }
        }

        public Language AppLanguage
        {
            get { return _language; }
            set
            {
                _language = value;
                var uri = new Uri("Languages\\" + _language.ToString("G") + ".xaml", UriKind.Relative);
                var resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
                Application.Current.Resources.Clear();
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
                if (_theme == Theme.Custom) CustomTheme.SetCustomTheme();
                if (_isLoaded)
                    Save();
            }
        }
        public Theme AppTheme
        {
            get { return _theme; }
            set
            {
                _theme = value;
                if (_theme != Theme.Custom)
                {
                    var uri = new Uri("Themes\\" + _theme.ToString("G") + ".xaml", UriKind.Relative);
                    var resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
                    Application.Current.Resources.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(resourceDict);
                }
                else
                {
                    CustomTheme.SetCustomTheme();
                }
                if (_isLoaded)
                    Save();
            }
        }

        public static void Save()
        {
            try
            {
                var x = new XmlSerializer(Current.GetType());
                using (var stringWriter = new StringWriter())
                {
                    x.Serialize(stringWriter, Current);
                    using (var sw = new StreamWriter(SettingsFileName, false, Encoding.Default))
                    {
                        sw.Write(stringWriter.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Message.Show(e.Message + "\n" + e.InnerException?.Message + "\n" + e.InnerException?.InnerException?.Message, (string)Application.Current.Resources["LangError"]);
            }
        }

        public static void Load()
        {
            if (!File.Exists(SettingsFileName)) return;
            using (var sr = new StreamReader(SettingsFileName, Encoding.Default))
            {
                try
                {
                    Current = (Settings)new XmlSerializer(Current.GetType()).Deserialize(sr);
                }
                catch (Exception e)
                {
                    Message.Show(e.Message, (string)Application.Current.Resources["LangError"]);
                    Current = new Settings();
                }
                finally
                {
                    Current._isLoaded = true;
                }
            }
        }


    }

}
