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
		System.Drawing.Point startpos;
        System.Drawing.Point endpos;
		bool isdrawing;
		
        public MainWindow()
        {
            InitializeComponent();
            Canvas.AddHandler(Mouse.PreviewMouseUpEvent, new MouseButtonEventHandler(Canvas_MouseUp));
            Canvas.AddHandler(Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(Canvas_MouseDown));
            Canvas.AddHandler(Mouse.MouseMoveEvent, new MouseEventHandler(Canvas_MouseMove));
        }

        private void Canvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isdrawing = true;
            startpos = new System.Drawing.Point((int)e.GetPosition((Canvas)sender).X, 
                (int)e.GetPosition((Canvas)sender).Y);
            Canvas.CaptureMouse();
        }

        private void Canvas_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isdrawing = false;
            this.Hide();
            var image = new ScreenCaptureService().CaptureScreen();
            


            var cropped = new Bitmap(Math.Abs((int)startpos.X - (int)endpos.X),
                Math.Abs((int)startpos.Y - (int)endpos.Y), 
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            
            using (Graphics g = Graphics.FromImage(cropped))
            {
                g.DrawImage(image, new System.Drawing.Rectangle(0, 0, cropped.Width, cropped.Height),
                                 new System.Drawing.Rectangle(
                                       Math.Min(startpos.X, endpos.X),
                                       Math.Min(startpos.Y, endpos.Y),
                                       Math.Abs(startpos.X - endpos.X),
                                       Math.Abs(startpos.Y - endpos.Y)
                                    ),
                                 GraphicsUnit.Pixel);
            }

            var link = new ImgurService(ConfigurationManager.AppSettings["Client-ID"], ConfigurationManager.AppSettings["Client-Secret"])
                    .UploadImageAnonymously(cropped);
            if (link != null)
                System.Diagnostics.Process.Start(link);
            else
                MessageBox.Show("An error with imgur has occured.");
            Canvas.ReleaseMouseCapture();
            this.Close();
        }

        private void Canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isdrawing)
            {
                endpos = new System.Drawing.Point((int)e.GetPosition((Canvas)sender).X, (int)e.GetPosition((Canvas)sender).Y);
                var drawingrect = new System.Drawing.Rectangle(
                                       Math.Min(startpos.X, endpos.X),
                                       Math.Min(startpos.Y, endpos.Y),
                                       Math.Abs(startpos.X - endpos.X),
                                       Math.Abs(startpos.Y - endpos.Y)
                                    );
                Canvas.Children.Clear();
                var rect = new System.Windows.Shapes.Rectangle()
                {
                    Stroke = System.Windows.Media.Brushes.Aqua,
                    Margin = new Thickness(
                        drawingrect.X,
                        drawingrect.Y,
                        0,
                        0
                        ),
                    StrokeThickness = 1,
                    Width = Math.Abs(drawingrect.Width),
                    Height = Math.Abs(drawingrect.Height)
                };
                Canvas.Children.Add(rect);
            }
        }

    }
}
