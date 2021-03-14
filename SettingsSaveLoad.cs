﻿using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace SaladWebhook
{
    [System.Serializable]
    public class SettingsSaveLoad
    {
        public float waitTimeMin = 0;
        public string webhook = "";
        public bool onlyIfNewPost = false;

        public void Save()
        {
            waitTimeMin = (float)Program.waittime;
            webhook = Program.Webhook;
            onlyIfNewPost = Program.postIfChange;
            BinaryFormatter formatter = new BinaryFormatter();
            string paths = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaladWebHook\\config.cfg");
            var stream = new FileStream(paths, FileMode.Create);
            SettingsSaveLoad data = this;
            formatter.Serialize(stream, data);
            stream.Close();
        }

        public static SettingsSaveLoad Load()
        {
            string paths = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaladWebHook\\config.cfg");
            if (File.Exists(paths))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                var stream = new FileStream(paths, FileMode.Open);
                SettingsSaveLoad data = formatter.Deserialize(stream) as SettingsSaveLoad;
                stream.Close();
                return data;
            }
            else
            {
                return null;
            }
        }
    }
}
