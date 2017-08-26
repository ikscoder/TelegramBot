using System;
using System.IO;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using TelegramBot.DataConnection;

namespace TelegramBot.GUI
{
    public class Settings
    {
        private const string SettingsFileName = "gui.settings";
        private static Settings _instance;

        private bool _isDarkTheme;

        public static Settings Current
        {
            get { return _instance ?? (_instance = new Settings()); }
            set { _instance = value; }
        }

        public static EventHandler OnSettingsChanged;

        public bool IsDarkTheme
        {
            get { return _isDarkTheme; }
            set
            {
                if (value == _isDarkTheme) return;              
                _isDarkTheme = value;
                if (value)
                {
                    var uri = new Uri("Themes\\Dark.xaml", UriKind.Relative);
                    ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
                    Application.Current.Resources.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(resourceDict);
                }
                else
                {
                    var uri = new Uri("Themes\\Light.xaml", UriKind.Relative);
                    ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
                    Application.Current.Resources.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(resourceDict);
                }
                OnSettingsChanged?.Invoke(nameof(IsDarkTheme), null);
            }
        }


        public static void Save()
        {
            try
            {
                using (var sw = new StreamWriter(SettingsFileName, false, Encoding.Default))
                {
                    sw.Write(JsonConvert.SerializeObject(Current,Formatting.Indented));
                }
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "Saving error - " + e.Message));
            }
        }
        public static void Load()
        {
            if (!File.Exists(SettingsFileName)) return;
            using (var sr = new StreamReader(SettingsFileName, Encoding.Default))
            {
                try
                {
                    Current = JsonConvert.DeserializeObject<Settings>(sr.ReadToEnd());
                }
                catch (Exception e) { Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "Loading error - " + e.Message)); }
            }
        }
    }
}
