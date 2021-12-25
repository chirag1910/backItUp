using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32.TaskScheduler;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System.Diagnostics;

namespace BackItUp.Pages
{
    /// <summary>
    /// Interaction logic for settings.xaml
    /// </summary>
    public partial class settings : Page
    {
        DateTime? selectedTime = null;
        public settings()
        {
            InitializeComponent();
            
            loadSettings();
        }

        private void loadSettings()
        {
            String dir_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BackItUp\\locals";
            String settings_file_path = dir_path + "\\settingsPreferences.json";

            String dataString = File.ReadAllText(settings_file_path);
            SettingsPreferences data = JsonConvert.DeserializeObject<SettingsPreferences>(dataString);
            var zipPath = data.saveLocation;

            if (!zipPath.EndsWith("\\"))
            {
                zipPath += "\\";
            }

            zipPath += data.saveAs + ".zip";

            if (!File.Exists(zipPath))
            {
                openBackupButton.Visibility = Visibility.Collapsed;
            }

            var zipFileDir = data.saveLocation.Substring(data.saveLocation.LastIndexOf("\\") + 1);
            savePathTextBox.Text = zipFileDir;

            if(zipFileDir.Length == 0)
            {
                savePathTextBox.Text = data.saveLocation;
            }
            savePathTextBox.ToolTip = data.saveLocation;
            saveAsTextBox.Text = data.saveAs;
            selectedTime = (DateTime)data.backupTime;
            

            backupTimePicker.SelectedTime = selectedTime;
            if (data.compressionLevel == Deflater.NO_COMPRESSION) {
                CompressionLevelSelector.SelectedIndex = 0;
            }
            else if (data.compressionLevel == Deflater.BEST_SPEED)
            {
                CompressionLevelSelector.SelectedIndex = 1;
            }
            else if (data.compressionLevel == Deflater.DEFAULT_COMPRESSION)
            {
                CompressionLevelSelector.SelectedIndex = 2;
            }
            else if (data.compressionLevel == Deflater.BEST_COMPRESSION)
            {
                CompressionLevelSelector.SelectedIndex = 3;
            }


            if (data.autoBackup)
            {
                automaticBackupRB.SelectedIndex = 0;
                turnOnAutoBackup((DateTime)selectedTime);
            }
            else
            {
                automaticBackupRB.SelectedIndex = 1;
                turnOffAutoBackup();
            }

            // ignores
            String ignoreString = "";
            String[] ignoreArray = data.ignore != null ? data.ignore : new string[0] { };
            for(var i = 0; i < ignoreArray.Length; i++)
            {
                ignoreString += ignoreArray[i] + (i != ignoreArray.Length-1 ? "," : "");
            }
            ignoreTextBox.Text = ignoreString;

            setupListeners();
        }

        private void setupListeners()
        {
            automaticBackupRB.SelectionChanged += (_, __) =>
            {
                saveSettings();
            };
            saveAsTextBox.TextChanged += (_, __) =>
            {
                saveSettings();
            };
            ignoreTextBox.TextChanged += (_, __) =>
            {
                saveSettings();
            };
            CompressionLevelSelector.SelectionChanged += (_, __) =>
            {
                saveSettings();
            };
        }

        // remove params and hard code the fing thinf
        private void saveSettings()
        {
            String dir_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BackItUp\\locals";
            String settings_file_path = dir_path + "\\settingsPreferences.json";

            SettingsPreferences settingsPreferences = new SettingsPreferences();
            Boolean autoBackup = false;
            if (automaticBackupRB.SelectedIndex == 0)
            {
                autoBackup = true;
                turnOnAutoBackup((DateTime)selectedTime);
            }
            else
            {
                turnOffAutoBackup();
            }
            settingsPreferences.autoBackup = autoBackup;
            settingsPreferences.saveLocation = savePathTextBox.ToolTip.ToString();
            settingsPreferences.saveAs = saveAsTextBox.Text;
            settingsPreferences.backupTime = (DateTime)selectedTime;
            if (CompressionLevelSelector.SelectedIndex == 0)
            {
                settingsPreferences.compressionLevel = Deflater.NO_COMPRESSION;
            }
            else if (CompressionLevelSelector.SelectedIndex == 1)
            {
                settingsPreferences.compressionLevel = Deflater.BEST_SPEED;
            }
            else if (CompressionLevelSelector.SelectedIndex == 2)
            {
                settingsPreferences.compressionLevel = Deflater.DEFAULT_COMPRESSION;
            }
            else if (CompressionLevelSelector.SelectedIndex == 3)
            {
                settingsPreferences.compressionLevel = Deflater.BEST_COMPRESSION;
            }

            if (ignoreTextBox.Text.Trim().Length > 0)
            {
                settingsPreferences.ignore = ignoreTextBox.Text.Split(',');
            }
            else
            {
                settingsPreferences.ignore = new String[0];
            }

            String dataStringToWrite = JsonConvert.SerializeObject(settingsPreferences);
            File.WriteAllText(settings_file_path, dataStringToWrite);
        }

