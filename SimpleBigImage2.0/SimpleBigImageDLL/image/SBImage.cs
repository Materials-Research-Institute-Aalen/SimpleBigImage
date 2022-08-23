using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using Emgu.CV;
//using Emgu.CV.Structure;

using System.Xml;
using System.Globalization;
using System.Drawing;
using System.IO;

namespace SimpleBigImage2
{
    /// <summary>
    /// This class represents a tile image with pyramid.
    /// It is specialized on holding the image, with memory management.
    /// </summary>
    public class SBImage//<ColorType> where ColorType : struct, IColor
    {
        int _iImageTileSize;
        SBITile[][,] _images;
        SBICutter _cutter;
        long _iXSize;
        long _iYSize;
        int _iDepth;

        long _iWidth;
        long _iHeight;

        string _strTarget;
        bool _blnMultithreadingMode = false;

        private int _iOperationCount;
        private int _iMaxOperationDifference;

        //Deep Variables
        private double _dblXScaling = 1;
        private double _dblYScaling = 1;

        private double _dblXOffset = 0;
        private double _dblYOffset = 0;

        private long _iPixelWidth = 0;
        private long _iPixelHeight = 0;

        private ZipFileManagement _zfmgmt;

        /// <summary>
        /// Sets the image to work for multithreaded
        /// </summary>
        public void setMultithreaded()
        {
            _blnMultithreadingMode = true;
        }

        /// <summary>
        /// Sets the image to work singlethreaded
        /// </summary>
        public void setSingleThreaded()
        {
            _blnMultithreadingMode = false;
            checkImageUsageSingleThreaded();
        }

        /// <summary>
        /// Writes an XML, can be either in a folder, or a zip file
        /// </summary>
        /// <param name="strFolder">the folder to write in</param>
        /// <param name="strXMLFile">the xml file</param>
        /// <param name="x">x size in tile count</param>
        /// <param name="y">y size in tile count</param>
        /// <param name="z">depth in tile count</param>
        /// <param name="iTileSize">the tile size</param>
        private void writeXML(string strFolder, string strXMLFile, long x, long y, int z, int iTileSize)
        {
            XmlWriter xmlWriter;
            MemoryStream stream = new MemoryStream();
            if (_zfmgmt == null)
            {
                xmlWriter = XmlWriter.Create(strFolder + @"\" + strXMLFile);
            }
            else
            {
                xmlWriter = XmlWriter.Create(stream);
            }

            xmlWriter.WriteStartDocument();

            xmlWriter.WriteStartElement("size");
            xmlWriter.WriteAttributeString("x", x.ToString());
            xmlWriter.WriteAttributeString("y", y.ToString());
            xmlWriter.WriteAttributeString("depth", z.ToString());
            xmlWriter.WriteAttributeString("xScale", Convert.ToString(_dblXScaling, new CultureInfo("en-US")));
            xmlWriter.WriteAttributeString("yScale", Convert.ToString(_dblYScaling, new CultureInfo("en-US")));
            xmlWriter.WriteAttributeString("xOffset", Convert.ToString(_dblXOffset, new CultureInfo("en-US")));
            xmlWriter.WriteAttributeString("yOffset", Convert.ToString(_dblYOffset, new CultureInfo("en-US")));
            xmlWriter.WriteAttributeString("pixelWidth", Convert.ToString(_iPixelWidth, new CultureInfo("en-US")));
            xmlWriter.WriteAttributeString("pixelHeight", Convert.ToString(_iPixelHeight, new CultureInfo("en-US")));
            xmlWriter.WriteAttributeString("TileSize", iTileSize.ToString());

            xmlWriter.WriteEndDocument();
            xmlWriter.Close();

            if (_zfmgmt != null)
            {
                _zfmgmt.saveToZip(strXMLFile, stream);
                stream.Dispose();
            }

        }

        /// <summary>
        /// Reads an xml file, either to a folder, or to an zip file
        /// </summary>
        /// <param name="strFolder">the folder to put the file in</param>
        /// <param name="strXMLFile">the file name</param>
        private void readXML(string strFolder, string strXMLFile)
        {
            XmlTextReader reader;// = new XmlTextReader(strFolder + strXMLFile);
            MemoryStream stream = null;// = new MemoryStream();
            if (_zfmgmt == null)
            {
                reader = new XmlTextReader(strFolder + @"\" + strXMLFile);
            }
            else
            {
                stream = _zfmgmt.loadFromZip(strXMLFile);
                stream.Position = 0;
                reader = new XmlTextReader(stream);
            }
            
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        while (reader.MoveToNextAttribute())
                        { // Read the attributes.
                            switch (reader.Name)
                            {
                                case "x":
                                    _iXSize = Convert.ToInt64(reader.Value);
                                    break;
                                case "y":
                                    _iYSize = Convert.ToInt64(reader.Value);
                                    break;
                                case "depth":
                                    _iDepth = Convert.ToInt32(reader.Value);
                                    break;
                                case "xScale":
                                    _dblXScaling = Convert.ToDouble(reader.Value, new CultureInfo("en-US"));
                                    break;
                                case "yScale":
                                    _dblYScaling = Convert.ToDouble(reader.Value, new CultureInfo("en-US"));
                                    break;
                                case "xOffset":
                                    _dblXOffset = Convert.ToDouble(reader.Value, new CultureInfo("en-US"));
                                    break;
                                case "yOffset":
                                    _dblYOffset = Convert.ToDouble(reader.Value, new CultureInfo("en-US"));
                                    break;
                                case "pixelWidth":
                                    _iPixelWidth = Convert.ToInt64(reader.Value, new CultureInfo("en-US"));
                                    break;
                                case "pixelHeight":
                                    _iPixelHeight = Convert.ToInt64(reader.Value, new CultureInfo("en-US"));
                                    break;
                                case "TileSize":
                                    _iImageTileSize = Convert.ToInt32(reader.Value);
                                    break;
                            }
                        }
                        break;
                    case XmlNodeType.Text: //Display the text in each element.
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        break;
                }
            }

            if (stream != null)
            {
                stream.Dispose();
            }
        }

