using Manga_Trackr.Pages;
using Manga_Trackr.ViewModels;
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

namespace Manga_Trackr
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow, IDisposable
    {
        MainViewModel _viewModel;
        bool _isDisposed;

        public MainWindow()
        {
            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;
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

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void MetroWindow_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Manga.Themes.ThemeManager.Instance.UnRegister(this);
        }
    }
}
