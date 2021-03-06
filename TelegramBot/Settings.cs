﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using TelegramBot.DAL;

namespace TelegramBot
{
    public class Settings
    {
        private const string SettingsFileName = "bot.settings";
        private static Settings _instance;
        public static Settings Current
        {
            get { return _instance ?? (_instance = new Settings()); }
            set { _instance = value; }
        }

        public string APIKey { get; set; }
        public string ConnectionString { get; set; }

        public Dictionary<string,bool> AvailableCommands { get; set; }=new Dictionary<string, bool>
        {
            {"officelist",true},
            {"nearestoffice",true},
            {"dialog",true},
            {"recall",true},
            {"opportunity",true},
            {"opportunitieslist",true},
        };
        

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
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "Saving error - "+e.Message));
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
                catch (Exception e) { Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "Loading error - "+e.Message)); }
            }
        }
    }
}
