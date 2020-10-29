using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Manga_Trackr.Pages
{
    /// <summary>
    /// Interaction logic for AllPage.xaml
    /// </summary>
    public partial class AllPage : UserControl
    {
        bool _isDisposed;

        public AllPage()
        {
            InitializeComponent();
            Manga.Themes.ThemeManager.Instance.Register(this);
            _isDisposed = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Manga.Themes.ThemeManager.Instance.UnRegister(this);
                }
            }
            _isDisposed = true;
        }
    }
}
