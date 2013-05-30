using System.IO;
using System.Web.Script.Serialization;

namespace Snippur.Core.Services
{
    public class ImgurSettingsService
    {
        public const string FILE = "imgur.config";

        public string ClientId { get; set; }

        public static ImgurSettingsService Load()
        {
            if (!File.Exists(FILE))
                File.WriteAllText(FILE, new JavaScriptSerializer().Serialize(new ImgurSettingsService()));
            return new JavaScriptSerializer().Deserialize<ImgurSettingsService>(File.ReadAllText(FILE));
        }

        public void Save()
        {
            File.WriteAllText(FILE, new JavaScriptSerializer().Serialize(this));
        }
    }
}
