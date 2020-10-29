using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Manga_Trackr
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var a = Manga.Database.DatabaseEngine.Instance;

            this.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
           /* MahApps.Metro.Accent accent = new MahApps.Metro.Accent("Gray", new Uri("pack://application:,,,/Themes/Gray.xaml"));
            MahApps.Metro.ThemeManager.ChangeTheme(this, accent, MahApps.Metro.Theme.Light);*/
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Manga.Structures.Properties.Settings.Default.DownloadCount = 0;
            Manga.Structures.Properties.Settings.Default.Save();
        }
    }
}
