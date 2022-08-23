using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;

namespace SimpleBigImage2
{
    public class DataSource : IStaticDataSource
    {
        private Stream _stream;

        public DataSource(MemoryStream stream)
        {
            _stream = stream;
            _stream.Position = 0;
        }

        public Stream GetSource()
        {
            return _stream;
        }
    }

    public class OldLargeZipFileManagement : IDisposable
    {
        ZipFile _archive;
        FileStream _stream;
        string _strFilePath;
        Mutex MUTEX = new Mutex();

        public OldLargeZipFileManagement(string strFilePath, bool blnCreateNew = false)
        {
            _strFilePath = strFilePath;
            if (File.Exists(_strFilePath))
            {
                if (blnCreateNew)
                {
                    File.Delete(_strFilePath);
                }
            }
            init();
        }

        ///////////////////////////////////////////

        private void init()
        {
            if (_archive != null)
            {
                _archive.Close();
                _archive = null;
            }
            if (_stream != null)
            {
                _stream.Dispose();
            }

            bool blnFileExists = File.Exists(_strFilePath);

            _stream = new FileStream(_strFilePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite);

            if (!blnFileExists)
            {
                ZipOutputStream output = new ZipOutputStream(_stream);
                /*
                ZipEntry entry = new ZipEntry(@"empty");
                output.PutNextEntry(entry);
                output.CloseEntry();
                */
                output.Finish();

                _stream.Close();

                output.Dispose();
                _stream.Dispose();
                
                _stream = new FileStream(_strFilePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite);
            }

            _archive = new ZipFile(_stream);
            _archive.UseZip64 = UseZip64.On;
            _archive.BufferSize = 1024 * 1024 * 1024;
            _archive.BeginUpdate();

            //_archive.Add(@"C:\Users\01475\Desktop\Anmerkungen.txt", "Anmerkungen.txt");

            //_archive.CommitUpdate();
        }

        private void deinit()
        {
            try
            {
                if (_stream != null)
                {
                    if (_stream.CanRead || _stream.CanWrite)
                    {
                        if (_archive != null)
                        {
                            _archive.CommitUpdate();
                            _archive.Close();
                            _archive = null;
                        }

                        if (_stream != null)
                        {
                            _stream.Dispose();
                        }
                        _stream = null;
                    }
                }
            }
            finally { }
        }

        private List<string> getFoldersOf(string strDirectoryName)
        {
            String[] data = Directory.GetDirectories(strDirectoryName);
            List<string> output = new List<string>();

            foreach (string str in data)
            {
                output.Add(str);
            }

            return output;
        }

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

        private void add(string strDirectoryName, List<string> folders, List<string> files)
        {
            foreach (string folderName in folders)
            {
                List<string> resFolders = getFoldersOf(folderName);
                List<string> resFiles = getFilesOf(folderName);

                add(strDirectoryName, resFolders, resFiles);
            }

            foreach (string fileName in files)
            {
                int iLength = strDirectoryName.Length;
                string strEntryName = fileName.Substring(iLength + 1);
                strEntryName = strEntryName.Replace('\\', '/');
                _archive.Add(fileName, strEntryName);
            }
        }

        public void compress(string strDirectoryName)
        {
            init();

            List<string> resFolders = getFoldersOf(strDirectoryName);
            List<string> resFiles = getFilesOf(strDirectoryName);

            add(strDirectoryName, resFolders, resFiles);

            //_archive.Add(strDirectoryName + @"\", CompressionMethod.Stored);
            
            deinit();
        }

        ///////////////////////////////////////////

        private void reinit()
        {
            init();
        }
        ///////////////////////////////////////////

        private ZipEntry getEntry(string strEntryName)
        {
            if (_archive == null)
            {
                throw new Exception("Archive is null; failed to call in right order");
            }
            
            ZipEntry output = _archive.GetEntry(strEntryName);
            return output;
        }

        private void setEntryStream(string strEntryName, MemoryStream stream)
        {
            ZipEntry entry = getEntry(strEntryName);
            if (stream == null && entry != null)
            {
                _archive.Delete(entry);
                return;
            }

            DataSource s = new DataSource(stream);
            _archive.Add(s, strEntryName);
            //_archive.Add(@"C:\Users\01475\Desktop\Anmerkungen.txt", "Anmerkungen.txt");
        }

        private MemoryStream getEntryStream(string strEntryName)
        {
            ZipEntry entry = getEntry(strEntryName);
            if (entry == null)
            {
                return null;
            }

            MemoryStream output = new MemoryStream();

            using (Stream s = _archive.GetInputStream(entry.ZipFileIndex))
            {
                s.Position = 0;
                s.CopyTo(output);
            }

            return output;
        }

        ///////////////////////////////////////////

        public string FILENAME
        {
            get
            {
                return _strFilePath;
            }
        }

        ///////////////////////////////////////////

        ~OldLargeZipFileManagement()
        {
            deinit();
        }

        public void Dispose()
        {
            deinit();
        }

        ///////////////////////////////////////////


        public void createFolderIfNotExisting(string strInternalDirectory)
        {
            return;
        }

        public bool checkFolderExists(string strInternalDirectory)
        {
            return true;
        }

        public bool checkFileExists(string strInternalFileName)
        {
            bool blnOutput = true;
            blnOutput = getEntry(strInternalFileName) != null;
            return blnOutput;
        }

        public MemoryStream loadFromZip(string strInternalFileName)
        {
            MemoryStream output = null;
            output = getEntryStream(strInternalFileName);

            return output;
        }

        public bool saveToZip(string strInternalFileName, MemoryStream stream)
        {
            setEntryStream(strInternalFileName, stream);

            return true;
        }
    }
}
