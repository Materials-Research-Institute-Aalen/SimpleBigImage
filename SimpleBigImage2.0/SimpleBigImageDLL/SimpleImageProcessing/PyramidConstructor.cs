using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBigImage2;
using System.Drawing;

namespace SimpleImageProcessing
{
    class PyramidConstructor
    {
        SBITile[][,] _tiles;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="tiles">All the tiles of an SBI, but only the low level [0] is important, the rest will be overwritten</param>
        public PyramidConstructor(SBITile[][,] tiles)
        {
            _tiles = tiles;
        }

        /// <summary>
        /// "Delivers" tiles to a higher thier, meaning they will be resized to 1/2 the size and put into a "bigger" tile as a part
        /// </summary>
        /// <param name="tiles">The 1-4 input tiles as 2 dimensional element</param>
        /// <param name="output">the one output tile</param>
        private void deliverTileFromTo(SBITile[][] tiles, SBITile output)
        {
            int iTileSize = tiles[0][0].TILESIZE;
            int iTileSizeHalf = iTileSize / 2;

            System.Drawing.Image result = new Bitmap(iTileSize, iTileSize);
            
            for (int x = 0; x < tiles.Length; x++)
            {
                for (int y = 0; y < tiles[x].Length; y++)
                {
                    SBITile tile = tiles[x][y];

                    System.Drawing.Image tileImage = tile.getImage(0);

                    if (tileImage == null)
                    {
                        Bitmap bitmap = new Bitmap(iTileSize, iTileSize);
                        tile.setImage(bitmap);
                        tileImage = bitmap;
                    }
                    using (System.Drawing.Image resized = ImageCopier.RESIZE(tileImage, iTileSizeHalf, iTileSizeHalf))
                    {
                        Rectangle src = new Rectangle(0, 0, iTileSizeHalf, iTileSizeHalf);
                        Rectangle dst = new Rectangle(x * iTileSizeHalf, y * iTileSizeHalf, iTileSizeHalf, iTileSizeHalf);

                        result = ImageCopier.COPY(resized, result, src, dst);
                    }

                    tileImage.Dispose();
                    tile.unload();
                }
            }
            
            output.setImage(result, true);
            result.Dispose();
            
            output.save();
            output.unload();
            //GC.Collect(1);
            //GC.Collect(2);
        }

        /// <summary>
        /// Goes from level to level + 1
        /// </summary>
        /// <param name="lowThier">The lower level</param>
        /// <param name="highThier">The level the tiles "get delivered to"</param>
        private void deliverLevelFromTo(SBITile[][] lowThier, SBITile[][] highThier)
        {
            //From destination to source!
            int iLTXMax = lowThier.Length;
            int iLTYMax = lowThier[0].Length;

            int iHTXMax = highThier.Length;
            int iHTYMax = highThier[0].Length;

            //Parallel.For(0, iHTXMax, x =>
            for (int x = 0; x < iHTXMax; x++)
            {
                int iLTXStart = x * 2;
                int iLTXEnd = x * 2 + 1;

                iLTXEnd = iLTXEnd > iLTXMax - 1 ? iLTXMax - 1 : iLTXEnd;

                if (iLTXStart > iLTXMax)
                {
                    throw new Exception("Failure in Algorithm to calculate Pyramid 1");
                }

                //Parallel.For(0, iHTYMax, y =>
                for (int y = 0; y < iHTYMax; y++)
                {
                    bool blnDoItAgain = true;
                    while (blnDoItAgain)
                    {
                        int iLTYStart = y * 2;
                        int iLTYEnd = y * 2 + 1;

                        iLTYEnd = iLTYEnd > iLTYMax - 1 ? iLTYMax - 1 : iLTYEnd;

                        if (iLTYStart > iLTYMax)
                        {
                            throw new Exception("Failure in Algorithm to calculate Pyramid 2");
                        }

                        int iXSize = iLTXEnd - iLTXStart + 1;
                        int iYSize = iLTYEnd - iLTYStart + 1;

                        SBITile[][] toCalc = new SBITile[iXSize][];
                        for (int i = 0; i < iXSize; i++)
                        {
                            toCalc[i] = new SBITile[iYSize];
                            for (int j = 0; j < iYSize; j++)
                            {
                                toCalc[i][j] = lowThier[i + iLTXStart][j + iLTYStart];
                            }
                        }
                        deliverTileFromTo(toCalc, highThier[x][y]);
                        blnDoItAgain = false;
                    }

                }//);
            }
        }

        /// <summary>
        /// Gets a complete thier/level
        /// </summary>
        /// <param name="iDepth">The current thier to calculate</param>
        /// <returns>All tiles of that thier</returns>
        private SBITile[][] calcThier(int iDepth)
        {
            List<List<SBITile>> thier = new List<List<SBITile>>();

            int iXSize = _tiles[iDepth].GetLength(0);
            int iYSize = _tiles[iDepth].GetLength(1);

            for (int x = 0; x < iXSize; x++)
            {
                if (_tiles[iDepth][x, 0] != null)
                {
                    List<SBITile> data = new List<SBITile>();
                    for (int y = 0; y < iYSize; y++)
                    {
                        if (_tiles[iDepth][x, y] != null)
                        {
                            data.Add(_tiles[iDepth][x, y]);
                        }
                    }
                    thier.Add(data);
                }
            }

            SBITile[][] output;
            output = new SBITile[thier.Count][];
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = thier[i].ToArray();
            }

            return output;
        }

        /// <summary>
        /// Generates the higherup thier from the given thier
        /// </summary>
        /// <param name="iDepth">the depth to work with</param>
        private void generateUpperThierFromThier(int iDepth)
        {
            if (_tiles.Length < iDepth)
            {
                throw new Exception("Failure in Algorithm Pyramid: Depth");
            }

            int iLowThier = iDepth;
            int iHighThier = iDepth + 1;

            SBITile[][] lowThier = calcThier(iLowThier);
            SBITile[][] highThier = calcThier(iHighThier);

            deliverLevelFromTo(lowThier, highThier);
            
        }

        /// <summary>
        /// Does the whole pyramid, hard.
        /// </summary>
        public void generatePyramid()
        {
            int iDepth = _tiles.Length - 1;

            for (int i = 0; i < iDepth; i++)
            {
                generateUpperThierFromThier(i);
            }
        }
    }
}
