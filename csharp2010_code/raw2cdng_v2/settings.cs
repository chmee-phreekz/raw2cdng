using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.IO;

namespace raw2cdng_v2
{
    class appSettings : AppSettings<appSettings>
    {
        public string outputPath = "";
        public bool sourcePath;
        public string uniqueName = "";
        public bool debugLogEnabled;
        public string debugLogFile = "";

        public bool chromaSmooth;
        public bool proxyJpeg;
        public bool highlightFix;
        public bool verticalBanding;
        public int format;
        public string prefix;
    }

    public class AppSettings<T> where T : new()
    {
        public static readonly string DEFAULT_FILENAME = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "raw2cdng.2.settings.json";

        public void Save()
        {
            File.WriteAllText(DEFAULT_FILENAME, (new JavaScriptSerializer()).Serialize(this));
        }

        public static void Save(T pSettings)
        {
            File.WriteAllText(DEFAULT_FILENAME, (new JavaScriptSerializer()).Serialize(pSettings));
            //MessageBox.Show(DEFAULT_FILENAME);
        }

        public static T Load()
        {
            T t = new T();
            if (File.Exists(DEFAULT_FILENAME))
                t = (new JavaScriptSerializer()).Deserialize<T>(File.ReadAllText(DEFAULT_FILENAME));
            return t;
        }
    }

    class debugging
    {
        public static string debugLogFilename { get; set; }

        public static void _saveDebug(string input)
        {
            if (!File.Exists(debugLogFilename)) File.Create(debugLogFilename);
            using (StreamWriter w = File.AppendText(debugLogFilename))
            {
                w.WriteLine(String.Format("{0:yy.MM.dd HH:mm:ss -- }", DateTime.Now) + input);
            }
        }
    }

}
