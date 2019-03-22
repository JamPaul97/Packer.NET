using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;


namespace PackerNET
{
    public class Packer
    {
        private Version version = new Version(0, 1, 1, 1);
        /// <summary>
        /// Progress Event. Will trigger when there is change on the Backup/Restore thread
        /// </summary>
        /// <param name="Current">The current task value</param>
        /// <param name="Maximum">The maximum numbers tasks</param>
        /// <param name="CurrentFile">The current file being proccesed</param>
        public delegate void ProgressEvent(int Current,int Maximum,string CurrentFile);
        /// <summary>
        /// Progress Event. Will trigger when the running task is finnished
        /// </summary>
        /// <param name="EllapsedMilliseconds">Miliseconds passes from the begging of the task.</param>
        public delegate void ProgressCompleted(long EllapsedMilliseconds);
        public event ProgressEvent ReportRestoreProgress;
        public event ProgressEvent ReportBackupProgress;
        public event ProgressCompleted ReportProgressCompleted;
        #region "Private Functions"
        private void OnProgressCompleted(long EllapsedMilliseconds)
        {
            if (ReportProgressCompleted != null)
                ReportProgressCompleted(EllapsedMilliseconds);
        }
        private void OnReportBackupProgress(int Current, int Max, string File)
        {
            if (ReportBackupProgress != null)
                ReportBackupProgress(Current, Max, File);
        }
        private void OnReportRestoreProgress(int Current, int Max, string File)
        {
            if (ReportRestoreProgress != null)
                ReportRestoreProgress(Current, Max, File);
        }
        private static void BackupFiles(string FilePath, List<FileFormat> files)
        {
            string currentFile = string.Empty;
            try
            {
                Stream stream = File.OpenWrite(FilePath);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, files);
                stream.Flush();
                stream.Close();
                stream.Dispose();
                stream.Close();
            }
            catch (System.Runtime.Serialization.SerializationException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        private void CrateFileFormats(string[] Files, ref List<FileFormat> FileList, string mainFolder, string BackupFile, ref Stopwatch sw)
        {
            for (int i = 0; i < Files.Length; i++)
            {
                FileFormat file = new FileFormat()
                {
                    FilePath = Path.GetDirectoryName(Files[i]).Replace(mainFolder, ""),
                    FileName = Path.GetFileName(Files[i]),
                    Data = File.ReadAllBytes(Files[i])
                };
                FileList.Add(file);
                OnReportBackupProgress(i, Files.Length - 1, Files[i]);
            }
            BackupFiles(BackupFile, FileList);
            sw.Stop();
            OnProgressCompleted(sw.ElapsedMilliseconds);
        }
        private static string[] DirSearch(string sDir, bool isFirst)
        {
            List<string> temp = new List<string>();
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d))
                    {
                        temp.Add(f);
                    }
                    foreach (string x in DirSearch(d, false))
                    {
                        temp.Add(x);
                    }
                }
                if (isFirst)
                {
                    foreach (string x in Directory.GetFiles(sDir))
                    {
                        temp.Add(x);
                    }
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
            return temp.ToArray();
        }
        #endregion


        #region "Public Functions"
        /// <summary>
        /// Restore a backup file to the specified location
        /// </summary>
        /// <param name="BackupFile">Path to the backup file</param>
        /// <param name="NewFolder">Path to the folder you want the items to be unpacked</param>
        public void Restore(string BackupFile, string NewFolder)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (!File.Exists(BackupFile))
            {
                throw new Exception("The files can not be found!" + BackupFile);
            }
            string currentFile = string.Empty;
            List<FileFormat> tempFileList;
            try
            {
                Stream stream = File.OpenRead(BackupFile);
                BinaryFormatter formatter = new BinaryFormatter();
                object obj = formatter.Deserialize(stream);
                tempFileList = (List<FileFormat>)obj;
                stream.Flush();
                stream.Close();
                stream.Dispose();
                stream.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("There was an error while Deserialing the file :" + ex.Message);
            }
            try
            {
                for (int i = 0; i < tempFileList.Count; i++)
                {
                    string fullPath = NewFolder + "\\" + tempFileList[i].FileName;
                    string pathOnly = NewFolder + "\\" + tempFileList[i].FilePath;
                    currentFile = tempFileList[i].FilePath + tempFileList[i].FileName;
                    if (!Directory.Exists(pathOnly))
                        Directory.CreateDirectory(pathOnly);
                    File.WriteAllBytes(fullPath, tempFileList[i].Data);
                    OnReportRestoreProgress(i, tempFileList.Count, currentFile);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " - " + currentFile);
            }
            sw.Stop();
            OnProgressCompleted(sw.ElapsedMilliseconds);
        }
        /// <summary>
        /// Backup a directory to the specified file
        /// </summary>
        /// <param name="Folder">Path for the folder to be backed up</param>
        /// <param name="BackupFile">The backup file</param>
        public void BackUp(string Folder, string BackupFile)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            List<FileFormat> temp = new List<FileFormat>();
            string[] files = DirSearch(Folder, true);
            Thread te = new Thread(() => CrateFileFormats(files, ref temp, Folder, BackupFile, ref sw));
            te.Start();
        }
        /// <summary>
        /// Get the version of PackerNET
        /// </summary>
        /// <returns>Version of PackerNET</returns>
        public Version Version()
        {
            return this.version;
        }
        #endregion



    }
}
