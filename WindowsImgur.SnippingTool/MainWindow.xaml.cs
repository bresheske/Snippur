using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsImgur.Core;
using WindowsImgur.Core.Services;

namespace WindowsImgur.SnippingTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		System.Windows.Point lefttoppoint;
        System.Windows.Point rightbottompoint;
		bool isdrawing;
		
        public MainWindow()
        {
            InitializeComponent();
            Canvas.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(Canvas_MouseUp));
            Canvas.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(Canvas_MouseDown));
            Canvas.AddHandler(UIElement.MouseMoveEvent, new MouseEventHandler(Canvas_MouseMove));
        }

        private void Canvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isdrawing = true;
            lefttoppoint = new System.Windows.Point((int)e.GetPosition((Canvas)sender).X, (int)e.GetPosition((Canvas)sender).Y);
        }

        private void Canvas_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isdrawing = false;
            this.Hide();
            var image = new ScreenCaptureService().CaptureScreen();
            var cropped = new Bitmap((int)rightbottompoint.X - (int)lefttoppoint.X, (int)rightbottompoint.Y - (int)lefttoppoint.Y, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            
            using (Graphics g = Graphics.FromImage(cropped))
            {
                g.DrawImage(image, new System.Drawing.Rectangle(0, 0, cropped.Width, cropped.Height),
                                 new System.Drawing.Rectangle((int)lefttoppoint.X, (int)lefttoppoint.Y, (int)rightbottompoint.X - (int)lefttoppoint.X, (int)rightbottompoint.Y - (int)lefttoppoint.Y),
                                 GraphicsUnit.Pixel);
            }

            var link = new ImgurService(ConfigurationManager.AppSettings["Client-ID"], ConfigurationManager.AppSettings["Client-Secret"])
                    .UploadImageAnonymously(cropped);
            if (link != null)
                System.Diagnostics.Process.Start(link);
            else
                MessageBox.Show("An error with imgur has occured.");
            this.Close();
        }

        private void Canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isdrawing)
            {
                rightbottompoint = new System.Windows.Point(e.GetPosition((Canvas)sender).X, e.GetPosition((Canvas)sender).Y);
                Canvas.Children.Clear();
                var rect = new System.Windows.Shapes.Rectangle()
                {
                    Stroke = System.Windows.Media.Brushes.Aqua,
                    Margin = new Thickness(
                        lefttoppoint.X,
                        lefttoppoint.Y,
                        0,
                        0
                        ),
                    StrokeThickness = 1,
                    Width = Math.Abs(rightbottompoint.X - lefttoppoint.X),
                    Height = Math.Abs(rightbottompoint.Y - lefttoppoint.Y)
                };
                Canvas.Children.Add(rect);
            }
        }

    }
}
