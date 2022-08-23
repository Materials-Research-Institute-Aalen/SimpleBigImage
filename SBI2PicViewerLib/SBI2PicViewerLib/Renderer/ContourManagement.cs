using Cornerstones.Poly2DMath;
using SBI2PicViewerLib.Geom;
using SBI2PicViewerLib.Renderer;
using SimpleBigImage2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SBI2PicViewerLib.Renderer
{
    class ContourManagement
    {
        //ToDo: Make it a setting
        public static int MINIMUM_DRAWING_SIZE = 5;
        public static int MINIMUM_DRAWING_COUNT = 10000;
        public static int MAXIMUM_DRAWING_COUNT = 20000;

        List<DrawPolyline2D> _polylines;
        List<DrawPolyline2D> _polyToRender;
        List<ContourTileListElement> _elements;
        private Contours _contours;
        private Color _defaultColor;

        private int _iTileSize;

        private SBImage _img;

        /// <summary>
        /// Creates a contour management - meaning that contours are added to Tiles, and rendered, when they are rendered.
        /// </summary>
        /// <param name="Tiles"></param>
        /// <param name="Contours"></param>
        public ContourManagement(Contours contours, Color defaultColor, SBImage img, int iTileSize)
        {
            _iTileSize = iTileSize;
            _img = img;

            _contours = contours;
            _defaultColor = defaultColor;

            _polylines = new List<DrawPolyline2D>();
            _polyToRender = new List<DrawPolyline2D>();
            _elements = new List<ContourTileListElement>();

            generateTileListElements();
        }

        /// <summary>
        /// renders the system
        /// </summary>
        /// <param name="cam">the camera to render to</param>
        /// <param name="currently_rendering">the elements currently visible</param>
        public void render(Camera cam, List<long[]> currently_rendering)
        {
            getElementsToRender(cam, currently_rendering);
            foreach (DrawPolyline2D poly in _polyToRender)
            {
                poly.render(cam.getZoom());
            }
        }

        /// <summary>
        /// Updates the internal list of all Elements to render - sped up
        /// </summary>
        /// <param name="cam">the camera to render to</param>
        /// <param name="currently_rendering">the elements currently visible</param>
        private void getElementsToRender(Camera cam, List<long[]> currently_rendering)
        {
            double zoom = cam.getZoom();
            _polyToRender = new List<DrawPolyline2D>();
            int iTileCount = currently_rendering.Count();
            foreach (ContourTileListElement elm in _elements)
            {
                if (elm.toRender(currently_rendering))
                {
                    int iCounter = 0;
                    foreach (DrawPolyline2D poly in elm.POLYLINES)
                    {
                        double width = poly.CONTOUR.GET_RECT.right - poly.CONTOUR.GET_RECT.left;
                        double height = poly.CONTOUR.GET_RECT.bottom - poly.CONTOUR.GET_RECT.top;
                        double size = width > height ? width : height;

                        size *= zoom;
                        iCounter++;
                        if ((size < MINIMUM_DRAWING_SIZE && iCounter > MINIMUM_DRAWING_COUNT) || iCounter > (double)MAXIMUM_DRAWING_COUNT / (double)iTileCount)
                        {
                            break;
                        }
                        _polyToRender.Add(poly);
                    }
                }
            }
        }

        /// <summary>
        /// Collides Polygons and gets the melted together result
        /// </summary>
        /// <param name="source">The polygon you want to melt into the system</param>
        /// <param name="result">the melted result</param>
        /// <param name="c">the color the result should be</param>
        /// <returns>everything it collides with</returns>
        public List<DrawPolyline2D> getCollidingPolylines(List<IntPoint> source, out List<DrawPolyline2D> result, Color c)
        {
            List<DrawPolyline2D> collidesWith = new List<DrawPolyline2D>();

            List<Contour> collidingExistingContours = new List<Contour>();
            List<Contour> newlyGeneratedContours = new List<Contour>();

            foreach (DrawPolyline2D input in _polyToRender)
            {
                if (input.CONTOUR.collidesWith(source))
                {
                    collidesWith.Add(input);
                    collidingExistingContours.Add(input.CONTOUR);
                }
            }

            result = new List<DrawPolyline2D>();

            newlyGeneratedContours.Add(new Contour(source));

            Contours existingContours = new Contours(collidingExistingContours);
            Contours newContours = new Contours(newlyGeneratedContours);

            //toAddTo = toAddTo + toBeAdded;

            foreach (Contour cont in newContours.LIST)
            {
                result.Add(new DrawPolyline2D(cont, c));
            }

            return collidesWith;
        }

        /// <summary>
        /// changes one contour, as shown on all tiles
        /// </summary>
        /// <param name="polylineToChange">Polyline to remove</param>
        /// <param name="newPolyline">Polyline to add</param>
        public void changeContour(List<DrawPolyline2D> polylineToChange, List<DrawPolyline2D> newPolyline)
        {
            removePolylines(polylineToChange);
            attachPolylines(newPolyline);
        }

        /// <summary>
        /// changes one contour, as shown on all tiles
        /// </summary>
        /// <param name="polylineToChange">Polyline to remove</param>
        /// <param name="newPolyline">Polyline to add</param>
        public void changeContour(DrawPolyline2D polylineToChange, DrawPolyline2D newPolyline)
        {
            List<DrawPolyline2D> toChange = new List<DrawPolyline2D>();
            toChange.Add(polylineToChange);
            List<DrawPolyline2D> toAdd = new List<DrawPolyline2D>();
            toAdd.Add(newPolyline);

            removePolylines(toChange);
            attachPolylines(toAdd);
        }
        
        /// <summary>
        /// puts the shapes onto tiles, currently a naive algorithm to find out if a contour should be on a tile
        /// </summary>
        public void update()
        {
            Contour[] mtInputArray = _contours.LIST.ToArray();
            DrawPolyline2D[] mtArray = new DrawPolyline2D[mtInputArray.Length];
            Parallel.For(0, mtInputArray.Length, i =>
            {
                mtArray[i] = new DrawPolyline2D(mtInputArray[i], _defaultColor);
            });

            _polylines = mtArray.ToList();

            generateTileListElements();

            naiveContoursToTiles();
        }

        /// <summary>
        /// Attaches new contours to the tiles
        /// </summary>
        /// <param name="polylines">A List of Polylines to add</param>
        public void attachPolylines(List<DrawPolyline2D> polylines)
        {
            _polylines.AddRange(polylines);
            naiveAttachContoursToTiles(polylines);
        }

        /// <summary>
        /// Attaches new contours to the tiles
        /// </summary>
        /// <param name="polylines">Everything you want to remove, my friend</param>
        public void removePolylines(List<DrawPolyline2D> input)
        {
            foreach (ContourTileListElement elm in _elements)
            {
                foreach (DrawPolyline2D contour in input)
                {
                    elm.POLYLINES.Remove(contour);
                }
            }
            foreach (DrawPolyline2D contour in input)
            {
                _polylines.Remove(contour);
            }
        }

        /// <summary>
        /// Gives you a list of Polylines at the point X, Y, with the smallest (and mostly most relevant) contour being last.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public List<DrawPolyline2D> getPolylinesAt(long x, long y)
        {
            List<DrawPolyline2D> output = new List<DrawPolyline2D>();
            if (_polyToRender != null)
            {
                foreach (DrawPolyline2D poly in _polyToRender)
                {
                    if (poly.isPointInside(x, y))
                    {
                        output.Add(poly);
                    }
                }
                return output;
            }
            return new List<DrawPolyline2D>();
        }

        public List<DrawPolyline2D> getAllPolylines()
        {
            return _polylines;
        }

        /// <summary>
        /// Gives you a list of Contours at the point X, Y, with the smallest (and mostly most relevant) contour being last.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public List<Contour> getContoursAt(long x, long y)
        {
            List<Contour> output = new List<Contour>();
            if (_polyToRender != null)
            {
                foreach (DrawPolyline2D poly in _polyToRender)
                {
                    if (poly.isPointInside(x, y))
                    {
                        output.Add(poly.CONTOUR);
                    }
                }
                return output;
            }
            return new List<Contour>();
        }

        /// <summary>
        /// Initializes the Tile List
        /// </summary>
        private void generateTileListElements()
        {
            _elements = new List<ContourTileListElement>();
            int iDepth = _img.Depth;
            for (int depth = 0; depth < iDepth; depth++)
            {
                double dblSize = ((double)_iTileSize * Math.Pow(2, depth));
                long lXSize = _img.getXSize(depth);
                long lYSize = _img.getYSize(depth);

                for (long x = 0; x < lXSize; x++)
                {
                    for (long y = 0; y < lYSize; y++)
                    {
                        ContourTileListElement elm = new ContourTileListElement(x, y, depth, x * dblSize, y * dblSize, dblSize);
                        _elements.Add(elm);
                    }
                }
            }
        }

        /// <summary>
        /// Naively puts the contour onto a tile
        /// </summary>
        /// <param name="input"></param>
        private void naiveAttachContoursToTiles(List<DrawPolyline2D> input)
        {
            foreach (ContourTileListElement elm in _elements)
            {
                bool blnResort = false;
                Mutex m = new Mutex();

                Parallel.ForEach(input, (poly) =>
                //foreach (DrawPolyline2D poly in input)
                {
                    if (poly.doesOverlap(elm.ROI))
                    {
                        m.WaitOne();
                        attachContourToTile(elm, poly);
                        blnResort = true;
                        m.ReleaseMutex();
                    }
                });
                //Remove empty elements
                while (elm.POLYLINES.Contains(null))
                {
                    elm.POLYLINES.Remove(null);
                }
                if (blnResort)
                {
                    //Sort for later; Big elements first
                    elm.POLYLINES.Sort(delegate(DrawPolyline2D one, DrawPolyline2D two)
                        {
                            double sizeOne = one.CONTOUR.CIRCUMFERENCE_FOR_SORTING;
                            double sizeTwo = two.CONTOUR.CIRCUMFERENCE_FOR_SORTING;
                            //Biggest first
                            return sizeTwo.CompareTo(sizeOne);
                        });
                }
            }
        }

        /// <summary>
        /// Naively puts the contour onto a tile
        /// </summary>
        private void naiveContoursToTiles()
        {
            foreach (ContourTileListElement elm in _elements)
            {
                elm.POLYLINES = null;
                elm.POLYLINES = new List<DrawPolyline2D>();
            }

            foreach (ContourTileListElement elm in _elements)
            {
                Mutex m = new Mutex();
                Parallel.ForEach(_polylines, poly => 
                //foreach (DrawPolyline2D poly in _polylines)
                {
                    if (poly.doesOverlap(elm.ROI))
                    {
                        m.WaitOne();
                        attachContourToTile(elm, poly);
                        m.ReleaseMutex();
                    }
                });
                //Remove empty elements
                while (elm.POLYLINES.Contains(null))
                {
                    elm.POLYLINES.Remove(null);
                }
                //Sort for later; Big elements first
                elm.POLYLINES.Sort(delegate(DrawPolyline2D one, DrawPolyline2D two)
                {
                    double width;
                    double height;

                    width = one.CONTOUR.GET_RECT.right - one.CONTOUR.GET_RECT.left;
                    height = one.CONTOUR.GET_RECT.bottom - one.CONTOUR.GET_RECT.top;
                    double sizeOne = width > height ? width : height;

                    width = two.CONTOUR.GET_RECT.right - two.CONTOUR.GET_RECT.left;
                    height = two.CONTOUR.GET_RECT.bottom - two.CONTOUR.GET_RECT.top;
                    double sizeTwo = width > height ? width : height;

                    //double sizeOne = one.CONTOUR.CIRCUMFERENCE;
                    //double sizeTwo = two.CONTOUR.CIRCUMFERENCE;
                    //Biggest first
                    return sizeTwo.CompareTo(sizeOne);
                });
            }
        }

        private TextureElement[] getRelevantTilesFromContour(DrawPolyline2D item)
        {
            return null;
        }

        private void attachContourToTile(ContourTileListElement input, DrawPolyline2D polyline)
        {
            if (polyline != null)
            {
                input.POLYLINES.Add(polyline);
            }
        }

        private void removeContourFromTile(ContourTileListElement input, DrawPolyline2D polyline)
        {
            input.POLYLINES.Remove(polyline);
        }
    }
}
