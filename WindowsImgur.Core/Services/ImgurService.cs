using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using WindowsImgur.Core.Services.ImgurObjects;

namespace WindowsImgur.Core.Services
{
    public class ImgurService
    {
        private readonly string _appclient;
        private readonly string _appsecret;

        public ImgurService(string appClient, string appSecret)
        {
            _appclient = appClient;
            _appsecret = appSecret;
        }

        public string UploadImageAnonymously(string file)
        {
            var image = (Bitmap)(Image.FromFile(file));
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                image.Save(ms, image.RawFormat);
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
            catch (WebException ex)
            {
                return null;
            }
            
        }


    }
}
