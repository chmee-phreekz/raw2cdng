using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace raw2cdng_v2
{
    /// <summary>
    /// Interaktionslogik für infoWindow.xaml
    /// </summary>
    public partial class infoWindow : Window
    {
        private static infoWindow instance = null;

        private infoWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // -- enable window move --

            this.DragMove();
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://blog.phreekz.de");
            }
            catch { }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.magiclantern.fm/forum/index.php?board=54.0");
            }
            catch { }
        }

        protected override void OnClosed(EventArgs e)
        {
            instance = null;

            base.OnClosed(e);
        }

        public static void ShowWindow()
        {
            if (instance == null)
                instance = new infoWindow();

            instance.Show();
            instance.Activate();
        }
    }
}
