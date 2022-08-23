using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

//using Emgu.CV;
//using Emgu.CV.Structure;

namespace SimpleBigImage2
{
    /// <summary>
    /// The cutter class is an internally used class to export Image Cutting and Exporting functionality of the SBImage class.
    /// </summary>
    public class SBICutter//<ColorType> where ColorType : struct, IColor
    {
        int _iImageTileSize = 0;
        int _iMaxDepth = 0;

        /// <summary>
        /// Constructor for the SBICutter, created by the SBImage class
        /// </summary>
        /// <param name="iImageTileSize">The tile size of the image</param>
        /// <param name="iMaxDepth">The maximum depth of the image</param>
        public SBICutter(int iImageTileSize, int iMaxDepth)
        {
            _iImageTileSize = iImageTileSize;
            _iMaxDepth = iMaxDepth;
        }


        ////////////////////////////////////////////////////////

        private SBITile[,] cutOut(SBITile[][,] images, long xStart, long yStart, long iWidth, long iHeight, int iDepth)
        {

            long iImageListXStart = 0, iImageListYStart = 0;
            long iImageListXEnd = 0, iImageListYEnd = 0;

            if (iDepth > _iMaxDepth)
            {
                throw new Exception("getImagePart: Above maximum pyramid depth");
            }

            if (xStart < 0 || yStart < 0)
            {
                throw new Exception("getImagePart: Failed to set right start coordinates (negative values found)");
            }
            if (iWidth < 0)
            {
                throw new Exception("getImagePart: Width below zero");
            }
            if (iHeight < 0)
            {
                throw new Exception("getImagePart: Height below zero");
            }

            long iMaxWidth = images[0].GetLength(0) * _iImageTileSize;
            long iMaxHeight = images[0].GetLength(1) * _iImageTileSize;


            if (xStart > iMaxWidth - 1)
            {
                throw new Exception("getImagePart: Start coordinates invalid");
            }
            if (yStart > iMaxHeight - 1)
            {
                throw new Exception("getImagePart: Start coordinates invalid");
            }

            if (iWidth + xStart > iMaxWidth)
            {
                iWidth = iMaxWidth - xStart;
            }
            if (iHeight + yStart > iMaxHeight)
            {
                iHeight = iMaxHeight - yStart;
            }

            long iCoordinatesXStart = xStart;
            long iCoordinatesYStart = yStart;
            long iCoordinatesXEnd = xStart + iWidth;
            long iCoordinatesYEnd = yStart + iHeight;

            long iMaxListXSize = images[iDepth].GetLength(0);
            long iMaxListYSize = images[iDepth].GetLength(1);

            iImageListXStart = xStart / _iImageTileSize;
            iImageListYStart = yStart / _iImageTileSize;

            iImageListXEnd = (iCoordinatesXEnd % _iImageTileSize == 0 ? iCoordinatesXEnd / _iImageTileSize : (iCoordinatesXEnd / _iImageTileSize) + 1);
            iImageListYEnd = (iCoordinatesYEnd % _iImageTileSize == 0 ? iCoordinatesYEnd / _iImageTileSize : (iCoordinatesYEnd / _iImageTileSize) + 1);

            iImageListXEnd = (iImageListXEnd > iMaxListXSize ? iMaxListXSize : iImageListXEnd);
            iImageListYEnd = (iImageListYEnd > iMaxListYSize ? iMaxListYSize : iImageListYEnd);

            long iListXSize = iImageListXEnd - iImageListXStart;
            long iListYSize = iImageListYEnd - iImageListYStart;

            if (iListXSize + iImageListXStart > iMaxListXSize)
            {
                throw new Exception("Failed to calculate correct X image size");
            }
            if (iListYSize + iImageListYStart > iImageListYEnd)
            {
                throw new Exception("Failed to calculate correct Y image size");
            }

            SBITile[,] list = new SBITile[iListXSize, iListYSize];

            for (int i = 0; i < iListXSize; i++)
            {
                for (int j = 0; j < iListYSize; j++)
                {
                    list[i, j] = images[iDepth][iImageListXStart + i, iImageListYStart + j];
                }
            }

            return list;
        }

