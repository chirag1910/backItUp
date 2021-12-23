using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using Newtonsoft.Json;
using Notifications.Wpf;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using System.Text;

namespace BackItUp
{
    class Zipper
    {

        private List<String> fileList = new List<String>();
        private Int64 allFilesSize = 0;
        private bool cancelFlag = false;
        private string zipPath = "";
        private String prevZipPath = "";
        private bool cancelButtonClicked = false;
        private int compressionLevel;
        private Dictionary<string, string> pathAndRelativePath = new Dictionary<string, string>();

        public void Zip(string[] paths, string[] ignores, string outputFilePath, int compressionLevel, ProgressBar progressBar, TextBlock progressStatus, TextBlock progressValue, TextBlock fileNameInProgress, Button progressCancelButton, TextBlock filesDone)
        {
            var listener = new ProgressListener(fileList, progressBar, progressStatus, progressValue, fileNameInProgress, progressCancelButton, filesDone);
            listener.StartedCalculatingFiles();
            createTree(paths, ignores);

            zipPath = outputFilePath.Substring(0, outputFilePath.LastIndexOf("\\")+1) + ".~" + outputFilePath.Substring(outputFilePath.LastIndexOf("\\") + 1);
            prevZipPath = outputFilePath;
            this.compressionLevel = compressionLevel;
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }
            var newZipFile = File.Create(zipPath);
            File.SetAttributes(zipPath, File.GetAttributes(zipPath) | FileAttributes.Hidden);
            createZip(newZipFile, listener);
        }

        public void CancelZip()
        {
            cancelFlag = true;
            while (cancelFlag)
            {
                Thread.Sleep(100);
            }
        }

        private Boolean isInIgnore(string path, string[] ignores)
        {
            for(var i = 0; i < ignores.Length; i++)
            {
                FileAttributes type = File.GetAttributes(path);

                if (path.Contains(ignores[i].Trim()) && type.HasFlag(FileAttributes.Directory))
                {
                    return true;
                }
            }
            return false;
        }

        private Boolean isInArray(string value, string[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                if (value == array[i].Trim())
                {
                    return true;
                }
            }
            return false;
        }

