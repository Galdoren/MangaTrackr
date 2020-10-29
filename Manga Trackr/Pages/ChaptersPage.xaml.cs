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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Manga_Trackr.Pages
{
    /// <summary>
    /// Interaction logic for ChaptersPage.xaml
    /// </summary>
    public partial class ChaptersPage : UserControl
    {
        ChaptersViewModel _dataContext;

        public ChaptersPage(MangaModel model)
        {
            _dataContext = new ChaptersViewModel(model);
            this.DataContext = _dataContext;
            InitializeComponent();
            Manga.Themes.ThemeManager.Instance.Register(this);
        }
    }
}