        /// <summary>
        /// A Complex function which tells you where, and how much, of the current tile, will be used to be drawn on, or cut out
        /// </summary>
        /// <param name="i">x tilepos in internal array</param>
        /// <param name="j">y tilepos in internal array</param>
        /// <param name="iListXSize">The arraysize in x direction</param>
        /// <param name="iListYSize">The arraysize in y direction</param>
        /// <param name="startX">x Position inside the small (non-SBI) image</param>
        /// <param name="startY">y Position inside the small (non-SBI) image</param>
        /// <param name="iExpectedWidth">The expected width of the image (gets resized if SBI is too small)</param>
        /// <param name="iExpectedHeight">The expected height of the image (gets resized if SBI is too small)</param>
        /// <param name="iTileWidth">The width of tiles (normally 1024)</param>
        /// <param name="iTileHeight">The height of tiles (normally 1024)</param>
        /// <param name="iBigImageXOffset">x Position inside the big (SBI) image</param>
        /// <param name="iBigImageYOffset">y Position inside the big (SBI) image</param>
        /// <param name="iSmallImageXPos">unused</param>
        /// <param name="iSmallImageYPos">unused</param>
        /// <param name="iSmallImageWidth">unused</param>
        /// <param name="iSmallImageHeight">unused</param>
        /// <param name="iBigImageXPos">unused</param>
        /// <param name="iBigImageYPos">unused</param>
        private static void getTileCopyOffsets(int i, int j, int iListXSize, int iListYSize, long startX, long startY, long iExpectedWidth, long iExpectedHeight, int iTileWidth, int iTileHeight, ref int iBigImageXOffset, ref int iBigImageYOffset, out int iSmallImageXPos, out int iSmallImageYPos, out int iSmallImageWidth, out int iSmallImageHeight, out int iBigImageXPos, out int iBigImageYPos)
        {
            int iCurrentXPos = i * iTileWidth;
            int iCurrentYPos = j * iTileHeight;
            iSmallImageXPos = 0;
            iSmallImageYPos = 0;
            iSmallImageWidth = iTileWidth;
            iSmallImageHeight = iTileHeight;
            iBigImageXPos = 0;
            iBigImageYPos = 0;

            iBigImageXPos = iCurrentXPos - iBigImageXOffset;
            iBigImageYPos = iCurrentYPos - iBigImageYOffset;

            if (i == 0)
            {
                iSmallImageXPos = (int)(startX % iTileWidth);
                iSmallImageWidth = iTileWidth - iSmallImageXPos;
                iBigImageXOffset = iSmallImageXPos;
                iBigImageXPos = 0;
                if (iSmallImageWidth <= 0)
                {
                    throw new Exception("putTogether: Error on Width (equals zero)");
                }
            }
            else if (i == iListXSize - 1)
            {
                iSmallImageWidth = (int)((startX + iExpectedWidth) % iTileWidth);
                if (iSmallImageWidth <= 0)
                {
                    long iListStart = startX / iTileWidth;
                    if (startX + iExpectedWidth == (iListXSize+iListStart) * iTileWidth)
                    {
                        iSmallImageWidth = iTileWidth;
                    }
                    else
                    {
                        throw new Exception("putTogether: Error on Width (equals zero; i == iListXSize - 1)");
                    }
                }
            }

            if (j == 0)
            {
                iSmallImageYPos = (int)(startY % iTileHeight);
                iSmallImageHeight = iTileHeight - iSmallImageYPos;
                iBigImageYOffset = iSmallImageYPos;
                iBigImageYPos = 0;
                if (iSmallImageHeight <= 0)
                {
                    throw new Exception("putTogether: Error on Height (equals zero)");
                }
            }
            else if (j == iListYSize - 1)
            {
                iSmallImageHeight = (int)((startY + iExpectedHeight) % iTileHeight);
                if (iSmallImageHeight <= 0)
                {
                    long iTileOffset = startY / iTileHeight;
                    if (startY + iExpectedHeight == (iListYSize +  iTileOffset)* iTileHeight)
                    {
                        iSmallImageHeight = iTileHeight;
                    }
                    else
                    {
                        throw new Exception("putTogether: Error on Height (equals zero; j == iListYSize - 1)");
                    }
                }
            }

            if (iBigImageXPos + iSmallImageWidth > iExpectedWidth)
            {
                iSmallImageWidth = (int)(iExpectedWidth - iBigImageXPos);
                if (iSmallImageWidth <= 0)
                {
                    throw new Exception("putTogether: Tile Cutout Width smaller or equal zero");
                }
            }
            if (iBigImageYPos + iSmallImageHeight > iExpectedHeight)
            {
                iSmallImageHeight = (int)(iExpectedHeight - iBigImageYPos);
                if (iSmallImageHeight <= 0)
                {
                    throw new Exception("putTogether: Tile Cutout Width smaller or equal zero");
                }
            }
        }