        private void createTree(string[] paths, string[] ignores, string s = "")
        {
            foreach(String path in paths)
            {

                if (path != null && !isInIgnore(path, ignores))
                {
                    FileAttributes type = File.GetAttributes(path);

                    try
                    {

                        if (type.HasFlag(FileAttributes.Directory) || Path.GetPathRoot(path) == path)
                        {
                            var files = Directory.GetFiles(path);
                            var dirs = Directory.GetDirectories(path);

                            if (dirs.Length == 0 && files.Length == 0)
                            {
                                if (s != "")
                                {
                                    fileList.Add(path);
                                    pathAndRelativePath.Add(path, s);
                                }
                                else
                                {
                                    fileList.Add(path);
                                    pathAndRelativePath.Add(path, path.Substring(0, path.LastIndexOf("\\")));
                                }
                            }
                            else
                            {
                                if (s == "") { 
                                createTree(dirs, ignores, path.Substring(0, path.LastIndexOf("\\")));
                                createTree(files, ignores, path.Substring(0, path.LastIndexOf("\\")));
                                }
                                else
                                {
                                createTree(dirs, ignores, s);
                                createTree(files, ignores, s);

                                }
                            }
                        }
                        else
                        {
                            if (!isInArray(Path.GetExtension(path), ignores))
                            {
                                if (s != "")
                                {
                                    fileList.Add(path);
                                    pathAndRelativePath.Add(path, s);
                                    allFilesSize += new FileInfo(path).Length;
                                }
                                else
                                {
                                    fileList.Add(path);
                                    pathAndRelativePath.Add(path, path.Substring(0, path.LastIndexOf("\\")));
                                    allFilesSize += new FileInfo(path).Length;

                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
        }


        private void createZip(Stream stream, ProgressListener listener)
        {
            long total = 0;
            
            byte[] buffer = new byte[81920];
            ZipOutputStream zos = null;

            listener.Started();
            try
            {
                zos = new ZipOutputStream(stream);
                zos.SetLevel(compressionLevel);
                FileStream input = null;
                foreach (string file in fileList)
                {
                    listener.changeFileName(file);
                    FileAttributes attr = File.GetAttributes(file);
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        string givenPath = "";
                        pathAndRelativePath.TryGetValue(file, out givenPath);
                        var folderName = file.Replace(givenPath, "");
                        if (!folderName.EndsWith("\\"))
                            folderName += "\\";
                        ZipEntry ze = new ZipEntry(folderName);
                        ze.Size = 0;
                        zos.PutNextEntry(ze);
                    }
                    else
                    {
                        string givenPath = "";
                        pathAndRelativePath.TryGetValue(file, out givenPath);
                        ZipEntry ze = new ZipEntry(file.Replace(givenPath, ""));

                        ze.Size = new FileInfo(file).Length;
                        zos.PutNextEntry(ze);
                        try
                        {
                            input = File.OpenRead(file);

                            int len;
                            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                if (!cancelFlag)
                                {
                                    zos.Write(buffer, 0, len);
                                    total += len;
                                    listener.progressUpdate(total, allFilesSize);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if (cancelFlag)
                            {
                                break;
                            }
                        }
                        finally
                        {
                            input.Close();
                        }
                    }
                }
                if (fileList.Count > 0)
                    zos.CloseEntry();

                
            }
            catch (Exception e)
            {
                listener.Failed(e);
                try
                {
                    zos.Close();

                }
                catch
                {

                }
                try
                {
                    stream.Close();

                }
                catch
                {

                }

                if (File.Exists(zipPath)) { File.Delete(zipPath); }
                cancelFlag = true;

            }
            finally
            {
                try
                {
                    zos.Close();
                    if (fileList.Count == 0)
                    {
                        stream.Close();
                    }
                }
                catch (Exception)
                {

                }
            }
            if (cancelFlag)
            {
                stream.Close();
                if (File.Exists(zipPath)) { File.Delete(zipPath); }
                cancelFlag = false;
                cancelButtonClicked = true;
            }

            try
            {
                if (!cancelButtonClicked)
                {
                    if (File.Exists(prevZipPath)) { File.Delete(prevZipPath); }
                    File.Move(zipPath, prevZipPath);
                    File.SetAttributes(prevZipPath, FileAttributes.Normal);
                    listener.progressComplete(total, allFilesSize);
                }
                else
                {
                    if (File.Exists(zipPath)) { File.Delete(zipPath); }
                }
            }
            catch (Exception e)
            {
            }

        }

        public class ProgressListener
        {
            private ProgressBar progressBar;
            private TextBlock progressStatus;
            private TextBlock progressValue;
            private TextBlock fileNameInProgress;
            private Button progressCancelButton;
            private TextBlock filesDone;
            private Stopwatch stopwatch;
            private Int64 totalFiles = 0;
            private List<string> allFiles;
            private NotificationManager notificationManager;


            public ProgressListener(List<string> fileList, ProgressBar progressBar, TextBlock progressStatus, TextBlock progressValue, TextBlock fileNameInProgress, Button progressCancelButton, TextBlock filesDone)
            {
                this.progressBar = progressBar;
                this.progressStatus = progressStatus;
                this.progressValue = progressValue;
                this.fileNameInProgress = fileNameInProgress;
                this.progressCancelButton = progressCancelButton;
                this.filesDone = filesDone;
                this.allFiles = fileList;
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
                    progressCancelButton.Content = "Done";
                });


                notificationManager.Show(new NotificationContent
                {
                    Title = Assembly.GetExecutingAssembly().GetName().Name,
                    Message = "Backup Failed",
                    Type = NotificationType.Error
                });
            }

            internal void progressComplete(long total, Int64 totalSize)
            {

                progressBar.Dispatcher.Invoke(() =>
                {
                progressBar.Value = 100;
                progressBar.Foreground = new SolidColorBrush(Color.FromArgb(255, 92, 184, 92));
                    progressStatus.Text = "Completed";
                    progressValue.Text = "100%";
                    progressCancelButton.Content = "Done";
                });


                stopwatch.Stop();

                notificationManager.Show(new NotificationContent
                {
                    Title = Assembly.GetExecutingAssembly().GetName().Name,
                    Message = "Backup Completed",
                    Type = NotificationType.Success
                });

                // adding to backup logs

                // setting up new 
                BackupLogResult backupLogResult = new BackupLogResult();

                backupLogResult.date = DateTime.Now.ToString();
                backupLogResult.filesBackedUp = totalFiles.ToString();
                backupLogResult.timeTaken = stopwatch.Elapsed.Minutes.ToString() + " : " + (stopwatch.Elapsed.Seconds % 60).ToString();


                // reading prev data
                String dir_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BackItUp\\locals";
                String log_file_path = dir_path + "\\logsPreferences.json";
                String dataString = File.ReadAllText(log_file_path);
                LogsPreferences logsPreferencesFileData = JsonConvert.DeserializeObject<LogsPreferences>(dataString);

                // adding new data
                LogsPreferences logsPreferences = new LogsPreferences() { 
                    logs = new BackupLogResult[logsPreferencesFileData.logs.Length + 1]
                };

                logsPreferences.logs[0] = backupLogResult;
                for(int i = 0; i < logsPreferencesFileData.logs.Length; i++)
                {
                    logsPreferences.logs[i + 1] = logsPreferencesFileData.logs[i];
                }

                // saving data in file
                String dataStringToWrite = JsonConvert.SerializeObject(logsPreferences);
                File.WriteAllText(log_file_path, dataStringToWrite);

                
                var key = Registry.CurrentUser.CreateSubKey("Software\\BackItUp");
                key.SetValue("NumberOfBackups", backupLogResult.date);



            }

            internal void progressUpdate(long total, Int64 totalSize)
            {
                progressBar.Dispatcher.Invoke(() =>
                {
                    progressBar.Value = (total * 100) / totalSize;
                    progressValue.Text = progressBar.Value.ToString() + "%";
                });
            }
            internal void changeFileName(string path)
            {
                totalFiles += 1;
                fileNameInProgress.Dispatcher.Invoke(() =>
                {
                    fileNameInProgress.Text = path.Substring(path.LastIndexOf("\\")+1);
                    filesDone.Text = totalFiles.ToString() + " / " + allFiles.Count.ToString();
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
                    Message = "Backup Started",
                    Type = NotificationType.Information
                });

                stopwatch = Stopwatch.StartNew();
            }
            internal void StartedCalculatingFiles()
            {
                progressBar.Dispatcher.Invoke(() =>
                {
                    progressStatus.Text = "Initializing";
                });

                notificationManager.Show(new NotificationContent
                {
                    Title = Assembly.GetExecutingAssembly().GetName().Name,
                    Message = "Collecting Files",
                    Type = NotificationType.Information
                });
            }
        }
    }
}
