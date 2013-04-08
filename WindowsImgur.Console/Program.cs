using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsImgur.Core.Services;

namespace WindowsImgur.Console
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
                var link = new ImgurService(ConfigurationManager.AppSettings["Client-ID"], ConfigurationManager.AppSettings["Client-Secret"])
                    .UploadImageAnonymously(file);
                if (link != null)
                    System.Diagnostics.Process.Start(link);
                else
                    System.Console.WriteLine("An error with imgur has occured.");
            }

            if (setkey)
            {
                if (!new RegistryKeyService().SetImgurKey())
                {
                    System.Console.WriteLine("Editing the keys requires administrative permissions.  Try a runas command. ");
                }
            }

            if (deletekey)
            {
                if (!new RegistryKeyService().DeleteImgurKey())
                {
                    System.Console.WriteLine("Editing the keys requires administrative permissions.  Try a runas command. ");
                }
            }
        }
    }
}