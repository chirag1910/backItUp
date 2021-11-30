using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Diagnostics;
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


                    var fileListKey = Registry.CurrentUser.CreateSubKey("Software\\BackItUp\\ZipCommand\\filelist");
                    var zipPathKey = Registry.CurrentUser.CreateSubKey("Software\\BackItUp\\ZipCommand\\zipPath");
                    fileListKey.SetValue("", fileNames.Length);
                    for (int counter = 0; counter < fileNames.Length; counter++)
                    {
                        fileListKey.SetValue(counter.ToString(), fileNames[counter]);
                    }
                    zipPathKey.SetValue("", dlg.FileName);






                    String applicationPath = AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName;
                    var info = new ProcessStartInfo();
                    info.CreateNoWindow = false;
                    info.UseShellExecute = true;
                    info.FileName = applicationPath;
                    info.Arguments = "--startzip anyvaluedoesntmatter --showwindow " + Boolean.TrueString + " --pickFromReg " + Boolean.TrueString;
                    Process.Start(info);

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
                    var fileListKey = Registry.CurrentUser.CreateSubKey("Software\\BackItUp\\ZipCommand\\filelist");
                    var zipPathKey = Registry.CurrentUser.CreateSubKey("Software\\BackItUp\\ZipCommand\\zipPath");
                    fileListKey.SetValue("", fileNames.Length);
                    for (int counter = 0; counter < fileNames.Length; counter++)
                    {
                        fileListKey.SetValue(counter.ToString(), fileNames[counter]);
                    }
                    zipPathKey.SetValue("", dlg.FileName);






                    String applicationPath = AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName;
                    var info = new ProcessStartInfo();
                    info.CreateNoWindow = false;
                    info.UseShellExecute = true;
                    info.FileName = applicationPath;
                    info.Arguments = "--startzip anyvaluedoesntmatter --showwindow " + Boolean.TrueString + " --pickFromReg " + Boolean.TrueString;
                    Process.Start(info);

                }
            }
        }
    }
}
