using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.VisualStyles;

namespace RaidBossChecker
{
    public static class RegKey
    {
        static MainWindow mainWindow = Application.Current.Windows[0] as MainWindow; // object main window to have access to controls from main window

        public static void Load()
        {
            using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\RaidBossChecker"))
            {
                // need to test
                if (registryKey.GetValue("First Start") == null)
                {
                    // Load default data:
                    registryKey.SetValue("First Start", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")); // current time (only once)
                    registryKey.SetValue("Volume", "0,5");
                    registryKey.SetValue("Sound", "Police");
                    registryKey.SetValue("Server", 0); // server number
                    registryKey.SetValue("SecondsToUpdate", 1); // seconds to update list (INDEX)
                    mainWindow.GuidePage();
                }
            }
        }

        public static object Get(string value)
        {
            using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\RaidBossChecker")) 
            {
                return registryKey.GetValue(value); 
            }
        }

        public static void Set(string value, object element)
        {
            using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\RaidBossChecker"))
            {
                registryKey.SetValue(value, element);
            }
        }

        
    }
}
