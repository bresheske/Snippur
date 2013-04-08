using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsImgur.Core.Services.ImgurObjects
{
    public class UploadResponse
    {
        public bool success { get; set; }
        public int status { get; set; }
        public UploadData data { get; set; }
    }
}
