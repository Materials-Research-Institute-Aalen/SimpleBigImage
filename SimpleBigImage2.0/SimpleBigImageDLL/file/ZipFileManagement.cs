using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;

namespace SimpleBigImage2
{
    public class ZipFileManagement : IDisposable
    {
        ZipArchive _archive;
        FileStream _stream;
        string _strFilePath;
        CompressionLevel _compression = CompressionLevel.NoCompression;
        Mutex MUTEX = new Mutex();

        public ZipFileManagement(string strFilePath, bool blnCreateNew = false)
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
                _archive.Dispose();
            }
            if (_stream != null)
            {
                _stream.Dispose();
            }

//            _stream = new FileStream(_strFilePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite);
//            _archive = new ZipArchive(_stream, ZipArchiveMode.Update);

            //_stream = new FileStream(_strFilePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Read);
            _stream = new FileStream(_strFilePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Read);
            _archive = new ZipArchive(_stream, ZipArchiveMode.Read);
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
                            _archive.Dispose();
                        }
                        _archive = null;
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

        ///////////////////////////////////////////

        private void reinit()
        {
            if (_archive != null)
            {
                _archive.Dispose();
            }
            if (_stream != null)
            {
                _stream.Dispose();
            }

            _stream = new FileStream(_strFilePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Read);
            _archive = new ZipArchive(_stream, ZipArchiveMode.Read);
        }

        private void getArchive()
        {
            MUTEX.WaitOne();

            /*
            if (_stream != null){
                _stream.Dispose();
            }
            if (_archive != null)
            {
                _archive.Dispose();
            }

            _stream = new FileStream(_strFilePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite);
            _archive = new ZipArchive(_stream, ZipArchiveMode.Update);
            */
            Random rand = new Random((int)DateTime.Now.Ticks);
            if (rand.NextDouble() < 0.01)
            {
                //reinit();
            }
            if (_stream == null)
            {
                throw new Exception("Archive is not initialized!");
            }
        }

        private void stopArchiveAccess()
        {
            /*
            if (_archive != null)
            {
                _archive.Dispose();
            }
            if (_stream != null)
            {
                _stream.Dispose();
            }
            */
            MUTEX.ReleaseMutex();
        }

        ///////////////////////////////////////////

        private ZipArchiveEntry getEntry(string strEntryName)
        {
            if (_archive == null)
            {
                throw new Exception("Archive is null; failed to call in right order");
            }
            strEntryName = strEntryName.Replace('\\', '/');
            ZipArchiveEntry output = _archive.GetEntry(strEntryName);
            return output;
        }

        private void createEntry(string strEntryName)
        {
            if (_archive == null)
            {
                throw new Exception("Archive is null; failed to call in right order");
            }

            _archive.CreateEntry(strEntryName, _compression);
            _stream.FlushAsync();
        }

        private void setEntryStream(string strEntryName, MemoryStream stream)
        {
            ZipArchiveEntry entry = getEntry(strEntryName);
            if (entry == null)
            {
                createEntry(strEntryName);
                entry = getEntry(strEntryName);
                if (entry == null)
                {
                    throw new Exception("Access to ZIP seems denied or corrupt!");
                }
            }
            if (stream == null)
            {
                entry.Delete();
                return;
            }
            using (Stream s = entry.Open())
            {
                stream.Position = 0;
                stream.CopyTo(s);
                stream.Flush();
            }
            _stream.FlushAsync();
            //_stream.Flush();
        }

        private MemoryStream getEntryStream(string strEntryName)
        {
            ZipArchiveEntry entry = getEntry(strEntryName);
            if (entry == null)
            {
                return null;
            }

            MemoryStream output = new MemoryStream();

            using (Stream s = entry.Open())
            {
                //s.Position = 0;
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

        ~ZipFileManagement()
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
            getArchive();
            bool blnOutput = true;
            blnOutput = getEntry(strInternalFileName) != null;
            stopArchiveAccess();
            return blnOutput;
        }

        public MemoryStream loadFromZip(string strInternalFileName)
        {
            MemoryStream output = null;
            getArchive();
            output = getEntryStream(strInternalFileName);
            stopArchiveAccess();

            return output;
        }

        public bool saveToZip(string strInternalFileName, MemoryStream stream)
        {
            getArchive();
            setEntryStream(strInternalFileName, stream);
            stopArchiveAccess();

            return true;
        }
    }
}
