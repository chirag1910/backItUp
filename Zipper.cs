using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using K4os.Compression.LZ4.Streams;
using Microsoft.Win32;
using Newtonsoft.Json;
using Notifications.Wpf;
using SharpCompress.Common;
using SharpCompress.Writers;
using SharpCompress.Writers.Tar;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Windows.Storage.Streams;
using Zstandard.Net;
using ZstdSharp;

namespace BackItUp
{
    class Zipper
    {
        private FilesHolder filesHolder;
        private List<String> fileList = new List<String>();
        private Int64 allFilesSize = 0;
        private bool cancelFlag = false;
        private string zipPath = "";
        private String prevZipPath = "";
        private bool cancelButtonClicked = false;
        private int compressionLevel;
        private Dictionary<string, string> pathAndRelativePath = new Dictionary<string, string>();
        private bool caching = false;
        private Dictionary<string, DateTime> modifiedTimes = new Dictionary<string, DateTime>();
        private List<string> directoryPaths = new List<string>();
        private bool useTar;


        public void Zip(string[] paths, string[] ignores, bool useTar, string outputFilePath, int compressionLevel, bool caching, int cachingThreads, long cacheSizeGb, bool standAlone, ProgressBar progressBar, TextBlock progressStatus, TextBlock progressValue, TextBlock fileNameInProgress, Button progressCancelButton, TextBlock filesDone)
        {
            this.useTar = useTar;
            filesHolder = new FilesHolder(cachingThreads, cacheSizeGb);
            this.caching = caching;
            var listener = new ProgressListener(fileList, standAlone, progressBar, progressStatus, progressValue, fileNameInProgress, progressCancelButton, filesDone);
            listener.StartedCalculatingFiles();
            createTree(paths, ignores);
            if (caching)
                filesHolder.startReading();
            zipPath = outputFilePath.Substring(0, outputFilePath.LastIndexOf("\\")+1) + ".~" + outputFilePath.Substring(outputFilePath.LastIndexOf("\\") + 1);
            if (useTar)
            {
                zipPath = zipPath.Replace(".zip", ".tar.zst");
            }
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
                                    modifiedTimes.Add(path, Directory.GetLastWriteTime(path));
                                    directoryPaths.Add(path);
                                    pathAndRelativePath.Add(path, s);
                                }
                                else
                                {
                                    fileList.Add(path);
                                    modifiedTimes.Add(path, Directory.GetLastWriteTime(path));
                                    directoryPaths.Add(path);
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
                                    long fileLen = new FileInfo(path).Length;
                                    modifiedTimes.Add(path, File.GetLastWriteTime(path));
                                    allFilesSize += fileLen;
                                    if (fileLen < 20 * 1024 * 1024 && caching)                          //cache files with size less than 10mb
                                        filesHolder.addToHolder(path, fileLen);
                                    
                                }
                                else
                                {
                                    fileList.Add(path);
                                    pathAndRelativePath.Add(path, path.Substring(0, path.LastIndexOf("\\")));
                                    long fileLen = new FileInfo(path).Length;
                                    modifiedTimes.Add(path, File.GetLastWriteTime(path));
                                    allFilesSize += fileLen;
                                    if (fileLen < 20 * 1024 * 1024 && caching)
                                        filesHolder.addToHolder(path, fileLen);

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
            if (useTar)
            {
                createTar(stream, listener);
                return;
            }
            long total = 0;
            
            byte[] buffer = new byte[1 * 1024 * 1024];
            ZipArchive zos = null;
            listener.Started();
            try
            {
                FileStream input = null;


                zos = new ZipArchive(stream, ZipArchiveMode.Create);
                CompressionLevel compression = compressionLevel == 0 ? CompressionLevel.NoCompression : compressionLevel == 1 ? CompressionLevel.Fastest : compressionLevel == -1 ? CompressionLevel.Optimal : CompressionLevel.Optimal;
                foreach (string file in fileList)
                {
                    if (cancelFlag)
                    {
                        break;
                    }
                    listener.changeFileName(file);
                    if (filesHolder.isInHolder(file))
                    {
                        string givenPath = "";
                        pathAndRelativePath.TryGetValue(file, out givenPath);
                        var entry = zos.CreateEntry(file.Replace(givenPath, "").TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace('\\', '/'), compression);
                        DateTime modified;
                        modifiedTimes.TryGetValue(file, out modified);
                        entry.LastWriteTime = modified;
                        var str = entry.Open();
                        str.Write(filesHolder.getFileData(file));
                        str.Close();


                        total += filesHolder.getFileLength(file);

                        listener.progressUpdate(total, allFilesSize);
                        if (cancelFlag)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (directoryPaths.Contains(file))
                        {
                            string givenPath = "";
                            pathAndRelativePath.TryGetValue(file, out givenPath);
                            var folderName = file.Replace(givenPath, "");
                            if (!folderName.EndsWith("\\"))
                                folderName += "\\";
                            folderName = folderName.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace('\\', '/');


                            zos.CreateEntry(folderName);


                        }
                        else
                        {
                            string givenPath = "";
                            pathAndRelativePath.TryGetValue(file, out givenPath);
                            try
                            {

                                var entry = zos.CreateEntry(file.Replace(givenPath, "").TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace('\\', '/'), compression);
                                
                                DateTime modified;
                                modifiedTimes.TryGetValue(file, out modified);
                                entry.LastWriteTime = modified;
                                var str = entry.Open();
                                input = File.OpenRead(file);

                                int len;
                                while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    if (!cancelFlag)
                                    {
                                        str.Write(buffer, 0, len);
                                        total += len;
                                        listener.progressUpdate(total, allFilesSize);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                str.Close();
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
                }

                try { zos.Dispose(); } catch { }
            }
            catch (Exception e)
            {
                listener.Failed(e);
                try { zos.Dispose(); } catch { }
                if (File.Exists(zipPath)) { File.Delete(zipPath); }
                cancelFlag = true;

            }
            filesHolder.cancelAndClearAll();
            if (cancelFlag)
            {
                if (File.Exists(zipPath)) { File.Delete(zipPath); }
                cancelFlag = false;
                cancelButtonClicked = true;
            }

            try
            {
                if (!cancelButtonClicked)
                {
                    prevZipPath = useTar ? prevZipPath.Replace(".zip", ".tar.zst") : prevZipPath;
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




        private void createTar(Stream stream, ProgressListener listener)
        {
            long total = 0;
            using (var buffered = new BufferedStream(stream, 1 * 1024 * 1024))
            using (var zstdStream = new ZstandardStream(buffered, CompressionMode.Compress))
            using (var countingStream = new CountingStream(zstdStream))
            using (var tarWriter = WriterFactory.Open(countingStream, ArchiveType.Tar, CompressionType.None))
            {
                byte[] buffer = new byte[1 * 1024 * 1024];
                listener.Started();
                try
                {
                    FileStream input = null;


                    CompressionLevel compression = compressionLevel == 0 ? CompressionLevel.NoCompression : compressionLevel == 1 ? CompressionLevel.Fastest : compressionLevel == -1 ? CompressionLevel.Optimal : CompressionLevel.Optimal;
                    foreach (string file in fileList)
                    {
                        if (cancelFlag)
                        {
                            break;
                        }
                        listener.changeFileName(file);
                        if (filesHolder.isInHolder(file))
                        {
                            string givenPath = "";
                            pathAndRelativePath.TryGetValue(file, out givenPath);



                            DateTime modified;
                            modifiedTimes.TryGetValue(file, out modified);

                            using (var memStream = new MemoryStream(filesHolder.getFileData(file)))
                            {
                                tarWriter.Write(file.Replace(givenPath, "").TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace('\\', '/'), memStream, modified);
                            }

                            total += filesHolder.getFileLength(file);

                            listener.progressUpdate(total, allFilesSize);
                            if (cancelFlag)
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (directoryPaths.Contains(file))
                            {
                                string givenPath = "";
                                pathAndRelativePath.TryGetValue(file, out givenPath);
                                var folderName = file.Replace(givenPath, "");
                                if (!folderName.EndsWith("\\"))
                                    folderName += "\\";
                                folderName = folderName.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace('\\', '/');

                                DateTime modified;
                                modifiedTimes.TryGetValue(file, out modified);

                                tarWriter.Write(folderName, new MemoryStream(Array.Empty<byte>()), modified);


                            }
                            else
                            {
                                string givenPath = "";
                                pathAndRelativePath.TryGetValue(file, out givenPath);
                                try
                                {

                                    //var entry = zos.CreateEntry(file.Replace(givenPath, "").TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace('\\', '/'), compression);

                                    DateTime modified;
                                    modifiedTimes.TryGetValue(file, out modified);
                                    input = File.OpenRead(file);

                                    long bytesWrittenToTar = countingStream.BytesWritten;
                                    long fileLen = new FileInfo(file).Length;
                                    long totalOriginal = total;
                                    bool isWriting = true;
                                    Thread tarProgUpdater = new Thread(() =>
                                    {

                                        while (isWriting && countingStream.BytesWritten - bytesWrittenToTar < fileLen)
                                        {
                                            total = totalOriginal + countingStream.BytesWritten - bytesWrittenToTar;
                                            listener.progressUpdate(total, allFilesSize);
                                            Thread.Sleep(10);
                                        }
                                    });
                                    tarProgUpdater.Start();
                                    tarWriter.Write(file.Replace(givenPath, "").TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace('\\', '/'), input, modified);
                                    input.Close();
                                    isWriting = false;
                                    try { tarProgUpdater.Abort(); } catch { }

                                    total = totalOriginal + fileLen;
                                    listener.progressUpdate(total, allFilesSize);
                                    //while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
                                    //{
                                    //    if (!cancelFlag)
                                    //    {
                                    //        str.Write(buffer, 0, len);
                                    //        total += len;
                                    //        listener.progressUpdate(total, allFilesSize);
                                    //    }
                                    //    else
                                    //    {
                                    //        break;
                                    //    }
                                    //}
                                    //str.Close();
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
                    }

                }
                catch (Exception e)
                {
                    listener.Failed(e);
                    cancelFlag = true;
                }
            }
            filesHolder.cancelAndClearAll();
            if (cancelFlag)
            {
                if (File.Exists(zipPath)) { File.Delete(zipPath); }
                cancelFlag = false;
                cancelButtonClicked = true;
            }

            try
            {
                if (!cancelButtonClicked)
                {
                    prevZipPath = useTar ? prevZipPath.Replace(".zip", ".tar.zst") : prevZipPath;
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




        class CountingStream : Stream
        {
            private readonly Stream _baseStream;
            public long BytesWritten { get; private set; }

            public CountingStream(Stream baseStream)
            {
                _baseStream = baseStream;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _baseStream.Write(buffer, offset, count);
                BytesWritten += count;
            }

            // Override other required abstract members to delegate to _baseStream

            public override bool CanRead => _baseStream.CanRead;
            public override bool CanSeek => _baseStream.CanSeek;
            public override bool CanWrite => _baseStream.CanWrite;
            public override long Length => _baseStream.Length;
            public override long Position { get => _baseStream.Position; set => _baseStream.Position = value; }
            public override void Flush() => _baseStream.Flush();
            public override int Read(byte[] buffer, int offset, int count) => _baseStream.Read(buffer, offset, count);
            public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);
            public override void SetLength(long value) => _baseStream.SetLength(value);
        }


        public class FilesHolder
        {
            private ConcurrentDictionary<int, string> filePaths;
            private ConcurrentBag<int> isLoaded;
            private ConcurrentDictionary<int, long> fileSizes;
            private ConcurrentDictionary<int, byte[]> data;
            private List<Thread> t;
            private int currWriting = 0;
            private int threadsCount;
            private int fileCount = 0;
            private List<string> filePathsIndexes;
            private long avgFileSize = 0;
            private long totalSize = 0;
            private long maxCache;
            private Thread gcThread;
            public FilesHolder(int cachingThreads, long cacheSizeGb)
            {
                filePaths = new ConcurrentDictionary<int,string>();
                isLoaded = new ConcurrentBag<int>();
                data = new ConcurrentDictionary<int, byte[]>();
                fileSizes = new ConcurrentDictionary<int, long>();
                t = new List<Thread>();
                filePathsIndexes = new List<string>();
                threadsCount = cachingThreads;
                maxCache = cacheSizeGb * 1024l * 1024l * 1024l;
                gcThread = new Thread(() =>
                {
                    while (true)
                    {

                        GC.Collect();
                        Thread.Sleep(500);
                    }
                });
                gcThread.Start();
            }
            private int getIndex(string path)
            {
                try
                {
                    
                    return filePaths[filePathsIndexes.IndexOf(path)].Equals(path)?filePathsIndexes.IndexOf(path) : -1;
                }
                catch { return -1; }
            }
            public void cancelAndClearAll()
            {

                foreach (Thread tr in t)
                {
                    try { tr.Abort(); } catch { }
                }
                try { gcThread.Abort(); GC.Collect(); } catch { }

                t.Clear();
                data.Clear();
                filePaths.Clear();
                isLoaded.Clear();
                fileSizes.Clear();

            }
            public void addToHolder(string filePath, long fileLength)
            {
                FileAttributes attr = File.GetAttributes(filePath);
                if ((attr & FileAttributes.Directory) != FileAttributes.Directory)
                {
                    filePaths.TryAdd(fileCount, filePath);
                    fileSizes.TryAdd(fileCount, fileLength);
                    data.TryAdd(fileCount, new byte[0]);
                    filePathsIndexes.Add(filePath);
                    fileCount++;
                    totalSize += fileLength;
                    avgFileSize = totalSize / fileCount;
                }
            }
            public bool isInHolder(string filePath)
            {
                return getIndex(filePath) != -1 ;
            }
            public void startReading ()
            {
                long numberOfFileToCache = maxCache / avgFileSize;

                for (int i = 0; i < threadsCount; i++)
                {
                    int j = i;
                    t.Add(new Thread(() =>
                    {
                        int n = 0;
                        foreach (var fp in filePaths)

                        {
                            try
                            {
                                int ind = fp.Key;
                                while (ind - currWriting > numberOfFileToCache) { Thread.Sleep(1); }
                                if ((j + (threadsCount * n)) == ind)
                                {
                                    FileStream f = File.OpenRead(fp.Value);
                                long fileLen = getFileLength(fp.Value);
                                data[ind] = new byte[fileLen];
                                    //for (long i = 0; i < fileLen; i++)
                                    //{
                                    //    data[ind][i] = (byte)f.ReadByte();
                                    //}
                                    int read = f.Read(data[ind], 0, (int)fileLen);
                                    f.Close();
                                    if (read == fileLen)
                                    {
                                        isLoaded.Add(ind);
                                        n++;
                                    }
                                }
                            }
                            catch
                            {

                            }
                        }
                    }));
                t[j].Start();
                }

            }

            public byte[] getFileData(string filePath)
            {
                int ind = getIndex(filePath);
                while (!isLoaded.Contains(ind)) { Thread.Sleep(1); }
                try { 
                    currWriting = ind;
                if (ind > 0)
                {
                        Array.Clear(data[ind - 1], 0, data[ind-1].Length);
                        data[ind - 1] = new byte[0];
                }
                return data[ind];
            }
                catch { return new byte[0]; }
            }

            public long getFileLength(string filePath)
            {
                return fileSizes[getIndex(filePath)];
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
            private bool standAlone = false;
            private Thread UpdaterThread;
            private long total;
            private Int64 totalSize = 0;
            private string currentPath = "";


            public ProgressListener(List<string> fileList, bool standAlone, ProgressBar progressBar, TextBlock progressStatus, TextBlock progressValue, TextBlock fileNameInProgress, Button progressCancelButton, TextBlock filesDone)
            {
                this.progressBar = progressBar;
                this.progressStatus = progressStatus;
                this.progressValue = progressValue;
                this.fileNameInProgress = fileNameInProgress;
                this.progressCancelButton = progressCancelButton;
                this.filesDone = filesDone;
                this.allFiles = fileList;
                this.standAlone = standAlone;
                notificationManager = new NotificationManager();
                stopwatch = Stopwatch.StartNew();
                UpdaterThread = new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            progressBar.Dispatcher.Invoke(new Action(() =>
                            {
                                progressBar.Value = (((total * 100) / totalSize) + ((totalFiles * 100) / allFiles.Count))/2 ;
                                progressValue.Text = progressBar.Value.ToString() + "%";
                            }));

                            fileNameInProgress.Dispatcher.Invoke(new Action(() =>
                            {
                                fileNameInProgress.Text = currentPath;
                                filesDone.Text = totalFiles.ToString() + " / " + allFiles.Count.ToString();

                            }));

                            Thread.Sleep(10);
                        }
                        catch { }

                    }
                });
            }

            internal void Failed(Exception e)
            {
                try
                {
                    UpdaterThread.Abort();
                    while (UpdaterThread.IsAlive) Thread.Sleep(10);
                }
                catch { }
                progressBar.Dispatcher.Invoke(() =>
                {
                progressBar.Foreground = new SolidColorBrush(Color.FromArgb(255, 217, 83, 79));
                    progressBar.Value = 100;
                    progressStatus.Text = "Failed: " + e.ToString();
                    progressValue.Text = "100%";
                    progressCancelButton.Content = "Done";
                });


                notificationManager.Show(new NotificationContent
                {
                    Title = Assembly.GetExecutingAssembly().GetName().Name,
                    Message = !standAlone ? "Backup Failed" : "Zipping Failed",
                    Type = NotificationType.Error
                });
            }

            internal void progressComplete(long total, Int64 totalSize)
            {

                try
                {
                    UpdaterThread.Abort();
                    while (UpdaterThread.IsAlive) Thread.Sleep(10);
                }
                catch { }
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
                    Message = !standAlone ? "Backup Completed" : "Zipping Completed",
                    Type = NotificationType.Success
                });

                // adding to backup logs

                if (!standAlone)
                {
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
                    LogsPreferences logsPreferences = new LogsPreferences()
                    {
                        logs = new BackupLogResult[logsPreferencesFileData.logs.Length + 1]
                    };

                    logsPreferences.logs[0] = backupLogResult;
                    for (int i = 0; i < logsPreferencesFileData.logs.Length; i++)
                    {
                        logsPreferences.logs[i + 1] = logsPreferencesFileData.logs[i];
                    }

                    // saving data in file
                    String dataStringToWrite = JsonConvert.SerializeObject(logsPreferences);
                    File.WriteAllText(log_file_path, dataStringToWrite);


                    var key = Registry.CurrentUser.CreateSubKey("Software\\BackItUp");
                    key.SetValue("NumberOfBackups", backupLogResult.date);
                }
                
            }

            internal void progressUpdate(long total, Int64 totalSize)
            {
                this.total = total;
                this.totalSize = totalSize;
            }
            internal void changeFileName(string path)
            {
                totalFiles += 1;
                currentPath = path;
                
                //fileNameInProgress.Dispatcher.Invoke(() =>
                //{
                //    //fileNameInProgress.Text = path.Substring(path.LastIndexOf("\\")+1);
                //    fileNameInProgress.Text = path;
                //    filesDone.Text = totalFiles.ToString() + " / " + allFiles.Count.ToString();
                //});
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
                    Message = !standAlone ? "Backup Started" : "Zipping Started",
                    Type = NotificationType.Information
                });


                UpdaterThread.Start();
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
