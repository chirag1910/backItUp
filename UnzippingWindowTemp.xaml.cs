using System;
using System.Threading;
using System.Windows.Forms;

namespace BackItUp
{
    /// <summary>
    /// Interaction logic for UnzippingWindowTemp.xaml
    /// </summary>
    public partial class UnzippingWindowTemp
    {

        private Unzipper unzipper;
        private Thread unzipThread;
        private bool manuallyClose = false;

        public UnzippingWindowTemp(String zipPath, String destPath)
        {
            InitializeComponent();

            this.Closing += (s, e) =>
            {
                if (unzipThread.IsAlive)
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
                }
            };

            unzipThread = new Thread(() => startUnzipping(zipPath, destPath));
            unzipThread.Start();
        }

        private void startUnzipping(String zipPath, String destPath)
        {
            try
            {
                unzipper = new Unzipper();
                unzipper.unzip(zipPath, destPath, unzipProgressBar, progressStatus, progressValue, zipEntryInProgress);
            }
            catch (Exception e)
            {
            }
        }
    }
}
