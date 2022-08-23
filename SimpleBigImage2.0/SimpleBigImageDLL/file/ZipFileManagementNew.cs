using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using Ionic.Zip;

namespace SimpleBigImage2
{
    public class ZipFileManagement : IDisposable
    {
        ZipFile _archive;
        string _strFilePath;
        Mutex MUTEX = new Mutex();

        /// <summary>
        /// Manages the zip file, or if even a zipfile is needed
        /// </summary>
        /// <param name="strFilePath">The file to open or create</param>
        /// <param name="blnCreateNew">Delete the old file and create a new one?</param>
        public ZipFileManagement(string strFilePath, bool blnCreateNew = true)
        {
            _strFilePath = strFilePath;
            if (blnCreateNew)
            {
                if (File.Exists(_strFilePath))
                {
                    File.Delete(_strFilePath);
                }
                FileStream filestream = new FileStream(_strFilePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite);
                ZipArchive archive = new ZipArchive(filestream, ZipArchiveMode.Create);
                //archive.CreateEntry("data.xml");
                filestream.Flush();
                archive.Dispose();
                filestream.Dispose();
            }
            init();
        }

        ///////////////////////////////////////////

        /// <summary>
        /// Inits the class/file
        /// </summary>
        private void init()
        {
            bool blnFileExists = File.Exists(_strFilePath);


            _archive = new ZipFile(_strFilePath);
            _archive.UseZip64WhenSaving = Zip64Option.Always;

        }

        /// <summary>
        /// Gets all the subdirectories. Only used
        /// </summary>
        /// <param name="strDirectoryName"></param>
        /// <returns></returns>
        private List<string> getSubDirectories(string strDirectoryName)
        {
            String[] data = Directory.GetDirectories(strDirectoryName);
            List<string> output = new List<string>();

            foreach (string str in data)
            {
                output.Add(str);
            }

            return output;
        }

        /// <summary>
        /// Gets all the files in a directory
        /// </summary>
        /// <param name="strDirectoryName">The directory to get the files from</param>
        /// <returns>A List of the filenames</returns>
        private List<string> getFilesOf(string strDirectoryName)
        {
            String[] data = Directory.GetFiles(strDirectoryName);

            List<string> output = new List<string>();

            foreach (string str in data)
            {
                output.Add(str);
            }

            return output;
        }

        /// <summary>
        /// ZIP function: Adds files and directories to a ZIP file
        /// </summary>
        /// <param name="strDirectoryName">The target directory</param>
        /// <param name="folders">the folders to add</param>
        /// <param name="files">the files to add</param>
        private void add(string strDirectoryName, List<string> folders, List<string> files)
        {
            foreach (string folderName in folders)
            {
                List<string> resFolders = getSubDirectories(folderName);
                List<string> resFiles = getFilesOf(folderName);

                add(strDirectoryName, resFolders, resFiles);
            }

            foreach (string fileName in files)
            {
                int iLength = strDirectoryName.Length;
                string strEntryName = System.IO.Path.GetDirectoryName(fileName);
                if (strEntryName.Length > iLength + 1)
                {
                    strEntryName = strEntryName.Substring(iLength + 1);
                }
                else
                {
                    strEntryName = "";
                }
                strEntryName = strEntryName.Replace('\\', '/');
                _archive.AddFile(fileName, strEntryName);
            }
        }

        /// <summary>
        /// Disposes and frees memory
        /// </summary>
        private void deinit()
        {
            try
            {
                if (_archive != null)
                {
                    _archive.Save();
                    _archive.Dispose();
                    _archive = null;
                }

            }
            finally { }
        }

        ///////////////////////////////////////////

        private void reinit()
        {
            init();
        }
        ///////////////////////////////////////////



        /// <summary>
        /// Just sets a mutex; 
        /// </summary>
        private void getArchive()
        {
            MUTEX.WaitOne();
        }

        /// <summary>
        /// Just sets a mutex;
        /// </summary>
        private void stopArchiveAccess()
        {
            MUTEX.ReleaseMutex();
        }

