using System;
using System.ComponentModel;
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
        public static bool debugLogEnabled { get; set; }

        public static void _startNewDebug(string input)
        {
                File.WriteAllText(debugLogFilename, input);
        }

        public static void _saveDebug(string input)
        {
            try
            {
                using (StreamWriter w = File.AppendText(debugLogFilename))
                {
                    w.WriteLine(input);
                }
            }
            catch (Exception e)
            {
                if (e.Source != null)
                    Console.WriteLine("Exception source: {0}", e.Source);
                throw;
            }
        }

        public static void _saveDebugObject(string input, object obj)
        {
            try{ 
            using (StreamWriter w = File.AppendText(debugLogFilename))
            {
                w.WriteLine(" ***** "+input);

                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
                {
                    string name = descriptor.Name;
                    object value = descriptor.GetValue(obj);
                    w.WriteLine("{0}={1}", name, value);
                }
                w.WriteLine(" ***** ");

            }
            }
            catch (Exception e)
            {
                if (e.Source != null)
                    Console.WriteLine("Exception source: {0}", e.Source);
                throw;
            }
        }
    }

}
