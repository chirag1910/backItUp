using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;


namespace BackItUp.Pages
{
    /// <summary>
    /// Interaction logic for zip.xaml
    /// </summary>
    public partial class zip : Page
    {

        public zip()
        {
            InitializeComponent();
        }

        private void selectFilesToZip_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Multiselect = true;
            dialog.Title = "Select files";
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                String[] fileNames = dialog.FileNames.ToArray();

                FileInfo fi = new FileInfo(fileNames[0]);

                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = fi.Name.Substring(0, fi.Name.LastIndexOf(fi.Extension));
                dlg.DefaultExt = ".zip";
                dlg.Filter = "Zip file (.zip)|*.zip";

                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    zippingWindowTemp window = new zippingWindowTemp(fileNames, dlg.FileName);
                    window.Show();
                }

                
            }
        }

        private void addFolderToBackupList_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Multiselect = true;
            dialog.Title = "Select Folders";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                String[] fileNames = dialog.FileNames.ToArray();

                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = fileNames[0].Substring(fileNames[0].LastIndexOf("\\")+1);
                dlg.DefaultExt = ".zip";
                dlg.Filter = "Zip file (.zip)|*.zip";

                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    zippingWindowTemp window = new zippingWindowTemp(fileNames, dlg.FileName);
                    window.Show();
                }
            }
        }
    }
}
