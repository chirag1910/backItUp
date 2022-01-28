using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Windows;
using ICSharpCode.SharpZipLib.Zip.Compression;
using BackItUp.Pages;

namespace BackItUp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        //public static String dir_path;
        public void appStart(object o, StartupEventArgs s)
        {

            //string[] files = { "F:\\program" };
            //new Zipper().Zip(files, "F:\\backup.zip", new Zipper.ProgressListener() { });

            string[] args = Environment.GetCommandLineArgs();
            var arguments = new Dictionary<string, string>();

            for (int i = 1; i < args.Length; i += 2)
            {

                try
                {
                    string arg = args[i].Substring(args[i].IndexOf("--") + 2);
                    arguments.Add(arg, args[i + 1]);
                }
                catch
                {

                }
            }

            initializeStaticFile();
            setupAutoBackupTask();
            if (!arguments.ContainsKey("startzip"))
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                var showWindow = Boolean.Parse(arguments.GetValueOrDefault("showwindow", Boolean.TrueString));
                var pickFileFromRegistry = Boolean.Parse(arguments.GetValueOrDefault("pickFromReg", Boolean.FalseString));
                var window = new zippingWindow(showWindow, pickFileFromRegistry);
                if (showWindow)
                {
                    window.Show();
                }
            }
        }

        private void setupAutoBackupTask()
        {
            String dir_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BackItUp\\locals";
            String settings_file_path = dir_path + "\\settingsPreferences.json";

            String dataString = File.ReadAllText(settings_file_path);
            SettingsPreferences data = JsonConvert.DeserializeObject<SettingsPreferences>(dataString);

            if (data.autoBackup)
            {
                settings.turnOnAutoBackup(data.backupTime);
            }
            else
            {
                settings.turnOffAutoBackup();
            }
        }

        private void initializeStaticFile()
        {
            String dir_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BackItUp\\locals";
            String settings_file_path = dir_path + "\\settingsPreferences.json";
            String log_file_path = dir_path + "\\logsPreferences.json";
            String fileList_file_path = dir_path + "\\fileListPreferences.json";

            if (!Directory.Exists(dir_path))
            {
                Directory.CreateDirectory(dir_path);
            }

            if (!File.Exists(settings_file_path))
            {
                String dowloadPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "{374DE290-123F-4565-9164-39C4925E467B}", String.Empty).ToString();

                SettingsPreferences settingsPreferences = new SettingsPreferences();
                settingsPreferences.autoBackup = false;
                settingsPreferences.saveLocation = dowloadPath;
                settingsPreferences.saveAs = "BackItUp backup";

                settingsPreferences.compressionLevel = Deflater.NO_COMPRESSION;
                settingsPreferences.backupTime = new DateTime(2001, 1, 1, 0, 0, 0);
                settingsPreferences.ignore = new String[0] { };

                String dataString = JsonConvert.SerializeObject(settingsPreferences);

                try
                {
                    FileStream file = File.Create(settings_file_path);
                    file.Close();
                    File.WriteAllText(settings_file_path, dataString);
                }
                catch (System.IO.IOException)
                {
                    initializeStaticFile();
                }

            }

            if (!File.Exists(log_file_path))
            {
                LogsPreferences logsPreferences = new LogsPreferences();
                logsPreferences.logs = new BackupLogResult[0] { };

                String dataString = JsonConvert.SerializeObject(logsPreferences);

                try
                {
                    FileStream file = File.Create(log_file_path);
                    file.Close();
                    File.WriteAllText(log_file_path, dataString);
                }
                catch (System.IO.IOException)
                {
                    initializeStaticFile();
                }
            }

            if (!File.Exists(fileList_file_path))
            {
                FileListPreferences fileListPreferences = new FileListPreferences();
                fileListPreferences.paths = new String[0] { };

                String dataString = JsonConvert.SerializeObject(fileListPreferences);

                try
                {
                    FileStream file = File.Create(fileList_file_path);
                    file.Close();
                    File.WriteAllText(fileList_file_path, dataString);
                }
                catch (System.IO.IOException)
                {
                    initializeStaticFile();
                }
            }
        }
    }
}
