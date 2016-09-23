using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snippur.Core.Services
{
    public class Helper
    {

        /// <summary>
        /// Returns a boolean specifying if there is an active internet connection 
        /// </summary>
        /// <returns></returns>
        public static bool HasActiveConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    //Could be any webpage just went with this one because it's well....google :)
                    using (var stream = client.OpenRead("http://www.google.com"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