        private void turnOnAutoBackup(DateTime dateTime)
        {
            new Thread(() => {
                var ts = new TaskService();
                var td = ts.NewTask();
                td.RegistrationInfo.Author = "BackItUp";
                td.RegistrationInfo.Description = "BackItUp automatic backup task";
                td.Triggers.Add(new DailyTrigger { StartBoundary = dateTime, DaysInterval = 1, Enabled = true });
                td.Settings.DisallowStartIfOnBatteries = false;
                td.Settings.StopIfGoingOnBatteries = false;
                td.Settings.StartWhenAvailable = true;

                //run this application or setup path to the file
                td.Actions.Add(new ExecAction((Process.GetCurrentProcess().MainModule.FileName), "--startzip 1 --showwindow false", null));

                ts.RootFolder.RegisterTaskDefinition("BackItUp", td);
            }).Start();
            

        }
        private void turnOffAutoBackup()
        {
            new Thread(() => {
                try{
                    var ts = new TaskService();
                    var task = ts.RootFolder.GetTasks().Where(a => a.Name== "BackItUp").FirstOrDefault();
                    if (task != null)
                    {
                        ts.RootFolder.DeleteTask("BackItUp");
                    }
                }
                catch (System.IO.FileNotFoundException e)
                {

                }
            }).Start();
        }

        private void resetSaveAsName(object sender, RoutedEventArgs e)
        {
            saveAsTextBox.Text = "BackItUp backup";
            saveSettings();
        }
        private void saveLocationSelector(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Title = "Select Folder";
            dialog.IsFolderPicker = true;
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                savePathTextBox.ToolTip = dialog.FileName;
                var zipFileDir = dialog.FileName.ToString().Substring(dialog.FileName.ToString().LastIndexOf("\\") + 1);
                savePathTextBox.Text = zipFileDir;

                if (zipFileDir.Length == 0)
                {
                    savePathTextBox.Text = dialog.FileName.ToString();
                }
                saveSettings();
            }
        }

        private void TimePicker_SelectedTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            selectedTime = e.NewValue;
            if(automaticBackupRB.SelectedIndex == 0)
            {
                turnOnAutoBackup((DateTime)selectedTime);
            }
            saveSettings();
        }

        private void resetIgnore(object sender, RoutedEventArgs e)
        {
            ignoreTextBox.Text = "";
            saveSettings();
        }
        private void openBackupFolder(object sender, RoutedEventArgs e)
        {
            String dir_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BackItUp\\locals";
            String settings_file_path = dir_path + "\\settingsPreferences.json";

            String dataString = File.ReadAllText(settings_file_path);
            SettingsPreferences settingsData = JsonConvert.DeserializeObject<SettingsPreferences>(dataString);
            var zipPath = settingsData.saveLocation;

            if (!zipPath.EndsWith("\\"))
            {
                zipPath += "\\";
            }
            Process.Start("explorer.exe", zipPath);
        }
        
        private void openBackup(object sender, RoutedEventArgs e)
        {
            String dir_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BackItUp\\locals";
            String settings_file_path = dir_path + "\\settingsPreferences.json";

            String dataString = File.ReadAllText(settings_file_path);
            SettingsPreferences settingsData = JsonConvert.DeserializeObject<SettingsPreferences>(dataString);
            var zipPath = settingsData.saveLocation;

            if (!zipPath.EndsWith("\\"))
            {
                zipPath += "\\";
            }

            zipPath += settingsData.saveAs + ".zip";
            Process p = new Process();
            ProcessStartInfo ps = new ProcessStartInfo();
            ps.FileName = "CMD.exe";
            ps.Arguments = "/C \"" + zipPath + "\"";
            ps.CreateNoWindow = true;
            p.StartInfo = ps;
            p.Start();
        }
    }
}
