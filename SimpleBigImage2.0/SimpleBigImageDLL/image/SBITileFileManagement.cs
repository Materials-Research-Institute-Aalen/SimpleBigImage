using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace SimpleBigImage2
{
    public class SBITileFileManagement
    {
        ZipFileManagement _zfmgmt;
        string _strFolderName;

        /// <summary>
        /// Creates a tile management system
        /// </summary>
        /// <param name="strTarget">The target folder</param>
        /// <param name="zfmgmt">The FileManagementSystem</param>
        public SBITileFileManagement(string strTarget, ZipFileManagement zfmgmt)
        {
            if (zfmgmt != null)
            {
                _zfmgmt = zfmgmt;//new ZipFileManagement(strTarget);
            }
            else
            {
                _zfmgmt = null;
                _strFolderName = strTarget;
            }
        }

        /// <summary>
        /// Loads an image with the filename
        /// </summary>
        /// <param name="strFileName">The tile file name</param>
        /// <returns>an Image</returns>
        public Image loadImage(string strFileName)
        {
            if (_zfmgmt == null)
            {
                return loadImageFromFile(strFileName);
            }
            return loadImageFromZip(strFileName);
        }

        /// <summary>
        /// Loads image from file
        /// </summary>
        /// <param name="strFile">The filename</param>
        /// <returns>An Image</returns>
        private Image loadImageFromFile(string strFile)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            using (System.IO.FileStream str = new System.IO.FileStream(strFile, System.IO.FileMode.Open))
            {
                str.Position = 0;
                str.CopyTo(stream);
                str.Close();
            }

            Image output = Image.FromStream(stream);

            return output;
        }

        /// <summary>
        /// Loads image from ZIP
        /// </summary>
        /// <param name="strFile">The internal filename</param>
        /// <returns>An Image</returns>
        private Image loadImageFromZip(string strFile)
        {
            System.IO.MemoryStream stream = _zfmgmt.loadFromZip(strFile);

            Image output = Image.FromStream(stream);

            return output;
        }

        /// <summary>
        /// saves the Image in a file
        /// </summary>
        /// <param name="strFile">The Filename to save in</param>
        /// <param name="img">The image to save</param>
        public void saveImage(string strFile, Image img)
        {
            if (_zfmgmt == null)
            {
                saveImageToFile(strFile, img);
                return;
            }
            saveImageToZip(strFile, img);
        }

        /// <summary>
        /// saves the Image in a file
        /// </summary>
        /// <param name="strFile">The Filename to save in</param>
        /// <param name="img">The image to save</param>
        private void saveImageToFile(string strFile, Image img)
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                if (img.PixelFormat != PixelFormat.DontCare)
                {
                    img.Save(stream, ImageFormat.Png);
                    using (System.IO.FileStream str = new System.IO.FileStream(strFile, System.IO.FileMode.Create))
                    {
                        stream.Position = 0;
                        stream.CopyTo(str);
                        str.Flush();
                        str.Close();
                    }
                }
            }
        }

        /// <summary>
        /// saves the Image in a zip
        /// </summary>
        /// <param name="strFile">The internal name</param>
        /// <param name="img">The image to save</param>
        private void saveImageToZip(string strFile, Image img)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            img.Save(stream, ImageFormat.Png);

            _zfmgmt.saveToZip(strFile, stream);

            stream.Dispose();
        }

        /// <summary>
        /// Creates folders inside the mainfolder (ZIP)
        /// </summary>
        /// <param name="strCurrFolder">Target Folder</param>
        /// <param name="astrFolders">The folders to put in</param>
        public void createFolder(string strCurrFolder, string[] astrFolders)
        {
            if (_zfmgmt == null)
            {
                for (int i = 0; i < astrFolders.Length - 1; i++)
                {
                    strCurrFolder += @"\" + astrFolders[i];
                    if (!System.IO.Directory.Exists(strCurrFolder))
                    {
                        System.IO.Directory.CreateDirectory(strCurrFolder);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the filenames
        /// </summary>
        /// <param name="strCurrFolder">The current folder</param>
        /// <param name="astrFolders">gets the Filename of the Tiles</param>
        /// <returns>The filename</returns>
        public string getTileFile(string strCurrFolder, string[] astrFolders)
        {
            string strFileName = "";
            if (_zfmgmt == null)
            {
                strFileName += strCurrFolder;
                for (int i = 0; i < astrFolders.Length; i++)
                {
                    strFileName += @"\" + astrFolders[i];
                }
            }
            else
            {
                strFileName = astrFolders[0];
                for (int i = 1; i < astrFolders.Length; i++)
                {
                    strFileName += @"\" + astrFolders[i];
                }
            }
            
            return strFileName;
        }

        /// <summary>
        /// Checks if files exist
        /// </summary>
        /// <param name="strFile"></param>
        /// <returns></returns>
        public bool fileExists(string strFile)
        {
            if (_zfmgmt != null)
            {
                return _zfmgmt.checkFileExists(strFile);
            }
            return System.IO.File.Exists(strFile);
        }
    }
}
