using System.Collections.Generic;
using System.Windows;

namespace WindowsImgur.SnippingTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            var list = new List<MainWindow>();

            /* Find out how many windows we need to open. */
            foreach (var s in System.Windows.Forms.Screen.AllScreens)
            {
                var win = new MainWindow()
                {
                    Width = s.Bounds.Width,
                    Height = s.Bounds.Height,
                    Left = s.Bounds.X,
                    Top = s.Bounds.Y,
                    Screen = s
                };

                win.OnWindowCapture += (o, args) =>
                {
                    foreach (var w in list)
                        w.Close();
                };

                win.Show();
                list.Add(win);
            }

        }
    }
}
