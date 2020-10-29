using MahApps.Metro.Controls;
using Manga.Interfaces;
using Manga.Reader.ViewModels;
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

namespace Manga.Reader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ReaderWindow : MetroWindow
    {
        ReaderViewModel _viewModel;

        public ReaderWindow(IMangaModel model)
        {
            _viewModel = new ReaderViewModel(model);
            this.DataContext = _viewModel;
            InitializeComponent();
        }
    }
}
