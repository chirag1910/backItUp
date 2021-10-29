using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BackItUp.Pages
{
    /// <summary>
    /// Interaction logic for DockLarge.xaml
    /// </summary>
    public partial class Navbar : Page
    {
        private Button[] navButtons = new Button[5];
        public Navbar()
        {
            InitializeComponent();

            navButtons[0] = homeButton;
            navButtons[1] = backupButton;
            navButtons[2] = zipButton;
            navButtons[3] = unzipButton;
            navButtons[4] = settingsButton;
        }

        private void settransparent()
        {
            for (int i = 0; i < navButtons.Length; i++)
            {
                navButtons[i].Background = Brushes.Transparent;
            }
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainFrame.Source = new Uri("/Pages/Home.xaml", UriKind.Relative);
            settransparent();
            homeButton.Background = Brushes.Gray;
        }

        private void backupButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainFrame.Source = new Uri("/Pages/BackUpList.xaml", UriKind.Relative);
            settransparent();
            backupButton.Background = Brushes.Gray;
        }

        private void zipButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainFrame.Source = new Uri("/Pages/Zip.xaml", UriKind.Relative);
            settransparent();
            zipButton.Background = Brushes.Gray;
        }

        private void unzipButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainFrame.Source = new Uri("/Pages/Unzip.xaml", UriKind.Relative);
            settransparent();
            unzipButton.Background = Brushes.Gray;
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainFrame.Source = new Uri("/Pages/Settings.xaml", UriKind.Relative);
            settransparent();
            settingsButton.Background = Brushes.Gray;
        }
    }
}
