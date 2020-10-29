using Manga_Trackr.Mediators;
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
    /// Interaction logic for FavouritesPage.xaml
    /// </summary>
    public partial class FavouritesPage : UserControl
    {
        FavouritesViewModel _viewModel;

        public FavouritesPage()
        {
            _viewModel = new FavouritesViewModel();
            FavouritesMediator.Instance.Register(_viewModel);
            this.DataContext = _viewModel;
            InitializeComponent();
            Manga.Themes.ThemeManager.Instance.Register(this);
        }
    }
}
