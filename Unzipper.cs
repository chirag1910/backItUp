using ICSharpCode.SharpZipLib.Zip;
using Notifications.Wpf;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;


namespace BackItUp
{
    class Unzipper
    {
        public void unzip(String zipPath, String destinationPath, ProgressBar progressBar, TextBlock progressStatus, TextBlock progressValue, TextBlock fileNameInProgress)
        {
            FileInfo fi = new FileInfo(zipPath);
            Int64 zipFileSize = fi.Length;
            Int64 sizeUnzipped = 0;

            var listener = new ProgressListener(progressBar, progressStatus, progressValue, fileNameInProgress);
            listener.initiated();

            ZipInputStream zipIn = null;
            FileStream stream = null;

            try
            {
                destinationPath = Path.Join(destinationPath, fi.Name.Substring(0, fi.Name.LastIndexOf(fi.Extension)));
                Directory.CreateDirectory(Path.Join(destinationPath));

                listener.Started();

                zipIn = new ZipInputStream(File.OpenRead(zipPath));
                ZipEntry entry;

                while ((entry = zipIn.GetNextEntry()) != null)
                {
                    listener.changeEntryName(entry.Name);
                    String dirPath = Path.GetDirectoryName(Path.Join(destinationPath, entry.Name));

                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }
                    if (!entry.IsDirectory)
                    {
                        stream = File.Create(Path.Join(destinationPath, entry.Name));

                        int size = 81920;
                        byte[] buffer = new byte[size];

                        while ((size = zipIn.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            stream.Write(buffer, 0, size);
                            sizeUnzipped += size;

                            listener.progressUpdate(sizeUnzipped, zipFileSize);
                        }
                    }

                    stream.Close();
                }
                listener.progressComplete();
            }
            catch (Exception ex)
            {
                listener.Failed(ex);
            }
            finally
            {
                if (zipIn != null)
                {
                    zipIn.Close();
                }

                if (stream != null)
                {
                    stream.Close();
                }
            }

        }

        public class ProgressListener
        {
            private ProgressBar progressBar;
            private TextBlock progressStatus;
            private TextBlock progressValue;
            private TextBlock fileNameInProgress;

            private NotificationManager notificationManager;

            public ProgressListener(ProgressBar progressBar, TextBlock progressStatus, TextBlock progressValue, TextBlock fileNameInProgress)
            {
                this.progressBar = progressBar;
                this.progressStatus = progressStatus;
                this.progressValue = progressValue;
                this.fileNameInProgress = fileNameInProgress;

                notificationManager = new NotificationManager();
            }

            internal void Failed(Exception e)
            {
                progressBar.Dispatcher.Invoke(() =>
                {
                    progressBar.Foreground = new SolidColorBrush(Color.FromArgb(255, 217, 83, 79));
                    progressBar.Value = 100;
                    progressStatus.Text = "Failed";
                    progressValue.Text = "100%";
                });

                notificationManager.Show(new NotificationContent
                {
                    Title = Assembly.GetExecutingAssembly().GetName().Name,
                    Message = "Un-zipping Failed",
                    Type = NotificationType.Error
                });
            }

            internal void progressComplete()
            {

                progressBar.Dispatcher.Invoke(() =>
                {
                    progressBar.Value = 100;
                    progressBar.Foreground = new SolidColorBrush(Color.FromArgb(255, 92, 184, 92));
                    progressStatus.Text = "Completed";
                    progressValue.Text = "100%";
                });


                notificationManager.Show(new NotificationContent
                {
                    Title = Assembly.GetExecutingAssembly().GetName().Name,
                    Message = "Un-zipping Completed",
                    Type = NotificationType.Success
                });
            }

            internal void progressUpdate(Int64 filesSizeUnzipped, Int64 totalZipFileSize)
            {
                progressBar.Dispatcher.Invoke(() =>
                {
                    progressBar.Value = (filesSizeUnzipped * 100) / totalZipFileSize;
                    progressValue.Text = progressBar.Value.ToString() + "%";
                });
            }

            internal void changeEntryName(string name)
            {
                fileNameInProgress.Dispatcher.Invoke(() =>
                {
                    fileNameInProgress.Text = name.Substring(name.LastIndexOf("/")+1);
                });
            }

            internal void Started()
            {
                progressBar.Dispatcher.Invoke(() =>
                {
                    progressBar.Foreground = new SolidColorBrush(Color.FromArgb(255, 2, 117, 216));
                    progressStatus.Text = "In Progress";
                });

                notificationManager.Show(new NotificationContent
                {
                    Title = Assembly.GetExecutingAssembly().GetName().Name,
                    Message = "Un-zipping Started",
                    Type = NotificationType.Information,
                });
            }
            internal void initiated()
            {
                progressBar.Dispatcher.Invoke(() =>
                {
                    progressStatus.Text = "Initializing";
                });
            }
        }
    }
}
