using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;

namespace BackItUp
{
    /// <summary>
    /// Interaction logic for zippingWindow.xaml
    /// </summary>
    public partial class zippingWindow
    {
        private string zipPath = "";
        private Zipper zipper;
        private Thread zipThread;
        private bool manuallyClose = false;
        private bool pickListFromRegistry;
        private bool showZippingWindow;
        public zippingWindow(bool showWindow, bool pickListFromRegistry = false)
        {
            InitializeComponent();
            showZippingWindow = showWindow;
            this.pickListFromRegistry = pickListFromRegistry;
            this.Closing += (s, e) =>
            {
                if (zipThread.IsAlive)
                {

                    e.Cancel = !manuallyClose;
                    this.Hide();

                    showZippingWindow = false;
                    NotifyIcon icon = new NotifyIcon();
                    icon.Visible = !manuallyClose;
                    icon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetEntryAssembly().ManifestModule.Name);
                    icon.Click += (_, __) =>
                    {
                        this.Show();
                        icon.Visible = false;
                        icon.Dispose();
                    };
                }
                else
                {
                    Environment.Exit(Environment.ExitCode);
                }
            };
            zipThread = new Thread(new ThreadStart(startZipping));
            zipThread.Start();

        }

        private void startZipping()
        {
            try
            {
                // getting files list
                String dir_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BackItUp\\locals";

                String settings_file_path = dir_path + "\\settingsPreferences.json";

                String dataString = File.ReadAllText(settings_file_path);
                SettingsPreferences settingsData = JsonConvert.DeserializeObject<SettingsPreferences>(dataString); ;


                settingsData.threads = settingsData.threads > 0 && settingsData.threads < 65 ? settingsData.threads : 1;
                settingsData.cacheSize = settingsData.cacheSize > 0 && settingsData.cacheSize < 11 ? settingsData.cacheSize : 1;
                if (!pickListFromRegistry)
                {
                    String log_file_path = dir_path + "\\fileListPreferences.json";

                    dataString = File.ReadAllText(log_file_path);

                    JObject data = JObject.Parse(dataString);
                    var results = data["paths"].Children().ToList();
                    var stringResults = new string[results.Count];
                    int i = 0;
                    foreach (String result in results)
                    {
                        stringResults[i] = result;
                        i++;
                    }

                    // getting zip file name

                    var zipPath = settingsData.saveLocation;

                    if (!zipPath.EndsWith("\\"))
                    {
                        zipPath += "\\";
                    }

                    zipPath += settingsData.saveAs + ".zip";
                    this.zipPath = zipPath;
                    zipper = new Zipper();
                    zipper.Zip(stringResults, settingsData.ignore, zipPath, settingsData.compressionLevel, settingsData.caching, settingsData.threads, settingsData.cacheSize, false, zipProgressBar, progressStatus, progressValue, fileNameInProgress, progressCancelButton, filesDone);
                    Thread.Sleep(2000);
                    this.Dispatcher.Invoke(() =>
                    {
                        if (!showZippingWindow)
                        {
                            manuallyClose = true;
                            Environment.Exit(Environment.ExitCode);
                        }
                    });
                }
                else
                {

                    var fileListKey = Registry.CurrentUser.CreateSubKey("Software\\BackItUp\\ZipCommand\\filelist");
                    var zipPathKey = Registry.CurrentUser.CreateSubKey("Software\\BackItUp\\ZipCommand\\zipPath");

                    zipPath = zipPathKey.GetValue("").ToString();
                    var numberOfFiles = Int32.Parse(fileListKey.GetValue("", "0").ToString());
                    var files = new string[numberOfFiles];
                    for (int count = 0; count < numberOfFiles; count++)
                    {
                        files[count] = fileListKey.GetValue(count.ToString()).ToString();
                    }
                    //this.zipPath = zipPath;
                    zipper = new Zipper();
                    zipper.Zip(files, new string[0], zipPath, settingsData.compressionLevel, settingsData.caching, settingsData.threads, settingsData.cacheSize, true, zipProgressBar, progressStatus, progressValue, fileNameInProgress, progressCancelButton, filesDone);
                    Registry.CurrentUser.DeleteSubKeyTree("Software\\BackItUp\\ZipCommand");
                }
            }
            catch (Exception e)
            {
            }
        }

        private void progressCancelButton_Click(object sender, RoutedEventArgs e)
        {
            progressStatus.Text = "Exiting";
            manuallyClose = true;
            Trace.WriteLine(progressCancelButton.Content.ToString());
            if (progressCancelButton.Content.ToString().Equals("Cancel"))
            {
                new Thread(() =>
                {
                    zipper.CancelZip();
                    this.Dispatcher.Invoke(() => this.Hide());
                    Thread.Sleep(3000);
                    this.Dispatcher.Invoke(() => Environment.Exit(Environment.ExitCode));
                }).Start();
            }
            else
            {
                Environment.Exit(Environment.ExitCode);
            }
        }


    }
}
