namespace Snippur.Core.Services.ImgurObjects
{
    public class UploadResponse
    {
        public bool success { get; set; }
        public int status { get; set; }
        public UploadData data { get; set; }
    }
}
