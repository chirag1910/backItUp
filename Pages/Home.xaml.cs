using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Threading;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace BackItUp.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        private ObservableCollection<BackupLogResult> listItems = new ObservableCollection<BackupLogResult>();
        public Home()
        {
            InitializeComponent();
            var listener = new RegistryUtils.RegistryMonitor(Registry.CurrentUser.CreateSubKey("Software\\BackItUp"));
            listener.RegChanged += (_, __) =>
            {
                Thread thread = new Thread(new ThreadStart(loadBackUpLogs));
                thread.Start();
            };
            listener.Start();
            try
            {
                Thread thread = new Thread(new ThreadStart(loadBackUpLogs));
                thread.Start();
            }
            catch { }
        }

        private void loadBackUpLogs()
        {

            String dir_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BackItUp\\locals";
            String log_file_path = dir_path + "\\logsPreferences.json";

            String dataString = File.ReadAllText(log_file_path);

            JObject data = JObject.Parse(dataString);
            IList<JToken> results = data["logs"].Children().ToList();
            backupLogs.Dispatcher.BeginInvoke((Action)(() => {
                if (results.Count == 0)
                {
                    backupLogs.Visibility = Visibility.Hidden;
                    noLogTitle.Visibility = Visibility.Visible;
                    clearListButton.Visibility = Visibility.Hidden;
                }
                else
                {
                    backupLogs.Visibility = Visibility.Visible;
                    noLogTitle.Visibility = Visibility.Hidden;
                    clearListButton.Visibility = Visibility.Visible;

                    backupLogsListView.ItemsSource = listItems;
                    listItems.Clear();
                    foreach (JToken result in results)
                    {
                        listItems.Add(new BackupLogResult()
                        {
                            date = result["date"].ToString(),
                            filesBackedUp = result["filesBackedUp"].ToString(),
                            timeTaken = result["timeTaken"].ToString()
                        });
                    }
                }
            }));
        }

        private void backupNowButton(object sender, RoutedEventArgs e)
        {
            String applicationPath = AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName;
            var info = new ProcessStartInfo();
            info.CreateNoWindow = false;
            info.UseShellExecute = true;
            info.FileName = applicationPath;
            info.Arguments = "--startzip anyvaluedoesntmatter --showwindow " + Boolean.TrueString;
            Process.Start(info);
        }

        private void ClearListClick(object sender, RoutedEventArgs e)
        {
            String dir_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BackItUp\\locals";
            String log_file_path = dir_path + "\\logsPreferences.json";

            LogsPreferences logsPreferences = new LogsPreferences();
            logsPreferences.logs = new BackupLogResult[0] { };

            String dataStringToWrite = JsonConvert.SerializeObject(logsPreferences);
            File.WriteAllText(log_file_path, dataStringToWrite);

            var key = Registry.CurrentUser.CreateSubKey("Software\\BackItUp");
            key.SetValue("NumberOfBackups", 0.ToString());

            loadBackUpLogs();

        }
    }
}