        ////////////////////////////////////////////////////////

        /// <summary>
        /// Returns the depthlayer (as integer) from the current zoom factor (as double)
        /// </summary>
        /// <param name="dblZoom">Wanted zoom factor</param>
        /// <param name="iMaxDepth">Maximum depth</param>
        /// <returns>The depth as integer</returns>
        public static int getDepthFromZoom(double dblZoom, int iMaxDepth)
        {
            int iDepth = (int)(1.0f / dblZoom / 2);
            if (iDepth > iMaxDepth)
            {
                iDepth = iMaxDepth;
            }

            return iDepth;
        }

        /// <summary>
        /// Returns the scaling factor of the given depth
        /// </summary>
        /// <param name="iDepth">The depth layer</param>
        /// <returns>The scaling factor for low thier pixels</returns>
        public static double getDepthScalingFactor(int iDepth)
        {
            double dblDepthFactor = 1 << iDepth;
            return 1 / dblDepthFactor;
        }

        /// <summary>
        /// Gives back a part of the tile image as a complete single image
        /// </summary>
        /// <param name="images">The tiles</param>
        /// <param name="xStart">Start coordinate x direction</param>
        /// <param name="yStart">Start coordinate y direction</param>
        /// <param name="dblWidth">Width of the wanted image part</param>
        /// <param name="dblHeight">Height of the wanted image part</param>
        /// <param name="iResultWidth">Width of the result image</param>
        /// <param name="iResultHeight">Height of the result image</param>
        /// <param name="iOperationID">The ID of the current operation</param>
        /// <returns>The cut out image</returns>
        public Image getImagePart(SBITile[][,] images, double xStart, double yStart, double dblWidth, double dblHeight, int iResultWidth, int iResultHeight, int iOperationID)
        {
            //Schritt 1: Tiefe ausrechnen

            if (iResultHeight < 1)
            {
                iResultHeight = 1;
            }
            if (iResultWidth < 1)
            {
                iResultWidth = 1;
            }

            int iDepth = 0;
            double dblScalingFactor = 1;
            double dblWFactor = (double)dblWidth / (double)iResultWidth;
            double dblHFactor = (double)dblHeight / (double)iResultHeight;

            double dblZoom = 1;

            if (dblHFactor < dblWFactor)
            {
                dblZoom = 1.0 / dblHFactor;
            }
            else
            {
                dblZoom = 1.0 / dblWFactor;
            }

            iDepth = getDepthFromZoom(dblZoom, _iMaxDepth);
            dblScalingFactor = getDepthScalingFactor(iDepth);

            xStart *= dblScalingFactor;// (int)((double)xStart / iDepthFactor + 0.5);
            yStart *= dblScalingFactor;// (int)((double)yStart / iDepthFactor + 0.5);
            dblWidth *= dblScalingFactor;// (int)((double)iWidth / iDepthFactor + 0.5);
            dblHeight *= dblScalingFactor;// (int)((double)iHeight / iDepthFactor + 0.5);

            //Schritt 3: Bild ausschneiden
            Image output;
            using (Image toCalculate = getImagePart(images, (int)xStart, (int)yStart, (int)dblWidth, (int)dblHeight, iDepth, iOperationID))
            {
                output = SimpleImageProcessing.ImageCopier.RESIZE(toCalculate, iResultWidth, iResultHeight);
                //output = toCalculate;//.Resize(iResultWidth, iResultHeight);
            }
            return output;
        }
        
        /// <summary>
        /// Returns the wanted image tile with overlap
        /// </summary>
        /// <param name="images">The tiles of the image</param>
        /// <param name="iXPos">The tile position in X direction in tile counts</param>
        /// <param name="iYPos">The tile position in Y direction in tile counts</param>
        /// <param name="iDepth">The used depth layer</param>
        /// <param name="iOverlap">The wanted overlap</param>
        /// <param name="iOperationID">Current operation ID</param>
        /// <returns>The cut out image</returns>
        public Image getImageTileWithOverlap(SBITile[][,] images, int iXPos, int iYPos, int iDepth, int iOverlap, int iOperationID)
        {
            int iXMax = 0;
            int iYMax = 0;

            if (iDepth > _iMaxDepth)
            {
                throw new Exception("getImagePart: Above maximum pyramid depth");
            }

            iXMax = images[iDepth].GetLength(0);
            iYMax = images[iDepth].GetLength(1);

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

            if (iOverlap != 0)
            {
                int iXOffset = _iImageTileSize - iOverlap;
                int iYOffset = _iImageTileSize - iOverlap;
                int iWidth = _iImageTileSize + 2 * iOverlap;
                int iHeight = _iImageTileSize + 2 * iOverlap;

                Image[,] list = new Image[3, 3];
                Image output;

                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        int iCurrentXPos = iXPos + i;
                        int iCurrentYPos = iYPos + j;

                        if (iCurrentXPos < 0)
                        {
                            iCurrentXPos += iXMax;
                        }
                        if (iCurrentYPos < 0)
                        {
                            iCurrentYPos += iYMax;
                        }

                        iCurrentXPos %= iXMax;
                        iCurrentYPos %= iYMax;

                        list[i + 1, j + 1] = images[iDepth][iCurrentXPos, iCurrentYPos].getImage(iOperationID);
                    }
                }

