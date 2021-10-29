using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.ObjectModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Globalization;
using ModernWpf.Controls;
using ListView = System.Windows.Controls.ListView;
using ListViewItem = System.Windows.Controls.ListViewItem;

namespace BackItUp.Pages
{
    public class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ListViewItem item = (ListViewItem)value;
            var listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
            int index = listView.ItemContainerGenerator.IndexFromContainer(item) + 1;
            return index.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("Gu Khale");
        }
    }
    /// <summary>
    /// Interaction logic for BackUpList.xaml
    /// </summary>
    public partial class BackUpList : System.Windows.Controls.Page
    {
        private ObservableCollection<FilePathString> listItems = new ObservableCollection<FilePathString>();
        public BackUpList()
        {
            InitializeComponent();

            loadBackupList();
        }

        private void loadBackupList()
        {
            String dir_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BackItUp\\locals";
            String log_file_path = dir_path + "\\fileListPreferences.json";

            String dataString = File.ReadAllText(log_file_path);

            JObject data = JObject.Parse(dataString);
            IList<JToken> results = data["paths"].Children().ToList();

            if (results.Count == 0)
            {
                pathsListView.Visibility = Visibility.Hidden;
                noPathTitle.Visibility = Visibility.Visible;
                clearBackUpList.Visibility = Visibility.Hidden;
            }
            else
            {
                pathsListView.Visibility = Visibility.Visible;
                clearBackUpList.Visibility = Visibility.Visible;
                noPathTitle.Visibility = Visibility.Hidden;

                pathsListView.ItemsSource = listItems;

                listItems.Clear();
                foreach (String result in results)
                {
                    if (result != null)
                    listItems.Add(new FilePathString() {
                        path = result,
                        fileName = result.Substring(result.LastIndexOf("\\") + 1)
                    });
                }
            }
        }

        private bool isContaining(String[] arr, String word)
        {
            bool contains = false;

            foreach(String element in arr)
            {
                if (element == word)
                {
                    contains = true;
                    break;
                }
            }

            return contains;
        }

        private void addFilesToBackupList_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Multiselect = true;
            dialog.Title = "Select files";
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                String[] fileNames = dialog.FileNames.ToArray();

                // to get the paths(array) from the local file
                String dir_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BackItUp\\locals";
                String log_file_path = dir_path + "\\fileListPreferences.json";
                String dataString = File.ReadAllText(log_file_path);
                FileListPreferences fileListPreferences = JsonConvert.DeserializeObject<FileListPreferences>(dataString);

                // to add the new path to the array
                String[] temp = new string[fileListPreferences.paths.Length + fileNames.Length];
                for (int i = 0; i < fileNames.Length; i++)
                {
                    temp[i] = fileNames[i];
                }
                for (int i = fileNames.Length; i < (fileListPreferences.paths.Length + fileNames.Length); i++)
                {
                    if (!isContaining(temp, fileListPreferences.paths[i - fileNames.Length]))
                    {
                        temp[i] = fileListPreferences.paths[i - fileNames.Length];
                    }
                }
                fileListPreferences.paths = temp;

                // saving the array
                String dataStringToWrite = JsonConvert.SerializeObject(fileListPreferences);
                File.WriteAllText(log_file_path, dataStringToWrite);

                loadBackupList();
            }
        }

        private void addFoldersToBackupList_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Multiselect = true;
            dialog.Title = "Select Folders";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                String[] fileNames = dialog.FileNames.ToArray();

                // to get the paths(array) from the local file
                String dir_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BackItUp\\locals";
                String log_file_path = dir_path + "\\fileListPreferences.json";
                String dataString = File.ReadAllText(log_file_path);
                FileListPreferences fileListPreferences = JsonConvert.DeserializeObject<FileListPreferences>(dataString);

                // to add the new path to the array
                String[] temp = new string[fileListPreferences.paths.Length + fileNames.Length];
                for (int i = 0; i < fileNames.Length; i++)
                {
                    temp[i] = fileNames[i];
                }
                for (int i = fileNames.Length; i < (fileListPreferences.paths.Length + fileNames.Length); i++)
                {
                    if (!isContaining(temp, fileListPreferences.paths[i - fileNames.Length]))
                    {
                        temp[i] = fileListPreferences.paths[i - fileNames.Length];
                    }
                }
                fileListPreferences.paths = temp;

                // saving the array
                String dataStringToWrite = JsonConvert.SerializeObject(fileListPreferences);
                File.WriteAllText(log_file_path, dataStringToWrite);

                loadBackupList();
            }
        }

        private void clearBackUpList_Click(object sender, RoutedEventArgs e)
        {

            var YesNoDialogPage = new YesNoDialog();
            var dialog = new ContentDialog()
            {

                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,

            };
            dialog.Content = new System.Windows.Controls.Frame()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Content = YesNoDialogPage
            };
            YesNoDialogPage.YesButton.Click += (_, __) =>
            {

                FileListPreferences fileListPreferences = new FileListPreferences();
                fileListPreferences.paths = new string[0] { };
                String dataString = JsonConvert.SerializeObject(fileListPreferences);

                String dir_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BackItUp\\locals";
                String log_file_path = dir_path + "\\fileListPreferences.json";

                File.WriteAllText(log_file_path, dataString);

                loadBackupList();
                dialog.Hide();
            };
            YesNoDialogPage.NoButton.Click += (_, __) =>
            {
                dialog.Hide();
            };
            dialog.ShowAsync();
        }

        private void deleteListPath_Click(object sender, RoutedEventArgs e)
        {
            Button removePathButton = sender as Button;
            var listRowData = removePathButton.DataContext as FilePathString;

            listItems.Remove(listRowData);

            // if list empty
            if (listItems.Count == 0)
            {
                pathsListView.Visibility = Visibility.Hidden;
                noPathTitle.Visibility = Visibility.Visible;
                clearBackUpList.Visibility = Visibility.Hidden;
            }
            else
            {
                pathsListView.Visibility = Visibility.Visible;
                clearBackUpList.Visibility = Visibility.Visible;
                noPathTitle.Visibility = Visibility.Hidden;
            }

            new Thread(() =>
            {
                var listToWrite = new FileListPreferences();
                listToWrite.paths = new string[listItems.Count];
                int count = 0;
                foreach (var listRow in listItems)
                {
                    listToWrite.paths[count] = listRow.path;
                    count++;
                }
                String dir_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BackItUp\\locals";
                String log_file_path = dir_path + "\\fileListPreferences.json";

                String dataStringToWrite = JsonConvert.SerializeObject(listToWrite);
                File.WriteAllText(log_file_path, dataStringToWrite);
            }).Start();
        }

    }
}
