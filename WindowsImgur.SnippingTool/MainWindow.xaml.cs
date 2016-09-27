using Snippur.Core.Services;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;
using MouseEventHandler = System.Windows.Input.MouseEventHandler;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing.Imaging;

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
		private bool IsDrawing;

        private bool _isdoneclipping;
        private bool IsDoneClipping
        {
            get
            {
                return _isdoneclipping;
            }

            set
            {
                if (value)
                {
                    //Draw the image that was snipped so that usser may preview it or edit it if he wants to
                    var drawingrect = (System.Windows.Shapes.Rectangle)DrawingCanvas.Children[0];
                    var rect = new System.Windows.Shapes.Rectangle()
                    {
                        Margin = drawingrect.Margin,
                        StrokeThickness = 0,
                        Width = Math.Abs(drawingrect.Width),
                        Height = Math.Abs(drawingrect.Height)
                    };
                    rect.Fill = new ImageBrush(ConvertToBitmapImage(GetScreenCrop()));
                    DrawingCanvas.Children.Clear();
                    DrawingCanvas.Children.Add(rect);
                }
                else
                    DrawingCanvas.Opacity = 0.6;
                _isdoneclipping = value;
            }
        }

        private bool _isallowedtoclip;
        public bool IsAllowedToClip
        {
            get
            {
                return _isallowedtoclip;
            }
            set
            {
                if (value)
                    //Bring up the canvas so that the user can select the region to clip
                    DrawingCanvas.Visibility = Visibility.Visible;
                else
                {
                    //Hide the canvas so that the user cannot clip screen
                    DrawingCanvas.Visibility = Visibility.Hidden;
                    //Reset drawing surafce
                    DrawingCanvas.Children.Clear();
                }
                _isallowedtoclip = value;
            }
        }

        private const int MinClipWidth = 30;
        private const int MinClipHeight = 30;

        public MainWindow()
		{
			InitializeComponent();
			DrawingCanvas.AddHandler(Mouse.PreviewMouseUpEvent, new MouseButtonEventHandler(Canvas_MouseUp));
			DrawingCanvas.AddHandler(Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(Canvas_MouseDown));
			DrawingCanvas.AddHandler(Mouse.MouseMoveEvent, new MouseEventHandler(Canvas_MouseMove));

            //Start with the docker expanded
            IsAllowedToClip = false;
            IsDoneClipping = false;
			VisualStateManager.GoToElementState(LayoutRoot, "Menu", true);
		}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
		{
            if (!IsAllowedToClip)
                return;
            IsDoneClipping = false;
			IsDrawing = true;
			VisualStateManager.GoToElementState(LayoutRoot, "Base", true);
			_startpos = new System.Drawing.Point((int)e.GetPosition((Canvas)sender).X, 
				(int)e.GetPosition((Canvas)sender).Y);
			DrawingCanvas.CaptureMouse();
		}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsAllowedToClip)
                return;
            IsDrawing = false;
            VisualStateManager.GoToElementState(LayoutRoot, "Menu", true);
            CheckDrawValidity();
            DrawingCanvas.ReleaseMouseCapture();
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void Canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (IsDrawing)
			{
				_endpos = new System.Drawing.Point((int)e.GetPosition((Canvas)sender).X, (int)e.GetPosition((Canvas)sender).Y);
				var drawingrect = new Rectangle(
									   Math.Min(_startpos.X, _endpos.X),
									   Math.Min(_startpos.Y, _endpos.Y),
									   Math.Abs(_startpos.X - _endpos.X),
									   Math.Abs(_startpos.Y - _endpos.Y)
									);
				DrawingCanvas.Children.Clear();
                var rect = new System.Windows.Shapes.Rectangle()
                {
                    Margin = new Thickness(
                        drawingrect.X,
                        drawingrect.Y,
                        0,
                        0
                        ),
                    StrokeThickness = 1,
                    Stroke = System.Windows.Media.Brushes.White,
                    StrokeDashArray = {1,2},
                    Width = Math.Abs(drawingrect.Width),
                    Height = Math.Abs(drawingrect.Height)
				};
				DrawingCanvas.Children.Add(rect);
			}
		}


        /// <summary>
        /// Clips a region of a screen
        /// </summary>
        /// <returns></returns>
        private Bitmap GetScreenCrop()
		{
            var image = new ScreenCaptureService().CaptureScreen(Screen);
			var cropped = new Bitmap(Math.Abs(_startpos.X - _endpos.X),
				Math.Abs(_startpos.Y - _endpos.Y),
				System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			using (var g = Graphics.FromImage(cropped))
			{
				g.DrawImage(image, new Rectangle(0, 0, cropped.Width, cropped.Height),
								 new Rectangle(
									   Math.Min(_startpos.X, _endpos.X),
									   Math.Min(_startpos.Y, _endpos.Y),
									   Math.Abs(_startpos.X - _endpos.X),
									   Math.Abs(_startpos.Y - _endpos.Y)
									),
								 GraphicsUnit.Pixel);
			}
            return cropped;
		}


        /// <summary>
        /// Ensures that the user actually clipped a region whose height and width are bigger than the values specified by
        ///  <see cref="MinClipWidth"/> and <see cref="MinClipHeight"/>
        /// </summary>
        private void CheckDrawValidity()
        {
            if (Math.Abs(_startpos.X - _endpos.X) > MinClipWidth && Math.Abs(_startpos.Y - _endpos.Y) > MinClipHeight)
            {
                IsDoneClipping = true;
                return;
            }
            IsDoneClipping = false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private BitmapImage ConvertToBitmapImage(Bitmap bitmap)
        {
            BitmapImage bitmapImage = null;
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleScreenSnipping(object sender,RoutedEventArgs e)
        {
            IsAllowedToClip = IsAllowedToClip == true ? false : true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TakeScreenshotClick(object sender, RoutedEventArgs e)
        {
            OnWindowCapture?.Invoke(this, null);
            var b = new ScreenCaptureService().CaptureScreen(Screen);

            var dialog = new SaveFileDialog { Filter = "PNG Images | *.png" };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                b.Save(dialog.FileName);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void UploadToImgurClick(object sender, RoutedEventArgs e)
		{
			this.Hide();
			if (OnWindowCapture != null)
				OnWindowCapture(this, null);

			var b = GetScreenCrop();
			UploadToImgur(b);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void SaveToFileClick(object sender, RoutedEventArgs e)
		{
			var b = GetScreenCrop();

			var dialog = new SaveFileDialog {Filter = "PNG Images | *.png"};
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				b.Save(dialog.FileName);
                Close();
            }
            else
            {

            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void CloseClick(object sender, RoutedEventArgs e)
		{
			this.Hide();
			if (OnWindowCapture != null)
				OnWindowCapture(this, null);
			Close();
		}
    }
}
