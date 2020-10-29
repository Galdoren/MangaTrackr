using MahApps.Metro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Manga.Themes
{
    public class ThemeManager
    {
        #region Members

        List<FrameworkElement> _items;
        private readonly object _semaphore;

        static ThemeManager _instance;

        private static readonly ResourceDictionary LightResource = new ResourceDictionary { Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/BaseLight.xaml") };
        private static readonly ResourceDictionary DarkResource = new ResourceDictionary { Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/BaseDark.xaml") };

        myThemes _current;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Instance of myThemeManager object.
        /// </summary>
        public static ThemeManager Instance
        { get { return _instance; } }

        #endregion

        #region Constructor

        static ThemeManager()
        {
            _instance = new ThemeManager();
        }

        private ThemeManager()
        {
            _semaphore = new object();
            _items = new List<FrameworkElement>();
            _current = myThemes.Gray;
        }

        #endregion

        public void Register(FrameworkElement element)
        {
            lock (_semaphore)
            {
                _items.Add(element);
                ChangeTheme(element, _current);
            }
        }

        public void UnRegister(FrameworkElement element)
        {
            lock (_semaphore)
                _items.Remove(element);
        }

        public void ChangeTheme(FrameworkElement element, myThemes theme)
        {
            lock (_semaphore)
            {
                lock (_semaphore)
                {
                    if (theme == myThemes.Gray)
                    {
                        Accent accent = new Accent("Gray", new Uri("pack://application:,,,/Manga.Themes;component/Themes/Gray.xaml"));
                        ChangeTheme(element, accent, Theme.Light);
                    }
                    else if (theme == myThemes.White)
                    {
                        ChangeTheme(element, new Accent("Blue", new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/Blue.xaml")), Theme.Light);
                        ChangeTheme(element, new Accent("Blue", new Uri("pack://application:,,,/Manga.Themes;component/Themes/White.xaml")), Theme.Light);
                    }
                    else if (theme == myThemes.Red)
                    {
                        ChangeTheme(element, MahApps.Metro.ThemeManager.DefaultAccents.First(a => a.Name == "Red"), Theme.Light);
                    }
                }
            }
        }

        public void ChangeTheme(myThemes theme)
        {
            lock (_semaphore)
            {
                if (theme == myThemes.Gray)
                {
                    Accent accent = new Accent("Gray", new Uri("pack://application:,,,/Manga.Themes;component/Themes/Gray.xaml"));
                    foreach (FrameworkElement element in _items)
                    {
                        ChangeTheme(element, accent, Theme.Light);
                    }
                }
                else if (theme == myThemes.White)
                {
                    foreach (FrameworkElement element in _items)
                    {
                        ChangeTheme(element, new Accent("Blue", new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/Blue.xaml")), Theme.Light);
                        ChangeTheme(element, new Accent("Blue", new Uri("pack://application:,,,/Manga.Themes;component/Themes/White.xaml")), Theme.Light);
                    }
                }
                else if (theme == myThemes.Red)
                {
                    foreach (FrameworkElement element in _items)
                    {
                        ChangeTheme(element, MahApps.Metro.ThemeManager.DefaultAccents.First(a => a.Name == "Red"), Theme.Light);
                    }
                }
                _current = theme;
            }
        }

        private void ChangeTheme(FrameworkElement element, Accent accent, Theme theme)
        {
            var themeResource = (theme == Theme.Light) ? LightResource : DarkResource;
            ApplyResourceDictionary(themeResource, element.Resources);
            ApplyResourceDictionary(accent.Resources, element.Resources);
        }

        private static void ApplyResourceDictionary(ResourceDictionary newRd, ResourceDictionary oldRd)
        {            
            foreach (DictionaryEntry r in newRd)
            {
                if (oldRd.Contains(r.Key))
                    oldRd.Remove(r.Key);

                oldRd.Add(r.Key, r.Value);
            }
        }
    }

    public enum myThemes
    {
        Gray,
        White,
        Red
    }
}
