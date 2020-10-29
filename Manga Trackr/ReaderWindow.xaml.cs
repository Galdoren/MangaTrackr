using Manga.Structures.Models;
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
using System.Windows.Shapes;

namespace Manga_Trackr
{
    /// <summary>
    /// Interaction logic for ReaderWindow.xaml
    /// </summary>
    public partial class ReaderWindow : MahApps.Metro.Controls.MetroWindow
    {
        ReaderViewModel _viewModel;
        bool _isDisposed;

        public ReaderWindow(MangaModel model)
        {
            _isDisposed = false;
            _viewModel = new ReaderViewModel(model);
            this.DataContext = _viewModel;
            InitializeComponent();
            Manga.Themes.ThemeManager.Instance.Register(this);
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
                    this.DataContext = null;
                    Manga.Themes.ThemeManager.Instance.UnRegister(this);
                }
            }
            _isDisposed = true;
        }

        private void MetroWindow_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Dispose();
        }
    }
}
