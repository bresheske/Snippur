using Microsoft.Win32;
using System;
using System.Security.Principal;

namespace WindowsImgur.Core.Services
{
    public class KeyService
    {

        public bool SetImgurKey()
        {
            if (!IsUserAdministrator())
                return false;

            /* Registry */
            var root = Registry.LocalMachine;
            var shell = root.OpenSubKey(@"Software\Classes\Paint.Picture\shell", true);
            SetCommandKeys(shell);
            shell = root.OpenSubKey(@"Software\Classes\pngfile\shell", true);
            SetCommandKeys(shell);

            return true;
        }

        private void SetCommandKeys(RegistryKey shell)
        {
            var sendkey = shell.CreateSubKey("Upload to Imgur");
            var command = sendkey.CreateSubKey("command");
            command.SetValue("", string.Format("{0} -f \"%1\"", Environment.GetCommandLineArgs()[0]));
        }

        public bool DeleteImgurKey()
        {
            if (!IsUserAdministrator())
                return false;

            var shell= Registry.LocalMachine.OpenSubKey(@"Software\Classes\Paint.Picture\shell", true);
            shell.DeleteSubKey(@"Upload to Imgur\command");
            shell.DeleteSubKey("Upload to Imgur");

            shell = Registry.LocalMachine.OpenSubKey(@"Software\Classes\pngfile\shell", true);
            shell.DeleteSubKey(@"Upload to Imgur\command");
            shell.DeleteSubKey("Upload to Imgur");
            return true;
        }

        public bool IsUserAdministrator()
        {
            bool isAdmin;
            try
            {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException)
            {
                isAdmin = false;
            }
            catch (Exception)
            {
                isAdmin = false;
            }
            return isAdmin;
        }
    }
}
