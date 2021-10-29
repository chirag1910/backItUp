using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Linq;
using System.Threading;

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
        private bool showZippingWindow;
        public zippingWindow(bool showWindow)
        {
            InitializeComponent();
            showZippingWindow = showWindow;
            this.Closing += (s, e) =>
            {
                if (zipThread.IsAlive)
                {

                    e.Cancel = !manuallyClose;
                    this.Hide();


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
                String log_file_path = dir_path + "\\fileListPreferences.json";

                String dataString = File.ReadAllText(log_file_path);

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
                String settings_file_path = dir_path + "\\settingsPreferences.json";

                dataString = File.ReadAllText(settings_file_path);
                data = JObject.Parse(dataString);

                var zipPath = data["saveLocation"].ToString();

                if (!zipPath.EndsWith("\\"))
                {
                    zipPath += "\\";
                }

                zipPath += data["saveAs"].ToString() + ".zip";
                this.zipPath = zipPath;
                zipper = new Zipper();
                zipper.Zip(stringResults, zipPath, zipProgressBar, progressStatus, progressValue, fileNameInProgress, progressCancelButton, filesDone);
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
            catch (Exception e)
            {
            }
        }

        private void progressCancelButton_Click(object sender, RoutedEventArgs e)
        {
            progressStatus.Text = "Exiting";
            manuallyClose = true;

            if (zipThread.IsAlive)
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
