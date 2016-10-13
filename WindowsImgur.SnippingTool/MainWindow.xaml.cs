using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing.Imaging;
using Snippur.Core.Services;

using Point = System.Drawing.Point;
using MessageBox = System.Windows.MessageBox;
using MouseEventHandler = System.Windows.Input.MouseEventHandler;

namespace Snippur.SnippingTool
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
        #region Variables

        /// <summary>
        /// The primary screen of the clients PC
        /// </summary>
        public Screen Screen;

        /// <summary>
        /// 
        /// </summary>
		public event EventHandler OnWindowCapture;

        /// <summary>
        /// Indicates the start position of a snipp region
        /// </summary>
		private Point DrawStartPosition;

        /// <summary>
        /// Indicates the end position of a snipp region
        /// </summary>
		private Point DrawEndPosition;

        /// <summary>
        /// Specifies the least possible width of a snipp region
        /// </summary>
        private const int MinClipWidth = 30;

        /// <summary>
        /// Specifies the least possible height of a snipp region
        /// </summary>
        private const int MinClipHeight = 30;

        /// <summary>
        /// A boolean which indicates that the user is still snipping 
        /// </summary>
		private bool IsSnipping;

        /// <summary>
        /// A boolean which indicates that the user is done snipping
        /// </summary>
        private bool _IsDoneSnipping;

        /// <summary>
        /// Property wrapper for <see cref="_IsDoneSnipping"/> with some custom logic
        /// </summary>
        private bool IsDoneDrawing
        {
            get { return _IsDoneSnipping; }

            set
            {
                if (value)
                {
                    //Draw the image that was just snipped so that user may preview it or edit it
                    var drawingrect = (System.Windows.Shapes.Rectangle)DrawingCanvas.Children[0];
                    var rect = new System.Windows.Shapes.Rectangle()
                    {
                        Margin = drawingrect.Margin,
                        StrokeThickness = 0,
                        Width = drawingrect.Width,
                        Height = drawingrect.Height
                    };
                    rect.Fill = new ImageBrush(ConvertToBitmapImage(GetScreenSnipp()));
                    DrawingCanvas.Children.Clear();
                    DrawingCanvas.Children.Add(rect);
                }
                else
                    DrawingCanvas.Opacity = 0.6;

                _IsDoneSnipping = value;
            }
        }

        /// <summary>
        /// A boolean which indictaes whether or not the user is allowed to snipp a region of the screen
        /// </summary>
        private bool _IsAllowedToSnipp;

        /// <summary>
        /// Property wrapper for <see cref="_IsAllowedToSnipp"/> with some custom logic
        /// </summary>
        public bool IsAllowedToSnipp
        {
            get { return _IsAllowedToSnipp; }
            set
            {
                if (value)
                    //Bring up the canvas so that the user can select the region to clip
                    DrawingCanvas.Visibility = Visibility.Visible;
                else
                {
                    //Hide the canvas so that the user cannot clip screen and reset the drawing surface
                    DrawingCanvas.Visibility = Visibility.Hidden;
                    DrawingCanvas.Children.Clear();
                }
                _IsAllowedToSnipp = value;
            }
        }

        #endregion

        public MainWindow()
		{
			InitializeComponent();
			DrawingCanvas.AddHandler(Mouse.PreviewMouseUpEvent, new MouseButtonEventHandler(Canvas_MouseUp));
			DrawingCanvas.AddHandler(Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(Canvas_MouseDown));
			DrawingCanvas.AddHandler(Mouse.MouseMoveEvent, new MouseEventHandler(Canvas_MouseMove));

            //Start with the docker expanded
            IsAllowedToSnipp = false;
            IsDoneDrawing = false;
			VisualStateManager.GoToElementState(LayoutRoot, "Menu", true);
		}


        /// <summary>
        /// Handles events for when the mouse buttons have been presssed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsAllowedToSnipp)
                return;

            IsDoneDrawing = false;
            IsSnipping = true;
            VisualStateManager.GoToElementState(LayoutRoot, "Base", true);
            DrawStartPosition = new System.Drawing.Point((int)e.GetPosition((Canvas)sender).X, (int)e.GetPosition((Canvas)sender).Y);
            DrawingCanvas.CaptureMouse();
        }


 
        /// <summary>
        /// Handles events for when the mouse is being moved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (IsSnipping)
			{
				DrawEndPosition = new System.Drawing.Point((int)e.GetPosition((Canvas)sender).X, (int)e.GetPosition((Canvas)sender).Y);
				var drawingrect = new Rectangle(
									   Math.Min(DrawStartPosition.X, DrawEndPosition.X),
									   Math.Min(DrawStartPosition.Y, DrawEndPosition.Y),
									   Math.Abs(DrawStartPosition.X - DrawEndPosition.X),
									   Math.Abs(DrawStartPosition.Y - DrawEndPosition.Y)
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
        /// Handles events for when the mouse buttons have been released
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsAllowedToSnipp)
                return;

            if (IsValidDraw())
            {
                VisualStateManager.GoToElementState(LayoutRoot, "Menu", true);
                IsSnipping = false;
                IsDoneDrawing = true;
            }
            else
            {
                MessageBox.Show("The region you have snipped is too small :(");
                IsSnipping = false;
            }

            //Always release mouse caputure 
            DrawingCanvas.ReleaseMouseCapture();

        }


        /// <summary>
        /// Returns a <see cref="Bitmap"/> of the 
        /// </summary>
        /// <returns></returns>
        private Bitmap GetScreenSnipp()
		{
            var image = new ScreenCaptureService().CaptureScreen(Screen);
			var cropped = new Bitmap(Math.Abs(DrawStartPosition.X - DrawEndPosition.X),
				Math.Abs(DrawStartPosition.Y - DrawEndPosition.Y),
				System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			using (var g = Graphics.FromImage(cropped))
			{
				g.DrawImage(image, new Rectangle(0, 0, cropped.Width, cropped.Height),
								 new Rectangle(
									   Math.Min(DrawStartPosition.X, DrawEndPosition.X),
									   Math.Min(DrawStartPosition.Y, DrawEndPosition.Y),
									   Math.Abs(DrawStartPosition.X - DrawEndPosition.X),
									   Math.Abs(DrawStartPosition.Y - DrawEndPosition.Y)
									),
								 GraphicsUnit.Pixel);
			}
            return cropped;
		}


        /// <summary>
        /// Checks that the user actually clipped a region whose height and width are bigger than the values specified by
        ///  <see cref="MinClipWidth"/> and <see cref="MinClipHeight"/>
        /// </summary>
        private bool IsValidDraw()
        {
            if (Math.Abs(DrawStartPosition.X - DrawEndPosition.X) > MinClipWidth && Math.Abs(DrawStartPosition.Y - DrawEndPosition.Y) > MinClipHeight)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// Converts a <see cref="Bitmap"/> to a <see cref="BitmapImage"/>
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
        /// Uploads and image to imgur
        /// </summary>
        /// <param name="bitmap">The image to upload</param>
		private void UploadToImgur(Bitmap bitmap)
		{
			var keys = ImgurSettingsService.Load();
			var link = new ImgurService(keys.ClientId)
					.UploadImageAnonymously(bitmap);
			if (link != null)
				System.Diagnostics.Process.Start(link);
			else
				MessageBox.Show("An error occured while uploading to imgur!. Please Check your internet connection and try again.");
		}

        #region Button-Click Handlers

        /// <summary>
        /// Toggles snipping on and off 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleScreenSnippingClick(object sender,RoutedEventArgs e)
        {
            IsAllowedToSnipp = IsAllowedToSnipp == true ? false : true;
        }


        /// <summary>
        /// Takes a full screenshot 
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
        /// Uploads an image to imgur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void UploadToImgurClick(object sender, RoutedEventArgs e)
		{
			this.Hide();
			if (OnWindowCapture != null)
				OnWindowCapture(this, null);

			var b = GetScreenSnipp();
			UploadToImgur(b);
		}

        /// <summary>
        /// Saves a snipp to the clients PC
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void SaveToFileClick(object sender, RoutedEventArgs e)
		{
			var b = GetScreenSnipp();

			var dialog = new SaveFileDialog {Filter = "PNG Images | *.png"};
			if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

			b.Save(dialog.FileName);
            Close();
        }


        /// <summary>
        /// Closes snippur 
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

        #endregion
    }
}
