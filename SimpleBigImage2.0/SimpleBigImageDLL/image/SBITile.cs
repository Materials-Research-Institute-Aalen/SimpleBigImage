using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using SimpleImageProcessing;

//using Emgu.CV;
//using Emgu.CV.Structure;

namespace SimpleBigImage2
{
    /// <summary>
    /// Represents a single tile of the whole image
    /// </summary>
    public class SBITile
    {
        static Image emptyImage = null;
        //static System.IO.Stream emptyStream = new System.IO.MemoryStream();

        private int iUsedCounter = 0;

        private bool _blnActive;

        ///x Position of the tile
        protected int _x;
        ///y Position of the tile
        protected int _y;
        ///depth Position of the tile
        protected int _depth; 

        private string _strImageIdentifier;
        private string _strFolder;

        private bool _blnEmpty = false;
        private bool _blnLoaded;
        private Image _imageTile;
        //private System.IO.Stream _stream;
        private int _size;
        private int _iLastOperationID;
        private bool _blnChanged;
        private Mutex _MUTEX = new Mutex();

        private SBITileFileManagement _file;

        //private bool _blnIsZipFile;
        private ZipFileManagement _zfmgmt;

        public void mtUse()
        {
            iUsedCounter++;
        }

        public void mtUnUse()
        {
            iUsedCounter--;
        }

        public bool mtInUse()
        {
            return iUsedCounter > 0;
        }

        public int X
        {
            get
            {
                return _x;
            }
        }

        public int Y
        {
            get
            {
                return _y;
            }
        }

