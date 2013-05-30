using NDesk.Options;
using Snippur.Core.Services;

namespace Snippur.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var file = "";
            var setkey = false;
            var deletekey = false;
            var set = new OptionSet()
            {
                {"f|file=", o => file = o},
                {"srk", o => setkey = true},
                {"drk", o => deletekey = true},
            };

            set.Parse(args);

            if (!string.IsNullOrEmpty(file))
            {
                var link = new ImgurService(ImgurSettingsService.Load().ClientId)
                    .UploadImageAnonymously(file);
                if (link != null)
                    System.Diagnostics.Process.Start(link);
                else
                    System.Console.WriteLine("An error with imgur has occured.");
            }

            if (setkey)
            {
                if (!new KeyService().SetImgurKey())
                {
                    System.Console.WriteLine("Editing the keys requires administrative permissions.  Execute CMD as an Administrator.");
                }
            }

            if (deletekey)
            {
                if (!new KeyService().DeleteImgurKey())
                {
                    System.Console.WriteLine("Editing the keys requires administrative permissions.  Execute CMD as an Administrator.");
                }
            }
        }
    }
}