using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using winIO = System.IO;

namespace raw2cdng_v2
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        //Add this method override
        protected void Application_Startup(object sender, StartupEventArgs e)
        {
            //debugging.debugLogFilename = Environment.CurrentDirectory + winIO.Path.DirectorySeparatorChar + "raw2cdng.2.debug.log";

            string[] clArgs = e.Args;

            if (clArgs.Length == 1)
            {
                //debugging._saveDebug("[app.xaml.cs][startup] -- you wanna play a video?");

                string file = clArgs[0];
                /*                
                                // ffplay - if existent
                                Process ffplay = new Process();
                                // should i use this? its a Uri.winIO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)
                                string startup = winIO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Substring(6) + winIO.Path.DirectorySeparatorChar + "ffplay.exe";
                                debugging._saveDebug("[app.xaml.cs][startup] -- startupDir : "+startup);
                                debugging._saveDebug("[app.xaml.cs][startup] -- clArgs[0] : " + clArgs[0]);

                                ProcessStartInfo info = new ProcessStartInfo(startup, file);

                                info.CreateNoWindow = true;
                                info.UseShellExecute = false;
                                ffplay.StartInfo = info;
                                ffplay.Start();
                  */
                // -------------- second way - using own player abilities

                mlvplay player = new mlvplay(file);
                player.Show();
                player.Focus();
                //MainWindow.Hide();
                //Application.Current.Shutdown();
            }
            else
            {
                MainWindow main = new MainWindow();
                main.Show();
                main.Focus();
            }
        }
    }
}