        /// <summary>
        /// Gives back the MemoryStream of a specific ZIP entry
        /// </summary>
        /// <param name="strInternalName"></param>
        /// <returns></returns>
        private MemoryStream getEntryStream(string strInternalName)
        {
            MemoryStream output = new MemoryStream();
            List<ZipEntry> entries = _archive.Entries.ToList();
            foreach (ZipEntry entry in entries)
            {
                strInternalName = strInternalName.Replace('\\','/');
                if (entry.FileName == strInternalName)
                {
                    entry.Extract(output);
                    return output;
                }
            }
            return null;
        }

        /// <summary>
        /// Sets a Stream for a specific ZIP entry
        /// </summary>
        /// <param name="strInternalName">Sets the name</param>
        /// <param name="data">The data to put in</param>
        private void setEntryStream(string strInternalName, MemoryStream data)
        {
            if (_archive.ContainsEntry(strInternalName))
            {
                _archive.UpdateEntry(strInternalName, data);
            }
            else
            {
                _archive.AddEntry(strInternalName, data);
            }
        }


        ///////////////////////////////////////////

        /*
        public string FILENAME
        {
            get
            {
                return _strFilePath;
            }
        }
        */
        ///////////////////////////////////////////

        /// <summary>
        /// Destructor
        /// </summary>
        ~ZipFileManagement()
        {
            deinit();
        }

        /// <summary>
        /// Deinits/Disposes
        /// </summary>
        public void Dispose()
        {
            deinit();
        }

        ///////////////////////////////////////////

        /// <summary>
        /// Historic function, actually does nothing.
        /// </summary>
        /// <param name="strInternalDirectory"></param>
        public void createFolderIfNotExisting(string strInternalDirectory)
        {
            return;
        }

        /// <summary>
        /// Check if a folder is initialized
        /// </summary>
        /// <param name="strInternalDirectory">The internal directory</param>
        /// <returns>"True" if the folder exists</returns>
        public bool checkFolderExists(string strInternalDirectory)
        {
            return _archive.ContainsEntry(strInternalDirectory);
        }

        /// <summary>
        /// Check if a file exists
        /// </summary>
        /// <param name="strInternalFileName">The filename to search</param>
        /// <returns>"True" if the file exists</returns>
        public bool checkFileExists(string strInternalFileName)
        {
            getArchive();
            bool blnOutput = true;
            blnOutput = _archive.ContainsEntry(strInternalFileName);
            stopArchiveAccess();
            return blnOutput;
        }

        /// <summary>
        /// Loads a file from the Zipfile into the memorystream
        /// </summary>
        /// <param name="strInternalFileName">The filename</param>
        /// <returns>A Memorystream with all the data</returns>
        public MemoryStream loadFromZip(string strInternalFileName)
        {
            MemoryStream output = null;
            getArchive();
            output = getEntryStream(strInternalFileName);
            stopArchiveAccess();

            return output;
        }

        /// <summary>
        /// Saves a Memorystream to the Zipfile
        /// </summary>
        /// <param name="strInternalFileName">Internal filename inside the zip</param>
        /// <param name="stream">The Memorystream to save</param>
        /// <returns>always true</returns>
        public bool saveToZip(string strInternalFileName, MemoryStream stream)
        {
            getArchive();
            setEntryStream(strInternalFileName, stream);
            stopArchiveAccess();

            return true;
        }

        /// <summary>
        /// Compresses a whole folder into a zipfile
        /// </summary>
        /// <param name="strInputFolder">The Folder to compress</param>
        /// <param name="blnDelete">Delete the original</param>
        public void compress(string strInputFolder, bool blnDelete = false)
        {
            _archive.CompressionLevel = Ionic.Zlib.CompressionLevel.None;
            //_archive.CompressionLevel = Ionic.Zlib.CompressionLevel.BestSpeed;
            _archive.UseZip64WhenSaving = Zip64Option.Always;

            List<string> resFolders = getSubDirectories(strInputFolder);
            List<string> resFiles = getFilesOf(strInputFolder);
            add(strInputFolder, resFolders, resFiles);
            _archive.Save();
            _archive.Dispose();

            if (blnDelete)
            {
                try
                {
                    Directory.Delete(strInputFolder, true);
                    Directory.Delete(strInputFolder, true);
                    Directory.Delete(strInputFolder, true);
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }
        }

    }
}