        /// <summary>
        /// A static function which gives back "true" if the piece is allowed to exist in that part.
        /// </summary>
        /// <param name="xPos">The x pos of the tile in tile positions</param>
        /// <param name="yPos">The y pos of the tile in tile positions</param>
        /// <param name="depth">The depth level of the piece to check</param>
        /// <returns>true or false: true, if the piece is allowed to exist there</returns>
        public static bool checkPieceExists(int xPos, int yPos, int depth)
        {
            int iStepSize = (int)Math.Pow(2, depth);
            if (iStepSize < 2 || (xPos % iStepSize == 0) && (yPos % iStepSize == 0))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Internal use only: Loads an image file (tile image)
        /// </summary>
        /// <param name="strName">The name of the file</param>
        /// <param name="iOperationID">The operation id</param>
        /// <returns>an image file</returns>
        private void loadImage(string strName, int iOperationID)
        {
            try
            {
                _iLastOperationID = iOperationID;
                if (_imageTile != null)
                {
                    _imageTile.Dispose();
                }
                _imageTile = _file.loadImage(_strImageIdentifier);
                _blnLoaded = true;
            }
            catch
            {
                _imageTile = emptyImage;
            }
        }

        private void saveImage(Image img, string strName)
        {
            _file.saveImage(_strImageIdentifier, img);
        }

        /// <summary>
        /// Constructor: Internal use for SBImage
        /// </summary>
        /// <param name="iSize">The pixel size of single tiles</param>
        /// <param name="strFolder">The main folder</param>
        /// <param name="xPos">The tile x position in tile positions</param>
        /// <param name="yPos">The tile y position in tile positions</param>
        /// <param name="depth">The depth layer the tile is in</param>
        public SBITile(ZipFileManagement zfmgmt, bool blnCheckFileExists, int iSize, string strFolder, int xPos, int yPos, int depth)
        {
            if (emptyImage == null)
            {
                emptyImage = new Bitmap(iSize, iSize);
            }

            if (!checkPieceExists(xPos, yPos, depth))
            {
                throw new Exception("Tried to create non-existend tile: X:" + xPos.ToString() + " Y:" + yPos.ToString() + " depth:" + depth.ToString());
            }

            _strFolder = strFolder;

            _zfmgmt = zfmgmt;

            _x = xPos;
            _y = yPos;
            _depth = depth;
            _size = iSize;

            setOwnFile();
            if (blnCheckFileExists)
            {
                createFolder();
            }

            _imageTile = null;
            _blnLoaded = false;
            _blnActive = true;

            _blnEmpty = false;

            _blnChanged = false;
        }

        /// <summary>
        /// Constructor: Internal use for SBImage
        /// </summary>
        /// <param name="iSize">The pixel size of single tiles</param>
        /// <param name="xPos">The tile x position in tile positions</param>
        /// <param name="yPos">The tile y position in tile positions</param>
        /// <param name="depth">The depth layer the tile is in</param>
        public SBITile(ZipFileManagement zfmgmt, bool blnCheckFileExists, int iSize, int xPos, int yPos, int depth)
        {
            if (emptyImage == null)
            {
                emptyImage = new Bitmap(iSize, iSize);
            }

            if (!checkPieceExists(xPos, yPos, depth))
            {
                throw new Exception("Tried to create non-existend tile: X:" + xPos.ToString() + " Y:" + yPos.ToString() + " depth:" + depth.ToString());
            }

            _strFolder = "";

            _zfmgmt = zfmgmt;

            _x = xPos;
            _y = yPos;
            _depth = depth;
            _size = iSize;

            setOwnFile();
            if (blnCheckFileExists)
            {
                createFolder();
            }

            _imageTile = null;
            _blnLoaded = false;
            _blnActive = false;

            _blnEmpty = true;

            _blnChanged = false;
        }

        private void _save()
        {
            if (_blnEmpty)
            {
                return;
            }
            if (_blnActive) //Es macht keinen Sinn, inaktive Kacheln zu speichern.
            {
                if (_imageTile != null)
                {
                    saveImage(_imageTile, _strImageIdentifier);
                }
                else
                {
                    _blnActive = false;
                }
            }
        }

        /// <summary>
        /// Saves the tile
        /// </summary>
        public void save()
        {

            _MUTEX.WaitOne();


            try
            {
                _save();
            }
            finally
            {
            }

            _MUTEX.ReleaseMutex();
        }

        private string[] getSubFolders()
        {
            return new string[] { _depth.ToString(), _x.ToString(), _y.ToString(), "main.png" };
        }

        private void setOwnFile()
        {
            string[] astrFolders = getSubFolders();
            _file = new SBITileFileManagement(_strFolder, _zfmgmt);
            _strImageIdentifier = _file.getTileFile(_strFolder, astrFolders);
        }

        /// <summary>
        /// For internal use only
        /// </summary>
        private void createFolder()
        {
            string[] astrFolders = getSubFolders();
            _file.createFolder(_strFolder, astrFolders);
        }

        public void prepare(string strFolder)
        {
            _strFolder = strFolder;
            setOwnFile();
            createFolder();
        }

        /// <summary>
        /// Saves the tile in a certain folder
        /// </summary>
        /// <param name="strFolder">The folder</param>
        /// <param name="iOperationID">The operation id</param>
        /// <param name="iMaxOperationDifference">The allowed operation difference</param>
        public void saveAs(string strFolder, int iOperationID = 0, int iMaxOperationDifference = 10)
        {
            try
            {

                _load(0);

                _strFolder = strFolder;
                setOwnFile();
                createFolder();
                
                saveImage(_imageTile, _strImageIdentifier);

                if (iOperationID > _iLastOperationID + iMaxOperationDifference)
                {
                    _unload();
                }

                _blnChanged = false;
            }
            finally
            {
            }
        }

        private void load(int iOperationID)
        {
            try
            {
                _load(iOperationID);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Loads the tile
        /// </summary>
        /// <param name="iOperationID">The operation ID</param>
        private void _load(int iOperationID)
        {

            if (_blnEmpty)
            {
                _imageTile = emptyImage;
                return;
            }
            if (!_blnLoaded)
            {
                if (_strImageIdentifier != null && _file.fileExists(_strImageIdentifier))
                {
                    loadImage(_strImageIdentifier, iOperationID);
                }
                else
                {
                    _imageTile = emptyImage;
                }
                _blnLoaded = true;
            }

            _blnChanged = false;

        }

        public void unload()
        {
            try
            {
                _unload();
            }
            finally
            {
            }

        }

        /// <summary>
        /// Unloads the tile, saves it if necessairy
        /// </summary>
        private void _unload()
        {

            _MUTEX.WaitOne();


            if (_blnLoaded && !_blnEmpty)
            {
                _blnLoaded = false;
                if (_imageTile != emptyImage)
                {
                    if (_blnChanged)
                    {
                        if (_strFolder != "")
                        {
                            _save();
                        }
                    }
                    if (_imageTile != null)
                    {
                        _imageTile.Dispose();
                    }
                }
                _imageTile = null;
            }

            _MUTEX.ReleaseMutex();

        }

        /// <summary>
        /// Checks if the Tile is loaded (and loads it with given operationID, if active but unloaded)
        /// </summary>
        /// <param name="iOperationID">The id of the operation</param>
        public void checkLoaded(int iOperationID)
        {
            if (_blnEmpty)
            {
                return;
            }
            if (_blnLoaded != _blnActive)
            {
                if (_blnActive)
                {
                    load(iOperationID);
                }
                else
                {
                    unload();
                }
            }
        }

        /// <summary>
        /// Gives back the image file stored in the tile - autoload when needed
        /// </summary>
        /// <param name="iOperationID">Operation ID</param>
        /// <returns>the image file</returns>
        public Image getImage(int iOperationID)
        {

            _MUTEX.WaitOne();

            if (_blnLoaded && _imageTile == null)
            {
                _blnLoaded = false;
            }
            if (!_blnLoaded)
            {
                load(iOperationID);
            }

            Image clone = ImageCopier.CLONE(_imageTile);

            _MUTEX.ReleaseMutex();
           
            return clone;
        }

        /// <summary>
        /// Sets the tile "active" so it can be disposed later on
        /// </summary>
        public void setActive()
        {
            _blnActive = true;
        }

        /// <summary>
        /// Copys the given image file and sets it as tile image
        /// </summary>
        /// <param name="input">the image to be set</param>
        /// <param name="blnSave">if set to "true", it automatically saves</param>
        public void setImage(Image input, bool blnSave = false)
        {
            _MUTEX.WaitOne();

            try
            {
                if (input.Width != _size || input.Height != _size)
                {
                    throw new Exception("SBITile: Cannot set new image; Wrong size");
                }
                if (_imageTile != emptyImage && _imageTile != null)
                {
                    _imageTile.Dispose();
                }

                _imageTile = SimpleImageProcessing.ImageCopier.CLONE(input);
                _blnChanged = true;

                _blnEmpty = false; //Das hat sich wohl erledigt.
                _blnLoaded = true; //Da ein Bild übergeben wurde, ist es automatisch geladen.
                _blnActive = true; //Es wurde ein Bild "aktiviert".

                if (blnSave)
                {
                    _save();
                }
            }
            finally
            {
            }

            _MUTEX.ReleaseMutex();
        }

        /// <summary>
        /// automatically unloads the Tile if needed
        /// </summary>
        /// <param name="iCurrentOperationID">The operation id</param>
        /// <param name="iMaxOperationDifference">How many operations before unloading.</param>
        public void checkUnload(int iCurrentOperationID, int iMaxOperationDifference)
        {
            try
            {
                if (iCurrentOperationID - _iLastOperationID > iMaxOperationDifference)
                {
                    _unload();
                }
            }
            finally
            {
            }
        }

        /// <summary>
        /// "Active" means, if this is true, the tile is actually an active, existing tile. Inactive tiles can neither be loaded, nor stored, not shown. Optimally they do not need resources then.
        /// </summary>
        public bool Active
        {
            get
            {
                //return _blnLoaded && _blnActive;
                return _blnActive;
            }
        }

        public bool LOADED
        {
            get
            {
                return _blnLoaded;
            }
        }

        public int TILESIZE
        {
            get
            {
                return _size;
            }
        }

        public string FILENAME
        {
            get
            {
                return _strImageIdentifier;
            }
        }
    }
}
