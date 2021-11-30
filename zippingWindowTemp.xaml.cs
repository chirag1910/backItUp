using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace BackItUp
{
    /// <summary>
    /// Interaction logic for zippingWindowTemp.xaml
    /// </summary>
    public partial class zippingWindowTemp
    {
        private string zipPath = "";
        private Zipper zipper;
        private Thread zipThread;
        private bool manuallyClose = false;

        public zippingWindowTemp(String[] fileList, String zipPath)
        {

            InitializeComponent();
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
                    //ProcessThreadCollection toastWindowProcess = Process.GetCurrentProcess().;
                    //File.WriteAllText(@"C:\Users\chira\Desktop\logs.txt", toastWindowProcess.Count.ToString());
                }
            };

            zipThread = new Thread(() => startZipping(fileList, zipPath));
            zipThread.Start();
        }
        private void startZipping(String[] fileList, String zipPath)
        {
            try
            {
                String dir_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BackItUp\\locals";
                String settings_file_path = dir_path + "\\settingsPreferences.json";
                string dataString = File.ReadAllText(settings_file_path);
                SettingsPreferences settingsData = JsonConvert.DeserializeObject<SettingsPreferences>(dataString);

                zipper = new Zipper();
                zipper.Zip(fileList, new string[0], zipPath, settingsData.compressionLevel, zipProgressBar, progressStatus, progressValue, fileNameInProgress, progressCancelButton, filesDone);
            }
            catch (Exception e)
            {
            }
        }

        private void progressCancelButton_Click(object sender, RoutedEventArgs e)
        {
            progressStatus.Text = "Exiting";
            manuallyClose = true;

            if (progressCancelButton.Content.ToString().Equals("Cancel"))
            {
                new Thread(() =>
                {
                    zipper.CancelZip();
                    this.Dispatcher.Invoke(() => this.Hide());
                    Thread.Sleep(3000);
                    this.Dispatcher.Invoke(() => this.Close());
                }).Start();
            }
            else
            {
                this.Close();
            }
        }
    }
}
