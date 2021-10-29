using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BackItUp.Pages
{
    /// <summary>
    /// Interaction logic for unzip.xaml
    /// </summary>
    public partial class unzip : Page
    {
        public unzip()
        {
            InitializeComponent();
        }

        private void selectZipFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "zip file (*.zip)|*.zip";
            dialog.Title = "Select a zip file to unzip";
            if (dialog.ShowDialog() == true)
            {
                String zipPath = dialog.FileName;

                CommonOpenFileDialog extractAtDialog = new CommonOpenFileDialog();
                extractAtDialog.Multiselect = false;
                extractAtDialog.IsFolderPicker = true;
                extractAtDialog.Title = "Extract at";

                if (extractAtDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    UnzippingWindowTemp window = new UnzippingWindowTemp(zipPath, extractAtDialog.FileName);
                    window.Show();
                }
            }
        }
    }
}
