using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WindowsImgur.Core;
using WindowsImgur.Core.Services;

namespace WindowsImgur.SnippingTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public System.Windows.Forms.Screen Screen;
        public event EventHandler OnWindowCapture;

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
            if (OnWindowCapture != null)
                OnWindowCapture(this, null);

            var image = new ScreenCaptureService().CaptureScreen(Screen);

            var cropped = new Bitmap(Math.Abs((int)startpos.X - (int)endpos.X),
                Math.Abs((int)startpos.Y - (int)endpos.Y), 
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            
            using (var g = Graphics.FromImage(cropped))
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
            var keys = ImgurSettingsService.Load();
            var link = new ImgurService(keys.ClientId)
                    .UploadImageAnonymously(cropped);
            if (link != null)
                System.Diagnostics.Process.Start(link);
            else
                MessageBox.Show("An error with imgur has occured.");
            Canvas.ReleaseMouseCapture();
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
                    Fill = System.Windows.Media.Brushes.DarkGray,
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
