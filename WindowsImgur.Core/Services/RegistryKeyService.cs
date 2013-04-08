using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace WindowsImgur.Core.Services
{
    public class RegistryKeyService
    {

        public bool SetImgurKey()
        {
            if (!IsUserAdministrator())
                return false;

            var root = Registry.LocalMachine;
            var shell = root.OpenSubKey(@"Software\Classes\Paint.Picture\shell", true);
            var sendkey = shell.CreateSubKey("Upload to Imgur");
            var command = sendkey.CreateSubKey("command");
            command.SetValue("", string.Format("{0} -f \"%1\"", Environment.GetCommandLineArgs()[0]));
            return true;
        }

        public bool DeleteImgurKey()
        {
            if (!IsUserAdministrator())
                return false;

            var shell= Registry.LocalMachine.OpenSubKey(@"Software\Classes\Paint.Picture\shell", true);
            shell.DeleteSubKey(@"Upload to Imgur\command");
            shell.DeleteSubKey("Upload to Imgur");
            return true;
        }

        public bool IsUserAdministrator()
        {
            //bool value to hold our return value
            bool isAdmin;
            try
            {
                //get the currently logged in user
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException ex)
            {
                isAdmin = false;
            }
            catch (Exception ex)
            {
                isAdmin = false;
            }
            return isAdmin;
        }
    }
}
