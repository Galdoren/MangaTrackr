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
    /// Interaction logic for InfoPage.xaml
    /// </summary>
    public partial class InfoPage : UserControl
    {
        InfoViewModel _dataContext;

        public InfoPage(MangaModel model)
        {
            _dataContext = new InfoViewModel(model);
            this.DataContext = _dataContext;
            InitializeComponent();
        }
    }
}
