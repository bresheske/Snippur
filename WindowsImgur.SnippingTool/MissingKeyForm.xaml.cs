using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WindowsImgur.Core.Services;

namespace WindowsImgur.SnippingTool
{
    /// <summary>
    /// Interaction logic for MissingKeyForm.xaml
    /// </summary>
    public partial class MissingKeyForm : Window
    {
        public MissingKeyForm()
        {
            InitializeComponent();
        }

        private void LoadImgurSettingsUrl(object sender, MouseButtonEventArgs e)
        {
            Process.Start(((TextBlock) sender).Text);
        }

        private void Quit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveAndQuit(object sender, RoutedEventArgs e)
        {
            var s = ImgurSettingsService.Load();
            s.ClientId = txtclientid.Text;
            s.Save();
            Quit(sender, e);
        }
    }
}