                output = putTogether(list, _iImageTileSize, iXOffset, iYOffset, iWidth, iHeight);
                return output;
            }
            if (iXPos >= images[iDepth].GetLength(0))
            {
                return null;
            }
            if (iYPos >= images[iDepth].GetLength(1))
            {
                return null;
            }
            return images[iDepth][iXPos, iYPos].getImage(iOperationID);
        }

        /// <summary>
        /// Puts an Image (can be a Bitmap) and draws it onto the SBI
        /// </summary>
        /// <param name="images">The Tiles</param>
        /// <param name="img">The Image to draw onto the SBI</param>
        /// <param name="xStart">X Position</param>
        /// <param name="yStart">Y Position</param>
        /// <param name="iOperationID">The ID of the operation, for management reason, can be 0</param>
        public void setImagePart(SBITile[][,] images, Image img, long xStart, long yStart, int iOperationID)
        {
            int iWidth = img.Width;
            int iHeight = img.Height;

            SBITile[,] tiles = cutOut(images, xStart, yStart, iWidth, iHeight, 0);

            int iListXSize = tiles.GetLength(0);
            int iListYSize = tiles.GetLength(1);

            int iActualWidth = iListXSize * _iImageTileSize;
            int iActualHeight = iListYSize * _iImageTileSize;

            int iBigImageXOffset = 0;
            int iBigImageYOffset = 0;

            for (int x = 0; x < iListXSize; x++)
            {
                for (int y = 0; y < iListYSize; y++)
                {
                    int iSmallImageXPos = 0;
                    int iSmallImageYPos = 0;
                    int iSmallImageWidth = _iImageTileSize;
                    int iSmallImageHeight = _iImageTileSize;
                    int iBigImageXPos = 0;
                    int iBigImageYPos = 0;
                    getTileCopyOffsets(x, y, iListXSize, iListYSize, xStart, yStart, iWidth, iHeight, _iImageTileSize, _iImageTileSize, ref iBigImageXOffset, ref iBigImageYOffset, out iSmallImageXPos, out iSmallImageYPos, out iSmallImageWidth, out iSmallImageHeight, out iBigImageXPos, out iBigImageYPos);

                    if (x == 0 || y == 0 || x == (iListXSize - 1) || y == (iListYSize - 1))
                    {
                        if ((x == (iListXSize - 1))
                            && (xStart + iWidth > iActualWidth))
                        {
                            iSmallImageWidth = _iImageTileSize;
                        }
                        if ((y == (iListYSize - 1))
                            && (yStart + iHeight > iActualHeight))
                        {
                            iSmallImageHeight = _iImageTileSize;
                        }

                        Image tileimage = tiles[x, y].getImage(iOperationID);
                        SimpleImageProcessing.ImageCopier.COPY(img, tileimage, new Rectangle(iBigImageXPos, iBigImageYPos, iSmallImageWidth, iSmallImageHeight), new Rectangle(iSmallImageXPos, iSmallImageYPos, iSmallImageWidth, iSmallImageHeight));
                        tiles[x, y].setImage(tileimage);
                        tileimage.Dispose();
                    }
                    else
                    {
                        Image tileimage = SimpleImageProcessing.ImageCopier.CROP(img, iBigImageXPos, iBigImageYPos, iSmallImageWidth, iSmallImageHeight);
                        tiles[x, y].setImage(tileimage);
                        tileimage.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Gives back a part of the tile image as a complete single image
        /// </summary>
        /// <param name="images">The tiles</param>
        /// <param name="xStart">Start coordinate x direction</param>
        /// <param name="yStart">Start coordinate y direction</param>
        /// <param name="iWidth">Width of the wanted image part</param>
        /// <param name="iHeight">Height of the wanted image part</param>
        /// <param name="iDepth">The used depth layer</param>
        /// <param name="iOperationID">The current operation</param>
        /// <returns>The cut out image</returns>
        public Image getImagePart(SBITile[][,] images, long xStart, long yStart, long iWidth, long iHeight, int iDepth, int iOperationID)
        {
            SBITile[,] tiles = cutOut(images, xStart, yStart, iWidth, iHeight, iDepth);

            int iListXSize = tiles.GetLength(0);
            int iListYSize = tiles.GetLength(1);

            Image[,] list = new Image[iListXSize, iListYSize];
            Image output;

            for (int i = 0; i < iListXSize; i++)
            {
                for (int j = 0; j < iListYSize; j++)
                {
                    list[i, j] = tiles[i, j].getImage(iOperationID);
                }
            }

            int iXOffset = (int)(xStart % _iImageTileSize);
            int iYOffset = (int)(yStart % _iImageTileSize);

            output = putTogether(list, _iImageTileSize, iXOffset, iYOffset, iWidth, iHeight);

            return output;
        }
        
        /// <summary>
        /// For internal use only: Puts together the image list
        /// </summary>
        /// <param name="imageList">The tiles</param>
        /// <param name="startX">x start in pixels</param>
        /// <param name="startY">y start in pixels</param>
        /// <param name="iExpectedWidth">wanted width of the result image</param>
        /// <param name="iExpectedHeight">wanted height of the result image</param>
        /// <returns>the image</returns>
        public static Image putTogether(Image[,] imageList, int iTileSize, long startX, long startY, long iExpectedWidth, long iExpectedHeight)
        {
            int iActualWidth = 0;
            int iActualHeight = 0;

            int iListXSize = imageList.GetLength(0);
            int iListYSize = imageList.GetLength(1);

            int iTileWidth = iTileSize;
            int iTileHeight = iTileSize;

            iActualWidth = iListXSize * iTileSize;
            iActualHeight = iListYSize * iTileSize;
            
            if (startX < 0 || startY < 0) { throw new Exception("putTogether: Invalid starting point");}
            if (startX > iActualWidth){throw new Exception("putTogether: Image part missing / cannot be cut: X Direction");}
            if (startY > iActualHeight){throw new Exception("putTogether: Image part missing / cannot be cut: Y Direction");}
            if (iExpectedHeight == 0){throw new Exception("putTogether: Expected height equals zero");}
            if (iExpectedWidth == 0){throw new Exception("putTogether: Expected width equals zero");}

            if (startX + iExpectedWidth > iActualWidth)
            {
                iExpectedWidth = iActualWidth - startX;
            }
            if (startY + iExpectedHeight > iActualHeight)
            {
                iExpectedHeight = iActualHeight - startY;
            }

            Image output = new Bitmap((int)iExpectedWidth, (int)iExpectedHeight);// new Image(iExpectedWidth, iExpectedHeight);
            int iBigImageXOffset = 0;
            int iBigImageYOffset = 0;

            for (int i = 0; i < iListXSize; i++)
            {
                for (int j = 0; j < iListYSize; j++)
                {
                    if (imageList[i, j] != null)
                    {
                        int iSmallImageXPos = 0;
                        int iSmallImageYPos = 0;
                        int iSmallImageWidth = iTileWidth;
                        int iSmallImageHeight = iTileHeight;
                        int iBigImageXPos = 0;
                        int iBigImageYPos = 0;

                        getTileCopyOffsets(i, j, iListXSize, iListYSize, startX, startY, iExpectedWidth, iExpectedHeight, iTileWidth, iTileHeight, ref iBigImageXOffset, ref iBigImageYOffset, out iSmallImageXPos, out iSmallImageYPos, out iSmallImageWidth, out iSmallImageHeight, out iBigImageXPos, out iBigImageYPos);

                        output = SimpleImageProcessing.ImageCopier.COPY(imageList[i, j], output, new Rectangle(iSmallImageXPos, iSmallImageYPos, iSmallImageWidth, iSmallImageHeight), new Rectangle(iBigImageXPos, iBigImageYPos, iSmallImageWidth, iSmallImageHeight));
                    }
                }
            }

            for (int i = 0; i < iListXSize; i++)
            {
                for (int j = 0; j < iListYSize; j++)
                {
                    if (imageList[i, j] != null)
                    {
                        imageList[i, j].Dispose();
                    }
                }
            }
            return output;
        }
    }
}
