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
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : MahApps.Metro.Controls.MetroWindow
    {
        SettingsViewModel _viewModel;

        public SettingsWindow()
        {
            _viewModel = new SettingsViewModel();
            this.DataContext = _viewModel;
            InitializeComponent();
            Manga.Themes.ThemeManager.Instance.Register(this);
        }

        private void MetroWindow_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Manga.Themes.ThemeManager.Instance.UnRegister(this);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
