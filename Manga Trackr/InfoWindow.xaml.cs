using Manga.Structures.Models;
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
using System.Windows.Shapes;

namespace Manga_Trackr
{
    /// <summary>
    /// Interaction logic for InfoWindow.xaml
    /// </summary>
    public partial class InfoWindow : MahApps.Metro.Controls.MetroWindow
    {
        // Lazy<ChaptersPage> _chaptersPage;

        InfoWindowViewModel _viewModel;

        private String _name;

        public String ModelName
        { get { return _name; } }

        public InfoWindow(MangaModel model)
        {
            this._name = model.Name;
            this.Title = model.Name;
            _viewModel = new InfoWindowViewModel(model);
            this.DataContext = _viewModel;
            InitializeComponent();
            Manga.Themes.ThemeManager.Instance.Register(this);
        }

        private void MetroWindow_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Manga.Themes.ThemeManager.Instance.UnRegister(this);
            
        }
    }
}
