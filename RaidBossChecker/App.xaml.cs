using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RaidBossChecker
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "RaidBossChecker";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                Application.Current.Shutdown();
            }

            base.OnStartup(e);
        }
    }
}
