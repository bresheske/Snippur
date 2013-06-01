using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Snippur.Core.Services;
using MessageBox = System.Windows.MessageBox;
using MouseEventHandler = System.Windows.Input.MouseEventHandler;

namespace Snippur.SnippingTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public System.Windows.Forms.Screen Screen;
        public event EventHandler OnWindowCapture;

        private System.Drawing.Point _startpos;
        private System.Drawing.Point _endpos;
        private bool _isdrawing;
		
        public MainWindow()
        {
            InitializeComponent();
            Canvas.AddHandler(Mouse.PreviewMouseUpEvent, new MouseButtonEventHandler(Canvas_MouseUp));
            Canvas.AddHandler(Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(Canvas_MouseDown));
            Canvas.AddHandler(Mouse.MouseMoveEvent, new MouseEventHandler(Canvas_MouseMove));
        }

        private void Canvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _isdrawing = true;
            VisualStateManager.GoToElementState(LayoutRoot, "Base", true);
            _startpos = new System.Drawing.Point((int)e.GetPosition((Canvas)sender).X, 
                (int)e.GetPosition((Canvas)sender).Y);
            Canvas.CaptureMouse();
        }

        private void Canvas_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _isdrawing = false;
            VisualStateManager.GoToElementState(LayoutRoot, "Menu", true);
            Canvas.ReleaseMouseCapture();
        }

        private void Canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isdrawing)
            {
                _endpos = new System.Drawing.Point((int)e.GetPosition((Canvas)sender).X, (int)e.GetPosition((Canvas)sender).Y);
                var drawingrect = new System.Drawing.Rectangle(
                                       Math.Min(_startpos.X, _endpos.X),
                                       Math.Min(_startpos.Y, _endpos.Y),
                                       Math.Abs(_startpos.X - _endpos.X),
                                       Math.Abs(_startpos.Y - _endpos.Y)
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

        private Bitmap GetScreenCrop()
        {
            var image = new ScreenCaptureService().CaptureScreen(Screen);
            var cropped = new Bitmap(Math.Abs(_startpos.X - _endpos.X),
                Math.Abs(_startpos.Y - _endpos.Y),
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(cropped))
            {
                g.DrawImage(image, new System.Drawing.Rectangle(0, 0, cropped.Width, cropped.Height),
                                 new System.Drawing.Rectangle(
                                       Math.Min(_startpos.X, _endpos.X),
                                       Math.Min(_startpos.Y, _endpos.Y),
                                       Math.Abs(_startpos.X - _endpos.X),
                                       Math.Abs(_startpos.Y - _endpos.Y)
                                    ),
                                 GraphicsUnit.Pixel);
            }
            return cropped;
        }

        private void UploadToImgur(Bitmap b)
        {
            var keys = ImgurSettingsService.Load();
            var link = new ImgurService(keys.ClientId)
                    .UploadImageAnonymously(b);
            if (link != null)
                System.Diagnostics.Process.Start(link);
            else
                MessageBox.Show("An error with imgur has occured.");
        }

        private void UploadToImgurClick(object sender, MouseButtonEventArgs e)
        {
            this.Hide();
            if (OnWindowCapture != null)
                OnWindowCapture(this, null);

            var b = GetScreenCrop();
            UploadToImgur(b);
            Close();
        }

        private void SaveToFileClick(object sender, MouseButtonEventArgs e)
        {
            this.Hide();
            if (OnWindowCapture != null)
                OnWindowCapture(this, null);

            var b = GetScreenCrop();

            var dialog = new SaveFileDialog {Filter = "PNG Images | *.png"};
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                b.Save(dialog.FileName);
            }
            Close();
        }

        private void CloseClick(object sender, MouseButtonEventArgs e)
        {
            this.Hide();
            if (OnWindowCapture != null)
                OnWindowCapture(this, null);
            Close();
        }
    }
}