        /// <summary>
        /// Generates the repesentation of the image in the RAM
        /// </summary>
        /// <param name="blnActiveFolder">Does it have an existing folder? Relevant for non-zip folder strategy</param>
        private void generateImageRepresentation(bool blnActiveFolder = true, bool blnCheckFileExists = true)
        {
            _images = new SBITile[_iDepth][,];
            for (int depth = 0; depth < _iDepth; depth++)
            {
                int iDepthRepresentation = 1 << depth;
                int iCurrMaxX = (int)(Math.Ceiling((double)_iXSize / (double)iDepthRepresentation));
                int iCurrMaxY = (int)(Math.Ceiling((double)_iYSize / (double)iDepthRepresentation));

                _images[depth] = new SBITile[iCurrMaxX, iCurrMaxY];
                for (int x = 0; x < iCurrMaxX; x++)
                {
                    for (int y = 0; y < iCurrMaxY; y++)
                    {
                        if (blnActiveFolder)
                        {
                            _images[depth][x, y] = new SBITile( _zfmgmt, blnCheckFileExists, _iImageTileSize, _strTarget, x * iDepthRepresentation, y * iDepthRepresentation, depth);
                        }
                        else
                        {
                            _images[depth][x, y] = new SBITile(_zfmgmt, blnCheckFileExists, _iImageTileSize, x * iDepthRepresentation, y * iDepthRepresentation, depth);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Load a Simple Big Image from a folder
        /// </summary>
        /// <param name="strTarget">The SBI Folder.</param>
        /// <param name="iUnloadImageAt">Count of operations Tiles will stay stored.</param>
        public SBImage(string strTarget, int iUnloadImageAt = 10)
        {
            //ToDo: Implement "Read Only" for ZIP files
            _iMaxOperationDifference = iUnloadImageAt;
            string strFileEndingSBIZIP = strTarget.Substring(strTarget.Length - 8, 8);
            string strFileEndingSBIOnly = strTarget.Substring(strTarget.Length - 4, 4);
            if (strFileEndingSBIZIP.ToLower() == ".sbi.zip")// || strFileEndingSBIOnly.ToLower() == ".sbi")
            {
                _zfmgmt = new ZipFileManagement(strTarget, false);
            }
            load(strTarget);
        }

        /// <summary>
        /// Creates a new SBImage
        /// </summary>
        /// <param name="strTarget">The target folder, or the .zip file</param>
        /// <param name="iWidth">the total width of the image</param>
        /// <param name="iHeight">the total height of the image</param>
        /// <param name="dblXScaling">the scaling in x direction</param>
        /// <param name="dblYScaling">the scaling in y direction</param>
        /// <param name="dblXOffset">the x position of the stage</param>
        /// <param name="dblYOffset">the y position of the stage</param>
        /// <param name="iTileSize">the size of each tile</param>
        /// <param name="iUnloadImageAt">a parameter that "unloads" tiles after a certain amout of processing steps</param>
        public SBImage(string strTarget, long iWidth, long iHeight, double dblXScaling = 1, double dblYScaling = 1, double dblXOffset = 0, double dblYOffset = 0, int iTileSize = 1024, int iUnloadImageAt = 10)
        {
            _strTarget = strTarget;

            long iXSize = (long)(Math.Ceiling((double)iWidth / (double)iTileSize));
            long iYSize = (long)(Math.Ceiling((double)iHeight / (double)iTileSize));

            long iDepth = iXSize > iYSize ? iXSize : iYSize;

            if (iDepth == 0)
            {
                iDepth = 1;
            }
            iDepth = (int)Math.Ceiling(Math.Log(iDepth, 2.0)) - 1;
            if (iDepth < 1)
            {
                iDepth = 1;
            }

            if (strTarget.Substring(strTarget.Length - 8, 8) == ".SBI.zip")
            {
                _zfmgmt = new ZipFileManagement(strTarget, true);
            }

            _iXSize = iXSize;
            _iYSize = iYSize;
            _iDepth = (int)iDepth;
            _iImageTileSize = iTileSize;
            _iMaxOperationDifference = iUnloadImageAt;


            generateImageRepresentation(true);

            _iPixelHeight = iHeight;
            _iPixelWidth = iWidth;

            _iWidth = _iXSize * _iImageTileSize;
            _iHeight = _iYSize * _iImageTileSize;

            _dblXScaling = dblXScaling;
            _dblYScaling = dblYScaling;

            
            writeXML(_strTarget, @"data.xml", _iXSize, _iYSize, _iDepth, _iImageTileSize);

            _cutter = new SBICutter(_iImageTileSize, (int)(iDepth - 1));
        }        

        /// <summary>
        /// Creates an empty Simple Big Image
        /// </summary>
        /// <param name="iXSize">X Tile Count of the SBI</param>
        /// <param name="iYSize">Y Tile Count of the SBI</param>
        /// <param name="iDepth">Tree depth of the SBI</param>
        /// <param name="iTileSize">Size of the tiles</param>
        /// <param name="iUnloadImageAt">Count of operations Tiles will stay stored</param>
        public SBImage(string strTarget, int iXSize, int iYSize, int iDepth, double dblXScaling, double dblYScaling, double dblXOffset, double dblYOffset, int iTileSize = 1024, int iUnloadImageAt = 10)
        {
            _iXSize = iXSize;
            _iYSize = iYSize;
            _iDepth = iDepth;
            _iImageTileSize = iTileSize;
            _iMaxOperationDifference = iUnloadImageAt;

            string strFileEndingSBIZIP = strTarget.Substring(strTarget.Length - 8, 8);
            string strFileEndingSBIOnly = strTarget.Substring(strTarget.Length - 4, 4);
            if (strFileEndingSBIZIP == ".SBI.zip" || strFileEndingSBIOnly.ToLower() == ".sbi")
            {
                _zfmgmt = new ZipFileManagement(strTarget, true);
                _strTarget = strTarget;
                generateImageRepresentation();
            }
            else
            {
                generateImageRepresentation(false);
            }



            _iWidth = _iXSize * _iImageTileSize;
            _iHeight = _iYSize * _iImageTileSize;

            _dblXScaling = dblXScaling;
            _dblYScaling = dblYScaling;

            _cutter = new SBICutter(_iImageTileSize, _iDepth - 1);
        }

        /// <summary>
        /// Loads the simple big image from a folder
        /// </summary>
        /// <param name="strFolder">the image folder</param>
        public void load(string strFolder = "")
        {
            if (_strTarget != "" && _images != null)
            {
                for (int i = 0; i < _images.Length; i++)
                {
                    foreach (SBITile t in _images[i])
                    {
                        if (t.Active)
                        {
                            t.unload();
                        }
                    }
                }
            }
            //string strFile = strFolder + @"\data.xml";
            _strTarget = strFolder;
            _iImageTileSize = -1;
            _iOperationCount = 0;

            readXML(strFolder, @"data.xml");

            if (_iImageTileSize == -1)
            {
                try
                {
                    Bitmap sizeGetter = new Bitmap(strFolder + @"\0\0\0\texture.png");
                    _iImageTileSize = sizeGetter.Width;
                    sizeGetter.Dispose();
                }
                catch
                {
                    _iImageTileSize = 1024;
                }
            }

            if (_iDepth == 0)
            {
                string[] dirs = System.IO.Directory.GetDirectories(strFolder);
                _iDepth = dirs.Length;
            }

            generateImageRepresentation(true, false);
            _cutter = new SBICutter(_iImageTileSize, _iDepth - 1);

            _iWidth = _iXSize * _iImageTileSize;
            _iHeight = _iYSize * _iImageTileSize;
        }

        /// <summary>
        /// Saves the current SBI
        /// </summary>
        public void save()
        {
            for (int i = 0; i < _images.Length; i++)
            {
                foreach (SBITile t in _images[i])
                {
                    t.save();
                }
            }
        }

        private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
        }

        public void cloneInto(string strFolder)
        {
            if (_zfmgmt == null)
            {
                save();
                if (!System.IO.Directory.Exists(strFolder))
                {
                    System.IO.Directory.CreateDirectory(strFolder);
                }
                else
                {
                    System.IO.Directory.Delete(strFolder, true);
                    System.IO.Directory.CreateDirectory(strFolder);
                }
                DirectoryInfo source = new DirectoryInfo(_strTarget);
                DirectoryInfo target = new DirectoryInfo(strFolder);
                CopyFilesRecursively(source, target);

            }
            else
            {
                save();
                if (File.Exists(strFolder))
                {
                    File.Delete(strFolder);
                }
                File.Copy(_strTarget, strFolder);
            }
        }

        /// <summary>
        /// Saves the SBI in the predefined folder
        /// </summary>
        /// <param name="strFolder">The folder</param>
        public void saveAs(string strFolder)
        {
            if (_zfmgmt == null)
            {
                if (!System.IO.Directory.Exists(strFolder))
                {
                    System.IO.Directory.CreateDirectory(strFolder);
                }


                for (int i = 0; i < _images.Length; i++)
                {
                    foreach (SBITile t in _images[i])
                    {
                        try
                        {
                            t.saveAs(strFolder, OPERATIONS, _iMaxOperationDifference);
                        }
                        catch// (Exception ex)
                        {

                            //Logger.err("saveAs/Tile error: " + ex.ToString() + "! //// " + strFolder + "\r\n-----\r\n", ex.Message, ex.StackTrace);
                        }
                    }
                }
                writeXML(strFolder, @"data.xml", _iXSize, _iYSize, _iDepth, _iImageTileSize);
            }
            else
            {
                save();
                if (File.Exists(strFolder))
                {
                    File.Delete(strFolder);
                }
                File.Copy(_strTarget, strFolder);
                if (strFolder.Substring(strFolder.Length - 8, 8) == ".SBI.zip")
                {
                    _zfmgmt = new ZipFileManagement(strFolder, false);
                }
                load(strFolder);
            }
        }

        /// <summary>
        /// Saves the SBI in the predefined folder
        /// </summary>
        /// <param name="strFolder">The folder</param>
        public void createSubDirectories(string strFolder)
        {
            if (!System.IO.Directory.Exists(strFolder))
            {
                System.IO.Directory.CreateDirectory(strFolder);
            }
            
            for (int i = 0; i < _images.Length; i++)
            {
                foreach (SBITile t in _images[i])
                {
                    try
                    {
                        t.prepare(strFolder);
                    }
                    catch// (Exception ex)
                    {
                        //Logger.err("Folder preparation error: " + ex.ToString() + "! //// " + strFolder + "\r\n-----\r\n", ex.Message, ex.StackTrace);
                    }
                }
            }
            writeXML(strFolder, @"data.xml", _iXSize, _iYSize, _iDepth, _iImageTileSize);
        }

        /// <summary>
        /// Internal use only
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="depth"></param>
        private void unloadTile(int x, int y, int depth)
        {
            _images[depth][x, y].unload();
        }

        /// <summary>
        /// Internal use only
        /// </summary>
        private void checkImageUsageSingleThreaded()
        {
            if (!MULTITHREADED)
            {
                try
                {
                    foreach (SBITile[,] part in _images)
                    {
                        foreach (SBITile img in part)
                        {
                            if (img.LOADED || img.Active)
                            {
                                img.checkUnload(_iOperationCount, _iMaxOperationDifference);
                            }
                        }
                    }
                }
                catch// (Exception ex) 
				{
                    //Logger.err("checkImageUsageSingleThreaded: " + ex.ToString() + "! ////\r\n-----\r\n", ex.Message, ex.StackTrace);
				};
            }
            //GC.Collect();
        }

        /// <summary>
        /// Does the memory management for tiles - one thread to rule them all
        /// </summary>
        public void checkImageUsageMultithreaded()
        {
            if (!MULTITHREADED)
            {
                throw new Exception("Tried to execute Multithread code in Singlethread mode!");
            }

            foreach (SBITile[,] part in _images)
            {
                foreach (SBITile img in part)
                {
                    if (img.Active && (!img.mtInUse()))
                    {
                        img.checkUnload(_iOperationCount, _iMaxOperationDifference);
                    }
                }
            }
        }

        /// <summary>
        /// Internal use only
        /// </summary>
        private int OPERATIONS
        {
            get
            {
                return _iOperationCount++;
            }
        }

        /// <summary>
        /// The tile count in x direction at a given level
        /// </summary>
        /// <param name="iLevel">the level of depth in the pyramid</param>
        /// <returns></returns>
        public int getXSize(int iLevel = 0)
        {
            return _images[iLevel].GetLength(0);
        }

        /// <summary>
        /// The tile count in y direction at a given level
        /// </summary>
        /// <param name="iLevel">the level of depth in the pyramid</param>
        /// <returns></returns>
        public int getYSize(int iLevel = 0)
        {
            return _images[iLevel].GetLength(1);
        }

        /// <summary>
        /// Puts an image onto the SBI
        /// </summary>
        /// <param name="img">The image/Bitmap</param>
        /// <param name="iXStart">The x position in pixels</param>
        /// <param name="iYStart">The y position in pixels</param>
        public void setImagePart(Image img, long iXStart, long iYStart)
        {
            _cutter.setImagePart(_images, img, iXStart, iYStart, OPERATIONS);
            checkImageUsageSingleThreaded();
        }

        /// <summary>
        /// Gives back the wanted image part
        /// </summary>
        /// <param name="iXStart">X Start position</param>
        /// <param name="iYStart">Y Start position</param>
        /// <param name="iWidth">Width of the cut image in pixels</param>
        /// <param name="iHeight">Height of the cut image in pixels</param>
        /// <returns></returns>
        public Image getImagePart(long iXStart, long iYStart, long iWidth, long iHeight)
        {
            Image output = _cutter.getImagePart(_images, iXStart, iYStart, iWidth, iHeight, 0, OPERATIONS);
            checkImageUsageSingleThreaded();

            return output;
        }

        /// <summary>
        /// Returns the tile in the given position
        /// </summary>
        /// <param name="iXPos">The x tile position inside the array</param>
        /// <param name="iYPos">The y tile position inside the array</param>
        /// <param name="iDepth">The level of depth on the pyramid</param>
        /// <returns>the tile, containing functions to retrieve info</returns>
        public SBITile getTile(long iXPos, long iYPos, int iDepth)
        {
            int i = OPERATIONS;
            checkImageUsageSingleThreaded();

            int iMaxDepth = _images.Length;

            if (iDepth > iMaxDepth)
            {
                throw new Exception("getImagePart: Above maximum pyramid depth");
            }

            int iXMax = _images[iDepth].GetLength(0);
            int iYMax = _images[iDepth].GetLength(1);

            if (iXPos < 0)
            {
                iXPos += iXMax;
            }
            if (iYPos < 0)
            {
                iYPos += iYMax;
            }

            iXPos %= iXMax;
            iYPos %= iYMax;

            return _images[iDepth][iXPos, iYPos];
        }

        /// <summary>
        /// Generates an image pyramid
        /// </summary>
        public void generatePyramid()
        {
            //GC.Collect();
            SimpleImageProcessing.PyramidConstructor constructor = new SimpleImageProcessing.PyramidConstructor(_images);
            constructor.generatePyramid();
        }

        /// <summary>
        /// Gets the tile at position with overlap
        /// </summary>
        /// <param name="iXPos">X Position in Tile Count</param>
        /// <param name="iYPos">Y Position in Tile Count</param>
        /// <param name="iDepth">Level of depth</param>
        /// <param name="iOverlap">Overlap in Pixels</param>
        /// <returns></returns>
        public Image getTileImage(int iXPos, int iYPos, int iDepth, int iOverlap = 0)
        {
            Image output = _cutter.getImageTileWithOverlap(_images, iXPos, iYPos, iDepth, iOverlap, OPERATIONS);
            checkImageUsageSingleThreaded();

            return output;
        }

        /// <summary>
        /// Sets the tile image
        /// </summary>
        /// <param name="img">The image to set</param>
        /// <param name="iXPos">The X Position in Tile Count</param>
        /// <param name="iYPos">The Y Position in Tile Count</param>
        /// <param name="iDepth">The level of depth</param>
        /// <param name="blnResize">Resize image to fit?</param>
        public void setTileImage(Image img, long iXPos, long iYPos, int iDepth, bool blnResize = false)
        {
            try
            {
                int iCurrentWidth = img.Width;
                int iCurrentHeight = img.Height;

                int iXMax = _images[iDepth].GetLength(0);
                int iYMax = _images[iDepth].GetLength(1);

                if (iXPos >= iXMax)
                {
                    throw new Exception("SBImage: SetTile failed (xSize)");
                }
                if (iYPos >= iYMax)
                {
                    throw new Exception("SBImage: SetTile failed (ySize)");
                }

                if (iCurrentHeight != _iImageTileSize)
                {
                    if (blnResize)
                    {
                        using (Image resImage = SimpleImageProcessing.ImageCopier.RESIZE(img, _iImageTileSize, _iImageTileSize))//  .Resize(_iImageTileSize, _iImageTileSize, Emgu.CV.CvEnum.Inter.Cubic))
                        {
                            _images[iDepth][iXPos, iYPos].setImage(resImage);
                        }
                    }
                    else
                    {
                        using (Image resImage = new Bitmap(_iImageTileSize, _iImageTileSize))//new Image(_iImageTileSize, _iImageTileSize))
                        {
                            Graphics g = Graphics.FromImage(resImage);
                            g.Clear(Color.Black);

                            int iOverlap = iCurrentHeight - _iImageTileSize;
                            if ((iOverlap & 1) == 1)
                            {
                                throw new Exception("SBImage, setTile: Overlap is not a natural number");
                            }
                            iOverlap /= 2;

                            SimpleImageProcessing.ImageCopier.COPY(img, resImage, new Rectangle(iOverlap, iOverlap, _iImageTileSize, _iImageTileSize), new Rectangle(0, 0, _iImageTileSize, _iImageTileSize));
                            _images[iDepth][iXPos, iYPos].setImage(resImage);
                            
                            g.Dispose();
                        }
                    }
                }
                else
                {
                    //Image clone = img;// SimpleImageProcessing.ImageCopier.CLONE(img);// (Image)img.Clone();
                    _images[iDepth][iXPos, iYPos].setImage(img, true);
                    _images[iDepth][iXPos, iYPos].unload();
                }
            }
            finally
            {
            }
            _iOperationCount++;
            checkImageUsageSingleThreaded();
        }

        /// <summary>
        /// Gives back the resized part of the image
        /// </summary>
        /// <param name="iXStart">X Start position</param>
        /// <param name="iYStart">Y Start position</param>
        /// <param name="iWidth">Width of the cut image in pixels</param>
        /// <param name="iHeight">Height of the cut image in pixels</param>
        /// <param name="iTargetWidth">Width of the result image in pixels</param>
        /// <param name="iTargetHeight">Height of the result image in pixels</param>
        /// <returns></returns>
        public Image getImagePartResized(long iXStart, long iYStart, long iWidth, long iHeight, int iTargetWidth, int iTargetHeight)
        {
            Image output = _cutter.getImagePart(_images, iXStart, iYStart, iWidth, iHeight, iTargetWidth, iTargetHeight, OPERATIONS);
            return output;
        }

        /// <summary>
        /// Gives back the resized part of the image
        /// </summary>
        /// <param name="iXStart">X Start position</param>
        /// <param name="iYStart">Y Start position</param>
        /// <param name="iWidth">Width of the cut image in pixels</param>
        /// <param name="iHeight">Height of the cut image in pixels</param>
        /// <param name="dblZoomFactor">The wanted zoom factor. Zoom factor, not scaling factor; factor > 1 meaning the image gets smaller.</param>
        /// <returns></returns>
        public Image getImagePartResized(long iXStart, long iYStart, long iWidth, long iHeight, double dblZoomFactor)
        {
            if (iXStart > Width)
            {
                throw new Exception("getImagePartResized: Coordinates need to be inside the picture");
            }
            if (iYStart > Height)
            {
                throw new Exception("getImagePartResized: Coordinates need to be inside the picture");
            }
            if (iXStart + iWidth > Width)
            {
                throw new Exception("getImagePartResized: Coordinates need to be inside the picture");
            }
            if (iYStart + iHeight > Height)
            {
                throw new Exception("getImagePartResized: Coordinates need to be inside the picture");
            }

            int iTargetWidth = (int)((double)iWidth / dblZoomFactor);
            int iTargetHeight = (int)((double)iHeight / dblZoomFactor);
            Image output = _cutter.getImagePart(_images, iXStart, iYStart, iWidth, iHeight, iTargetWidth, iTargetHeight, OPERATIONS);
            return output;
        }

        /// <summary>
        /// Removes the image
        /// </summary>
        public void Dispose()
        {
            if (_images != null)
            {
                foreach (SBITile[,] part in _images)
                {
                    foreach (SBITile img in part)
                    {
                        if (img.Active)
                        {
                            img.unload();
                        }
                    }
                }
                _images = null;
            }
            if (_zfmgmt != null)
            {
                _zfmgmt.Dispose();
            }

            //GC.Collect();
        }

        /// <summary>
        /// The destructor
        /// </summary>
        ~SBImage()
        {
            Dispose();
        }

        /// <summary>
        /// Overall width of the image in pixels
        /// </summary>
        public long Width
        {
            get
            {
                return _iPixelWidth == 0 ? _iWidth : _iPixelWidth;
            }
        }

        /// <summary>
        /// Overall height of the image in pixels
        /// </summary>
        public long Height
        {
            get
            {
                return _iPixelHeight == 0 ? _iHeight : _iPixelHeight;
            }
        }

        /// <summary>
        /// Tile count in X direction
        /// </summary>
        public long XSize
        {
            get
            {
                return _iXSize;
            }
        }

        /// <summary>
        /// Tile count in Y direction
        /// </summary>
        public long YSize
        {
            get
            {
                return _iYSize;
            }
        }

        /// <summary>
        /// Depth of the pyramid image
        /// </summary>
        public int Depth
        {
            get
            {
                return _iDepth;
            }
        }

        /// <summary>
        /// Size of the tiles in pixels
        /// </summary>
        public int Tilesize
        {
            get
            {
                return _iImageTileSize;
            }
        }

        /// <summary>
        /// Returns the scaling of the image in µm/pixel X
        /// </summary>
        public double XSCALING
        {
            get
            {
                return _dblXScaling;
            }
        }

        /// <summary>
        /// Returns the scaling of the image in µm/pixel Y
        /// </summary>
        public double YSCALING
        {
            get
            {
                return _dblYScaling;
            }
        }

        /// <summary>
        /// Returns a value compareable to a stage position top left (X Coordinate)
        /// </summary>
        public double XOFFSET
        {
            get
            {
                return _dblXOffset;
            }
        }

        /// <summary>
        /// Returns a value compareable to a stage position top left (Y Coordinate)
        /// </summary>
        public double YOFFSET
        {
            get
            {
                return _dblYOffset;
            }
        }

        /// <summary>
        /// Is it in "multithreaded mode"
        /// </summary>
        public bool MULTITHREADED
        {
            get
            {
                return _blnMultithreadingMode;
            }
        }

        public string FOLDERNAME
        {
            get
            {
                return _strTarget;
            }
        }
    }
}
