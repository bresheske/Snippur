using Snippur.Core.Services.ImgurObjects;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;

namespace Snippur.Core.Services
{
    /// <summary>
    /// Simple Imgur Upload service which is in charge of uploading
    /// images and returning URLs.
    /// </summary>
    public class ImgurService
    {
        private readonly string _appclient;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="appClient">
        /// Found in your Imgur Account Settings.
        /// </param>
        public ImgurService(string appClient)
        {
            _appclient = appClient;
        }

        /// <summary>
        /// Uploads an image to Imgur, returns the URL string if successful, null if not.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public string UploadImageAnonymously(Bitmap image)
        {
            //Check if there is an active internet connection
            if (!Helper.HasActiveConnection())
                return null;
                
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                bytes = ms.ToArray();
            }

            var data = System.Text.Encoding.UTF8.GetBytes("image=" + HttpUtility.UrlEncode(Convert.ToBase64String(bytes)));

            var req = WebRequest.CreateHttp("https://api.imgur.com/3/image");
            req.Method = "POST";
            req.Accept = "application/json";
            req.Headers.Add("Authorization", "Client-ID " + _appclient);
            req.KeepAlive = true;
            req.ContentType = "application/x-www-form-urlencoded";
            req.ServicePoint.Expect100Continue = false;
            req.GetRequestStream().Write(data, 0, data.Length);

            try
            {
                var response = new StreamReader(req.GetResponse().GetResponseStream()).ReadToEnd();
                return new JavaScriptSerializer().Deserialize<UploadResponse>(response).data.link;
            }
            catch (WebException)
            {
                return null;
            }

        }

        /// <summary>
        /// Uploads an image to Imgur, returns the URL string if successful, null if not.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public string UploadImageAnonymously(string file)
        {
            var image = (Bitmap)(Image.FromFile(file));
            return UploadImageAnonymously(image);
        }

    }
}
