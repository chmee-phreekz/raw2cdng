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
            debugging.debugLogFilename = Environment.CurrentDirectory + winIO.Path.DirectorySeparatorChar + "raw2cdng.2.debug.log";

            string[] clArgs = e.Args;

            if (clArgs.Length == 1)
            {
                string file = clArgs[0];
                
                debugging._saveDebug("[app.xaml.cs][startup] -- you wanna play a video? file: "+file);

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
