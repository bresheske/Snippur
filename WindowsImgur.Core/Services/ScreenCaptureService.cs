using System.Drawing;

namespace Snippur.Core.Services
{
    public class ScreenCaptureService
    {
        public Image CaptureScreen(System.Windows.Forms.Screen screen)
        {
            var bitmap = new Bitmap(screen.Bounds.Width, screen.Bounds.Height);
            using (var g = Graphics.FromImage(bitmap))
                g.CopyFromScreen(screen.Bounds.X, screen.Bounds.Y, 0, 0, new Size(screen.Bounds.Width, screen.Bounds.Height));
            return bitmap;
        }
    }
}
