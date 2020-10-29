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

namespace Manga_Trackr.Pages
{
    /// <summary>
    /// Interaction logic for CatalogPage.xaml
    /// </summary>
    public partial class CatalogPage : UserControl, IDisposable
    {
        bool _isDisposed;

        public CatalogPage()
        {
            InitializeComponent();
            Manga.Themes.ThemeManager.Instance.Register(this);
            _isDisposed = false;
        }

        private void ListView_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            var element = ((FrameworkElement)e.OriginalSource).DataContext as Manga.Structures.Models.MangaModel;
            if (element != null)
            {
                (this.DataContext as CatalogViewModel).OpenCommand.Execute(null);
                /*
                Manga.Framework.ParallelWorker worker = new Manga.Framework.ParallelWorker(3);

                worker.EnqueueFileToDownload(new Manga.Framework.DownloadFile() 
                { PathToSave = "D:\\Manga\\1.jpg", Url = "http://c.mfcdn.net/store/manga/246/34-309.0/compressed/v001.jpg" });

                worker.EnqueueFileToDownload(new Manga.Framework.DownloadFile()
                { PathToSave = "D:\\Manga\\2.jpg", Url = "http://c.mfcdn.net/store/manga/246/34-309.0/compressed/v002.jpg" });

                worker.EnqueueFileToDownload(new Manga.Framework.DownloadFile() 
                { PathToSave = "D:\\Manga\\3.jpg", Url = "http://c.mfcdn.net/store/manga/246/34-309.0/compressed/v003.jpg" });
                */
            }
        }

        private void TextBox_KeyUp_1(object sender, KeyEventArgs e)
        {
            /*if (e.Key == Key.Enter)
                (this.DataContext as CatalogViewModel).SearchCommand.Execute(null);
            else if (e.Key == Key.Escape)
            {
                if (searchBox.Text != null)
                {
                    searchBox.SetValue(TextBox.TextProperty, null);
                    (this.DataContext as CatalogViewModel).SearchCommand.Execute(null);
                }
            }*/
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

        private void searchBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            (this.DataContext as CatalogViewModel).SearchCommand.Execute(null);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
