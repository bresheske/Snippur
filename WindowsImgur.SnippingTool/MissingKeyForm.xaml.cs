using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Snippur.Core.Services;

namespace Snippur.SnippingTool
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
