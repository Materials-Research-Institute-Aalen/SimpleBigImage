using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cornerstones.Poly2DMath
{
    public class Contours
    {
        List<Contour> _contourList;

        /////////////////////////////////////////
        //Internal functions

        private List<List<IntPoint>> actualContourIntPointListOfLists()
        {
            List<List<IntPoint>> output = new List<List<IntPoint>>();

            foreach (Contour c in _contourList)
            {
                if (!c.IS_HOLE)
                {
                    output.Add(c.CONTOUR);
                }
            }

            return output;
        }

        /// <summary>
        /// Takes a rectangle with a certain border. Everything that touches that border (or is inside a range) will be "outside"
        /// </summary>
        /// <param name="border">The border of the rectangle</param>
        /// <param name="rectangle">The rectangle which is used to determine "outside"</param>
        /// <returns></returns>
        public Contours[] split(int border, IntRect rectangle)
        {
            List<Contour> inside = new List<Contour>();
            List<Contour> outside = new List<Contour>();

            if (rectangle.left == rectangle.right)
            {
                rectangle = getRect();
            }

            rectangle.left += border;
            rectangle.top += border;
            rectangle.right -= border;
            rectangle.bottom -= border;

            foreach (Contour c in _contourList)
            {
                IntRect contRectangle = c.GET_RECT;
                if (contRectangle.left <= rectangle.left ||
                    contRectangle.right >= rectangle.right ||
                    contRectangle.top <= rectangle.top ||
                    contRectangle.bottom >= rectangle.bottom)
                {
                    inside.Add(c);
                }
                else
                {
                    outside.Add(c);
                }
            }

            return new Contours[] { new Contours(inside), new Contours(outside) };
        }

        /// <summary>
        /// Takes a rectangle with a certain border. Everything that touches that border (or is inside a range) will be "outside". This time, everything smaller than X will be ignored.
        /// </summary>
        /// <param name="border">The border distance from the rectangle</param>
        /// <param name="rectangle">The rectangle</param>
        /// <param name="iMaxSize">The maximum size of an element, everything smaller will be ignored</param>
        /// <param name="dblMaxArea">The maximum size of an element, everything smaller will be ignored</param>
        /// <returns></returns>
        public Contours[] split(int border, IntRect rectangle, int iMaxSize, double dblMaxArea = 0)
        {
            List<Contour> inside = new List<Contour>();
            List<Contour> outside = new List<Contour>();

            if (rectangle.left == rectangle.right)
            {
                rectangle = getRect();
            }

            rectangle.left += border;
            rectangle.top += border;
            rectangle.right -= border;
            rectangle.bottom -= border;

            foreach (Contour c in _contourList)
            {
                IntRect contRectangle = c.GET_RECT;
                long lngWidth = contRectangle.right - contRectangle.left;
                long lngHeight = contRectangle.bottom - contRectangle.top;

                bool blnAddInside = true;

                if (dblMaxArea > 0)
                {
                    if (dblMaxArea >= Math.Abs(c.AREA))
                    {
                        blnAddInside = false;
                    }
                }
                if (lngHeight >= iMaxSize || lngHeight >= iMaxSize)
                {
                    blnAddInside = false;
                }

                if (contRectangle.left <= rectangle.left ||
                    contRectangle.right >= rectangle.right ||
                    contRectangle.top <= rectangle.top ||
                    contRectangle.bottom >= rectangle.bottom)
                {
                    blnAddInside = false;
                }

                if (blnAddInside)
                {
                    inside.Add(c);
                }
                else
                {
                    outside.Add(c);
                }
            }

            return new Contours[] { new Contours(inside), new Contours(outside) };
        }

        /// <summary>
        /// Splits by size; Small elements will be in one result, big ones in the other. Only counts as small if "size" AND "area" is small
        /// </summary>
        /// <param name="iMaxSize">The size to split by</param>
        /// <param name="dblMaxArea">The area to split by. Can be 0 to be ignored.</param>
        /// <returns></returns>
        public Contours[] split(int iMaxSize, double dblMaxArea = 0)
        {
            List<Contour> inside = new List<Contour>();
            List<Contour> outside = new List<Contour>();

            foreach (Contour c in _contourList)
            {
                IntRect contRectangle = c.GET_RECT;
                long lngWidth = contRectangle.right - contRectangle.left;
                long lngHeight = contRectangle.bottom - contRectangle.top;

                bool blnAddInside = true;

                if (dblMaxArea > 0)
                {
                    if (dblMaxArea >= Math.Abs(c.AREA))
                    {
                        blnAddInside = false;
                    }
                }
                if (lngHeight >= iMaxSize && lngHeight >= iMaxSize)
                {
                    blnAddInside = false;
                }

                if (blnAddInside)
                {
                    inside.Add(c);
                }
                else
                {
                    outside.Add(c);
                }
            }

            return new Contours[] { new Contours(inside), new Contours(outside) };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private List<List<IntPoint>> actualHoleIntPointListOfLists()
        {
            List<List<IntPoint>> output = new List<List<IntPoint>>();

            foreach (Contour c in _contourList)
            {
                if (c.IS_HOLE)
                {
                    output.Add(c.CONTOUR);
                }
            }

            return output;
        }

        private List<Contour> actualShapes()
        {
            List<Contour> output = new List<Contour>();

            foreach (Contour c in _contourList)
            {
                if (!c.IS_HOLE)
                {
                    output.Add(c);
                }
            }

            return output;
        }

        private List<Contour> actualHoles()
        {
            List<Contour> output = new List<Contour>();

            foreach (Contour c in _contourList)
            {
                if (c.IS_HOLE)
                {
                    output.Add(c);
                }
            }

            return output;
        }

        /////////////////////////////////////////
        //Operations

        /// <summary>
        /// Gets contour nr. X
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>A contour</returns>
        public Contour this[int index]
        {
            get
            {
                return _contourList[index];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="holes"></param>
        /// <returns></returns>
        private Contours subtractHoles(Contours holes)
        {
            Contours a = this;

            Clipper clip = new Clipper();
            clip.AddPaths(a.actualContourIntPointListOfLists(), PolyType.ptSubject, Contour.CLOSED);
            clip.AddPaths(holes.actualHoleIntPointListOfLists(), PolyType.ptClip, Contour.CLOSED);

            PolyTree tree = new PolyTree();

            clip.Execute(ClipType.ctDifference, tree);

            List<Contour> outList = new List<Contour>();

            foreach (PolyNode n in tree.m_AllPolys)
            {
                Contour c = new Contour(n.Contour, n.IsHole);
                outList.Add(c);
            }

            return new Contours(outList);
        }

        /// <summary>
        /// An Operation between this contours and a given set of contours. Works either for contours or for holes; Not for both at the same time.
        /// </summary>
        /// <param name="b">The other set of contours</param>
        /// <param name="type">Is it a Union? Is it a Subtraction?</param>
        /// <param name="blnHoles">Is it thoe Holes or a Contours to be operated with?</param>
        /// <returns>Contours</returns>
        public Contours operation(Contours b, ClipType type, bool blnHoles)
        {
            Contours a = this;

            Clipper clip = new Clipper();
            if (!blnHoles)
            {
                clip.AddPaths(a.actualContourIntPointListOfLists(), PolyType.ptSubject, Contour.CLOSED);
                clip.AddPaths(b.actualContourIntPointListOfLists(), PolyType.ptClip, Contour.CLOSED);
            }
            else
            {
                clip.AddPaths(a.actualHoleIntPointListOfLists(), PolyType.ptSubject, Contour.CLOSED);
                clip.AddPaths(b.actualHoleIntPointListOfLists(), PolyType.ptClip, Contour.CLOSED);
            }

            PolyTree tree = new PolyTree();

            clip.Execute(type, tree);

            List<Contour> outList = new List<Contour>();

            foreach (PolyNode n in tree.m_AllPolys)
            {
                Contour c;
                if (blnHoles)
                {
                    c = new Contour(n.Contour, !n.IsHole);
                }
                else
                {
                    c = new Contour(n.Contour, n.IsHole);
                }
                outList.Add(c);
            }

            return new Contours(outList);
        }
        
        /// <summary>
        /// Gives back the rectangle that has all Contours/Holes inside
        /// </summary>
        /// <returns>A rectangle</returns>
        public IntRect getRect()
        {
            if (_contourList.Count < 1)
            {
                return new IntRect(0, 0, 0, 0);
            }
            IntRect output = new IntRect(_contourList[0].GET_RECT);

            foreach (Contour c in _contourList)
            {
                output.left = output.left < c.GET_RECT.left ? output.left : c.GET_RECT.left;
                output.right = output.right > c.GET_RECT.right ? output.right : c.GET_RECT.right;
                output.top = output.top < c.GET_RECT.top ? output.top : c.GET_RECT.top;
                output.bottom = output.bottom > c.GET_RECT.bottom ? output.bottom : c.GET_RECT.bottom;
            }

            return output;
        }
        
        /// <summary>
        /// A Union
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Contours operator +(Contours a, Contours b)
        {
            if (a == null || b == null)
            {
                return new Contours(new List<Contour>());
            }

            //ToDo: Test
            Contours shapes = a.operation(b, ClipType.ctUnion, false);
            
            ///*
            //Handling overlap
            //Contours resultingHoles = new Contours(shapes.actualHoles());
            //resultingHoles = a.operation(resultingHoles, ClipType.ctUnion, true);
            //Contours holes = resultingHoles.operation(b, ClipType.ctUnion, true);

            Contours holes = a.operation(b, ClipType.ctUnion, true);

            shapes.combineWith(holes);
            //*/
            return shapes;
        }

        /// <summary>
        /// A Difference
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Contours operator -(Contours a, Contours b)
        {
            if (a == null || b == null)
            {
                return new Contours(new List<Contour>());
            }

            Contours shapes = a.operation(b, ClipType.ctDifference, false);
            Contours resultingHoles = new Contours(shapes.actualHoles());
            resultingHoles = a.operation(resultingHoles, ClipType.ctUnion, true);
            Contours holes = resultingHoles.operation(b, ClipType.ctUnion, true);

            Contours result = shapes.subtractHoles(holes);

            return result;
        }

        /// <summary>
        /// The intersection
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Contours operator *(Contours a, Contours b)
        {
            if (a == null || b == null)
            {
                return new Contours(new List<Contour>());
            }

            Contours shapes = a.operation(b, ClipType.ctIntersection, false);
            Contours resultingHoles = new Contours(shapes.actualHoles());
            resultingHoles = a.operation(resultingHoles, ClipType.ctUnion, true);
            Contours holes = resultingHoles.operation(b, ClipType.ctUnion, true);

            Contours result = shapes.subtractHoles(holes);

            return result;
        }

        /// <summary>
        /// A fast Version of the Union, buggy
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Contours operator &(Contours a, Contour b)
        {
            if (a == null || b == null)
            {
                return new Contours(new List<Contour>());
            }
            Contours result = null;
            List<List<IntPoint>> outputShapes = new List<List<IntPoint>>();
            List<List<IntPoint>> outputHoles = new List<List<IntPoint>>();

            List<List<IntPoint>> toCut;
            List<IntPoint> cutter = b.CONTOUR;

            IntPoint[] cutterArr = cutter.ToArray();

            //Shapes
            toCut = new List<List<IntPoint>>();
            foreach (Contour c in a.CONTOURS.LIST)
            {
                toCut.Add(c.CONTOUR);
            }

            foreach (List<IntPoint> poly in toCut)
            {
                IntPoint[] toCutArr = poly.ToArray();
                if (toCutArr.Length > 2)
                {
                    if (!Sutherland.SutherlandHodgman.IsColinear(toCutArr))
                    {
                        try
                        {
                            IntPoint[] cutoutArr = Sutherland.SutherlandHodgman.GetIntersectedPolygon(toCutArr, cutterArr);
                            if (cutoutArr.Length > 2)
                            {
                                List<IntPoint> cutout = new List<IntPoint>(cutoutArr);
                                outputShapes.Add(cutout);
                            }
                        }
                        catch { };
                    }
                }
            }

            //Holes
            toCut = new List<List<IntPoint>>();
            foreach (Contour c in a.HOLES.LIST)
            {
                toCut.Add(c.CONTOUR);
            }
            foreach (List<IntPoint> poly in toCut)
            {
                IntPoint[] toCutArr = poly.ToArray();
                if (toCutArr.Length > 2)
                {
                    try
                    {
                        if (!Sutherland.SutherlandHodgman.IsColinear(toCutArr))
                        {
                            IntPoint[] cutoutArr = Sutherland.SutherlandHodgman.GetIntersectedPolygon(toCutArr, cutterArr);
                            if (cutoutArr.Length > 2)
                            {
                                List<IntPoint> cutout = new List<IntPoint>(cutoutArr);
                                outputHoles.Add(cutout);
                            }
                        }
                    }
                    catch { };
                }
            }

            result = new Contours(outputShapes, outputHoles);

            return result;
        }

        /////////////////////////////////////////
        //Constructors
        /// <summary>
        /// Contstuctor
        /// </summary>
        /// <param name="ListOfListOfValuesShape">Shapes</param>
        /// <param name="ListOfListOfValuesHole">Holes</param>
        public Contours(List<List<IntPoint>> ListOfListOfValuesShape, List<List<IntPoint>> ListOfListOfValuesHole)
        {
            _contourList = new List<Contour>();
            if (ListOfListOfValuesShape != null)
            {
                foreach (List<IntPoint> lst in ListOfListOfValuesShape)
                {
                    _contourList.Add(new Contour(lst, false));
                }
            }
            if (ListOfListOfValuesHole != null)
            {
                foreach (List<IntPoint> lst in ListOfListOfValuesHole)
                {
                    _contourList.Add(new Contour(lst, true));
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="contourList">A List with both holes and contours</param>
        public Contours(List<Contour> contourList)
        {
            _contourList = new List<Contour>();
            foreach (Contour cont in contourList)
            {
                if (!cont.IS_HOLE)
                {
                    _contourList.Add(cont);
                }
            }
            foreach (Contour cont in contourList)
            {
                if (cont.IS_HOLE)
                {
                    _contourList.Add(cont);
                }
            }
        }

        /////////////////////////////////////////
        //public functions
        /// <summary>
        /// Sets the offset of ALL contours inside
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void setOffset(long x, long y)
        {
            foreach (Contour c in _contourList)
            {
                c.setOffset(x, y);
            }
        }

        /// <summary>
        /// Combines this set of Contours with another, without any logic
        /// </summary>
        /// <param name="cont2"></param>
        public void combineWith(Contours cont2)
        {
            List<Contour> contourList = new List<Contour>(cont2._contourList);
            contourList.AddRange(_contourList);
            _contourList = new List<Contour>();
            foreach (Contour cont in contourList)
            {
                if (!cont.IS_HOLE)
                {
                    _contourList.Add(cont);
                }
            }
            foreach (Contour cont in contourList)
            {
                if (cont.IS_HOLE)
                {
                    _contourList.Add(cont);
                }
            }
        }

        /// <summary>
        /// Simplifies the polygons inside it, to make math/drawing easier
        /// </summary>
        /// <param name="simplifier">The "ContourSimplifier" element, a class that simplifies contours</param>
        public void simplify(ContourSimplifier simplifier)
        {
            double dblTolerance = simplifier.TOLERANCE;
            foreach (Contour c in _contourList)
            {
                double dblWidth = c.GET_RECT.right - c.GET_RECT.left;
                double dblHeight = c.GET_RECT.bottom - c.GET_RECT.bottom;
                double dblDiameter = dblWidth > dblHeight ? dblWidth : dblHeight;
                if (dblDiameter > dblTolerance * 2)
                {
                    simplifier.contoursToCalculate(c);
                }
            }
            simplifier.simplify();
        }

        /// <summary>
        /// Sorts by size
        /// </summary>
        /// <param name="bottomUp">Do you want to do it the right way, or the wrong way</param>
        public void sortBySize(bool bottomUp = false)
        {
            if (bottomUp)
            {
                _contourList.Sort(delegate(Contour x, Contour y)
                {
                    return x.CONTOUR.Count.CompareTo(y.CONTOUR.Count);
                });
            }
            else
            {
                _contourList.Sort(delegate(Contour x, Contour y)
                {
                    return -x.CONTOUR.Count.CompareTo(y.CONTOUR.Count);
                });
            }
        }

        /// <summary>
        /// Sorts by area
        /// </summary>
        /// <param name="bottomUp">Does the big thingy go up?</param>
        public void sortByArea(bool bottomUp = false)
        {
            if (bottomUp)
            {
                _contourList.Sort(delegate(Contour x, Contour y)
                {
                    return x.AREA.CompareTo(y.AREA);
                });
            }
            else
            {
                _contourList.Sort(delegate(Contour x, Contour y)
                {
                    return -x.AREA.CompareTo(y.AREA);
                });
            }
        }

        /// <summary>
        /// A filter function which filters by size
        /// </summary>
        /// <param name="dblBorderArea">If "blnRemoveBig" is true, everything above this border will get deleted; Else, everything below this border will get deleted</param>
        /// <param name="blnRemoveBig">The filter can be made for big or for small objects</param>
        public void filterByArea(double dblBorderArea, bool blnRemoveBig = false)
        {
            List<Contour> newList = new List<Contour>();
            foreach (Contour c in _contourList)
            {
                if (blnRemoveBig)
                {
                    if (Math.Abs(c.AREA) <= dblBorderArea)
                    {
                        newList.Add(c);
                    }
                }
                else
                {
                    if (Math.Abs(c.AREA) >= dblBorderArea)
                    {
                        newList.Add(c);
                    }
                }
            }
            _contourList = newList;
        }
        
        /// <summary>
        /// Smelts the current contours to one, whenever possible (Overlapping Elements will now be one)
        /// </summary>
        /// <returns>A new set of Contours</returns>
        public Contours smelt(double delta = 1, JoinType jt = JoinType.jtSquare)
        {
            Contours a = this;

            ClipperOffset offset = new ClipperOffset();
            
            List<List<IntPoint>> paths = new List<List<IntPoint>>();

            foreach (Contour c in this._contourList)
            {
                if (!c.IS_HOLE)
                {
                    paths.Add(c.CONTOUR);
                }
            }

            offset.AddPaths(paths, jt, EndType.etClosedPolygon);

            List<List<IntPoint>> results = new List<List<IntPoint>>();

            offset.Execute(ref results, delta);

            List<Contour> lst = new List<Contour>();

            foreach (List<IntPoint> p in results)
            {
                lst.Add(new Contour(p, !Clipper.Orientation(p)));
            }


            return new Contours(lst);
        }

        /////////////////////////////////////////
        //FACTS

        /// <summary>
        /// Gives you the ammout of contours inside.
        /// </summary>
        public int COUNT
        {
            get
            {
                return _contourList.Count;
            }
        }

        /// <summary>
        /// Gives you the actual contours, without holes. Should be called "SHAPES", but is called "CONTOURS. Deal with it.
        /// </summary>
        public Contours CONTOURS
        {
            get
            {
                return new Contours(actualShapes());
            }
        }

        /// <summary>
        /// Gives you all the holes. All of them.
        /// </summary>
        public Contours HOLES
        {
            get
            {
                return new Contours(actualHoles());
            }
        }

        /// <summary>
        /// The most boring function; It just gives you the whole list of elements.
        /// </summary>
        public List<Contour> LIST
        {
            get
            {
                return _contourList;
            }
        }
    }
}
