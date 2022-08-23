using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cornerstones.Poly2DMath
{
    public delegate List<IntPoint> SimplificationMethod(List<IntPoint> input);

    /// <summary>
    /// A "Contour" class, which is a polygon describing a certain feature or element of a segmented image
    /// </summary>
    public class Contour
    {
        public static bool CLOSED = true;

        List<IntPoint> _actualContour;
        bool _isHole;
        double? _dblArea = null;
        double? _dblAngle = null;
        bool? _isClockwise = null;
        IntRect? _rectangle = null;
        double? _circumference = null;
        double? _circumference_for_sorting = null;

        /////////////////////////////////////////
        //Operations

        /// <summary>
        /// Gives back one inpoint of the contour stored inside
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IntPoint this[int index]
        {
            get
            {
                return new IntPoint(_actualContour[index].X, _actualContour[index].Y);
            }
        }

        /////////////////////////////////////////
        //Operations
        /// <summary>
        /// A general function for polygon operations, backed by the clipper library
        /// </summary>
        /// <param name="b"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private Contours operation(Contour b, ClipType type)
        {
            Contour a = this;

            Clipper clip = new Clipper();
            clip.AddPath(a._actualContour, PolyType.ptSubject, CLOSED);
            clip.AddPath(b._actualContour, PolyType.ptClip, CLOSED);

            PolyTree tree = new PolyTree();

            clip.Execute(type, tree);

            List<Contour> outList = new List<Contour>();

            foreach (PolyNode n in tree.m_AllPolys)
            {
                Contour c = new Contour(n.Contour, n.IsHole);
                outList.Add(c);
            }

            return new Contours(outList);
        }

        /// <summary>
        /// Multiplication means "intersection", everything that overlays is the result.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Contours operator *(Contour a, Contour b)
        {
            if (a == null || b == null)
            {
                return new Contours(new List<Contour>());
            }
            return a.operation(b, ClipType.ctIntersection);
        }

        /// <summary>
        /// Division means "xOr", everything that doesn't overlay is the result.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Contours operator /(Contour a, Contour b)
        {
            if (a == null || b == null)
            {
                return new Contours(new List<Contour>());
            }
            return a.operation(b, ClipType.ctXor);
        }

        /// <summary>
        /// Addition means "Union", everything gets smelted together.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Contours operator +(Contour a, Contour b)
        {
            if (a == null || b == null)
            {
                return new Contours(new List<Contour>());
            }
            return a.operation(b, ClipType.ctUnion);
        }

        /// <summary>
        /// Subtraction means "difference", technically b gets removed from a.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Contours operator -(Contour a, Contour b)
        {
            if (a == null || b == null)
            {
                return new Contours(new List<Contour>());
            }
            return a.operation(b, ClipType.ctDifference);
        }

        /////////////////////////////////////////
        //Constructors
        
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="values">A list of IntPoints, defining a polygon</param>
        /// <param name="isHole">This information defines if the polygon is part of a bigger polygon, and thus if it is a hole inside this polygon</param>
        public Contour(List<IntPoint> values, bool isHole = false)
        {
            _actualContour = values;
            _isHole = isHole;
            _rectangle = null;
        }

        public Contour(IntRect rectangle, bool isHole = false)
        {
            _actualContour = new List<IntPoint>();
            for (int i = 0; i < 4; i++)
            {
                //_actualContour.Add(new IntPoint(((i & 2) == 2 ? rectangle.right : rectangle.left), (((i - 1) & 2) == 2 ? rectangle.top : rectangle.bottom)));
                _actualContour.Add(new IntPoint(((i & 2) == 2 ? rectangle.left : rectangle.right), (((i - 1) & 2) == 2 ? rectangle.top : rectangle.bottom)));
            }
            _isHole = isHole;
            _rectangle = null;
        }

        /////////////////////////////////////////
        //Functions

        /// <summary>
        /// Draws a contour onto an image
        /// </summary>
        /// <param name="input">The Image to draw on</param>
        /// <param name="blnFill">Fill the polygon, or only draw a line</param>
        /// <param name="clr">The color of the line AND the filling (if chosen)</param>
        /// <param name="iXOffset">The offset from the upper left corner</param>
        /// <param name="iYOffset">The offset from the upper left corner</param>
        /// <param name="xResize">If smaller zero, the polygon becomes smaller, else, the polygon becomes bigger</param>
        /// <param name="yResize">If smaller zero, the polygon becomes smaller, else, the polygon becomes bigger</param>
        /// <returns>An image with the polygon on it</returns>
        public Image drawOntoImage(Image input, bool blnFill, Color clr, long iXOffset, long iYOffset, double xResize = 1, double yResize = 1)
        {
            Point[] polygonArray = new Point[_actualContour.Count];
            double xPos = GET_RECT.left;
            double yPos = GET_RECT.top;

            for (int i = 0; i < polygonArray.Length; i++)
            {
                polygonArray[i] = new Point((int)((double)(_actualContour[i].X + iXOffset) * xResize), (int)((double)(_actualContour[i].Y + iYOffset) * yResize));
            }
            
            SolidBrush brush = new SolidBrush(clr);

            Graphics g = Graphics.FromImage(input);
            g.FillPolygon(brush, polygonArray, System.Drawing.Drawing2D.FillMode.Winding);

            return input;
        }

        /// <summary>
        /// Clones a contour
        /// </summary>
        /// <returns>An identical contour</returns>
        public Contour CLONE()
        {
            List<IntPoint> output = new List<IntPoint>();

            foreach (IntPoint p in _actualContour)
            {
                output.Add(new IntPoint(p.X, p.Y));
            }

            return new Contour(output, this.IS_HOLE);
        }

        /// <summary>
        /// Rotates the output polygon by dblAngle
        /// </summary>
        /// <param name="dblAngle">The angle</param>
        /// <returns>The polygon, as double point, to prevent integer clipping</returns>
        public List<DoublePoint> getRotatedList(double dblAngle)
        {
            double x = (double)(GET_RECT.left + GET_RECT.right) / 2.0;
            double y = (double)(GET_RECT.top + GET_RECT.bottom) / 2.0;

            DoublePoint middle = new DoublePoint(x, y);

            List<DoublePoint> rotated = new List<DoublePoint>();

            foreach (IntPoint p in _actualContour)
            {
                DoublePoint point = new DoublePoint(p.X, p.Y);

                double dblX = point.X;
                double dblY = point.Y;

                dblX -= middle.X;
                dblY -= middle.Y;

                double dblXrot = dblX * Math.Cos(dblAngle) - dblY * Math.Sin(dblAngle);
                double dblYrot = dblX * Math.Sin(dblAngle) + dblY * Math.Cos(dblAngle);

                dblXrot += middle.X;
                dblYrot += middle.Y;

                rotated.Add(new DoublePoint(dblXrot, dblYrot));
            }

            return rotated;
        }

        /// <summary>
        /// Checks if a given point is inside the polygon, point being at the border is acceptable, too. Does NOT check holes, inside is inside.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>"true" if the given point touches the polygon</returns>
        public bool isPointInside(double x, double y)
        {
            if (x >= RECTANGLE.left && x <= RECTANGLE.right && y >= RECTANGLE.top && y <= RECTANGLE.bottom)
            {
                return Clipper.PointInPolygon(new IntPoint(x, y), this.CONTOUR) != 0;
            }
            return false;
        }

        /// <summary>
        /// A function that removes invalid data from a list
        /// </summary>
        /// <param name="input">the data to be checked</param>
        /// <returns>the result</returns>
        private DoublePoint[] makeList(DoublePoint[] input)
        {
            int iLast = 0;
            DoublePoint[] output = new DoublePoint[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                if (double.IsNaN(input[i].X))
                {
                    if (i == 0)
                    {
                        while (double.IsNaN(input[i].X))
                        {
                            output[i] = new DoublePoint(double.NaN, double.NaN);
                            i++;
                            if (i >= output.Length)
                            {
                                return null;
                            }
                        }
                        i--;
                    }
                    else
                    {
                        int iOffset = -1;
                        double dblFirst = input[i + iOffset].Y;
                        iOffset++;
                        bool blnLastAreInvalid = false;
                        while (double.IsNaN(input[i + iOffset].X))
                        {
                            iOffset++;
                            if (i + iOffset >= input.Length)
                            {
                                iOffset--;
                                blnLastAreInvalid = true;
                                break;
                            }
                        }
                        double dblSecond = input[i + iOffset].Y;

                        for (int j = 0; j < iOffset; j++)
                        {
                            if (!blnLastAreInvalid)
                            {
                                output[i + j].X = i + j + output[0].X;

                                double secondWeight = (double)(j + 1) / (double)(iOffset + 1);
                                double firstWeight = 1 - secondWeight;

                                output[i + j].Y = dblFirst * firstWeight + dblSecond * secondWeight;
                            }
                            else
                            {
                                output[i + j] = new DoublePoint(double.NaN, double.NaN);
                            }
                        }

                        i += (iOffset - 1);
                    }
                }
                else
                {
                    iLast = i;
                    output[i] = input[i];
                }
            }

            return output;
        }

        /// <summary>
        /// Separates a DoublePoint Polygon at the middle, making it two lines: an upper line and a lower line
        /// </summary>
        /// <param name="input">The input</param>
        /// <returns>Upper and Lower line</returns>
        private DoublePoint[][] getUpperAndLower(List<DoublePoint> input)
        {
            int iTopPos = 0;
            int iLeftPos = 0;
            int iBottomPos = 0;
            int iRightPos = 0;
            
            double top = input[0].Y;
            double left = input[0].X;
            double bottom = input[0].Y;
            double right = input[0].X;

            for (int i = 0; i < input.Count; i++) 
            {
                DoublePoint p = input[i];

                if (!(top < p.Y))
                {
                    top = p.Y;
                    iTopPos = i;
                }
                if (!(left < p.X))
                {
                    left = p.X;
                    iLeftPos = i;
                }
                if (!(bottom > p.Y))
                {
                    bottom = p.Y;
                    iBottomPos = i;
                }
                if (!(right > p.X))
                {
                    right = p.X;
                    iRightPos = i;
                }
            }

            int iLowest = (int)Math.Floor(left);
            int iHighest = (int)Math.Ceiling(right);
            int iSize = iHighest - iLowest;

            DoublePoint[] upper = new DoublePoint[iSize];
            DoublePoint[] lower = new DoublePoint[iSize];

            int iLeftPosForUpper = 0;
            int iRightPosForUpper = 0;

            int iLeftPosForLower = 0;
            int iRightPosForLower = 0;

            if (iLeftPos < iRightPos)
            {
                iLeftPosForUpper = iLeftPos;
                iRightPosForUpper = iRightPos;
                iLeftPosForLower = iRightPos + input.Count;
                iRightPosForLower = iLeftPos;
            }
            else
            {
                iLeftPosForUpper = iLeftPos;
                iRightPosForUpper = iRightPos + input.Count;
                iLeftPosForLower = iRightPos;
                iRightPosForLower = iLeftPos;
            }

            for (int i = 0; i < upper.Length; i++)
            {
                upper[i] = new DoublePoint(double.NaN, double.NaN);
            }
            for (int i = 0; i < lower.Length; i++)
            {
                lower[i] = new DoublePoint(double.NaN, double.NaN);
            }

            for (int i = iLeftPosForUpper; i <= iRightPosForUpper; i++)
            {
                int iPos = i % input.Count;
                int iXPos = (int)Math.Floor(input[iPos].X);

                double x = input[iPos].X;
                double y = input[iPos].Y;

                if (double.IsNaN(upper[iXPos - iLowest].X) || y < upper[iXPos - iLowest].Y)
                {
                    upper[iXPos - iLowest] = new DoublePoint(x, y);
                }
            }

            for (int i = iLeftPosForLower; i <= iRightPosForLower; i++)
            {
                int iPos = i % input.Count;
                int iXPos = (int)Math.Floor(input[iPos].X);
                double x = input[iPos].X;
                double y = input[iPos].Y;

                if (double.IsNaN(lower[iXPos - iLowest].X) || y > lower[iXPos - iLowest].Y)
                {
                    lower[iXPos - iLowest] = new DoublePoint(x, y);
                }
            }
            
            upper = makeList(upper);
            lower = makeList(lower);

            if (upper == null || lower == null)
            {
                return null;
            }

            return new DoublePoint[][] { upper, lower };
        }

        /// <summary>
        /// Makes a polygon a table of heights/thicknesses
        /// </summary>
        /// <param name="dblAngle">the angle to measure in/to rotate the polygon before measuring</param>
        /// <returns>A table of widths/heights, depending on the angle</returns>
        public double[] getHeightTable(double dblAngle)
        {
            List<DoublePoint> rotatedContour = getRotatedList(dblAngle);
            IntRect rectangle;

            rectangle.top = (int)rotatedContour[0].Y;
            rectangle.bottom = (int)rotatedContour[0].Y;
            rectangle.left = (int)rotatedContour[0].X;
            rectangle.right = (int)rotatedContour[0].X;

            foreach (DoublePoint p in rotatedContour)
            {
                rectangle.top = rectangle.top < p.X ? rectangle.top : (int)Math.Round(p.X);
                rectangle.left = rectangle.left < p.X ? rectangle.left : (int)Math.Round(p.X);
                rectangle.bottom = rectangle.bottom > p.X ? rectangle.bottom : (int)Math.Round(p.X);
                rectangle.right = rectangle.right > p.X ? rectangle.right : (int)Math.Round(p.X);
            }


            DoublePoint[][] data = getUpperAndLower(rotatedContour);

            if (data == null)
            {
                return null;
            }

            double[] output = new double[data[0].Length];
            List<double> info = new List<double>();

            for (int i = 0; i < output.Length; i++)
            {
                if (!double.IsNaN(data[1][i].X) && !double.IsNaN(data[0][i].X))
                {
                    output[i] = data[1][i].Y - data[0][i].Y;
                    info.Add(output[i]);
                }
                else
                {
                    output[i] = double.NaN;
                }
            }

            return output;
        }

        /// <summary>
        /// Calculates the angle between two points
        /// </summary>
        /// <param name="inside">The "inside" point</param>
        /// <param name="outside">The "outside" point</param>
        /// <param name="length">Output: The distance between the points</param>
        /// <param name="angle">Output: The angle</param>
        private void calculateSingleAngle(IntPoint inside, IntPoint outside, out double length, out double angle)
        {
            length = 0;
            angle = double.NegativeInfinity;

            double x1 = inside.X;
            double y1 = inside.Y;

            double x2 = outside.X;
            double y2 = outside.Y;

            length = ((x1 - x2) * (x1 - x2)) + ((y1 - y2) * (y1 - y2));
            angle = Math.Atan2((y1 - y2), (x1 - x2)) + Math.PI;
        }

        /// <summary>
        /// Returns the angle of the contour
        /// </summary>
        private void calculateAngle()
        {
            long x = (GET_RECT.left + GET_RECT.right) / 2;
            long y = (GET_RECT.top + GET_RECT.bottom) / 2;

            IntPoint middle = new IntPoint(x, y);

            double dblAngle = double.NegativeInfinity;
            double dblLength = double.NegativeInfinity;

            foreach (IntPoint p in _actualContour)
            {
                double dblLocalAngle;
                double dblLocalLength;
                calculateSingleAngle(middle, p, out dblLocalLength, out dblLocalAngle);
                if (dblLocalLength > dblLength)
                {
                    dblAngle = dblLocalAngle;
                    dblLength = dblLocalLength;
                }
            }

            _dblAngle = dblAngle;
        }

        /// <summary>
        /// Uses a simplification method to reduce the amount of polygon points
        /// </summary>
        /// <param name="method">A method to use</param>
        public void simplifyWith(SimplificationMethod method)
        {
            _actualContour = method(_actualContour);
            _rectangle = null;
        }

        /// <summary>
        /// Sets the offset of the contour
        /// </summary>
        /// <param name="x">x offset</param>
        /// <param name="y">y offset</param>
        public void setOffset(long x, long y)
        {
            int iMax = _actualContour.Count;
            for (int i = 0; i < iMax; i++)
            {
                IntPoint p = _actualContour[i];
                //p.X += x;
                //p.Y += y;
                _actualContour[i] = new IntPoint(p.X + x, p.Y + y);
            }
            _rectangle = null;
        }

        /////////////////////////////////////////
        //FACTS

        public double ANGLE
        {
            get
            {
                if (_dblAngle != null)
                {
                    return (double)_dblAngle;
                }
                calculateAngle();
                return (double)_dblAngle;
            }
        }

        public double AREA
        {
            get
            {
                if (_dblArea != null)
                {
                    return (double)_dblArea;
                }
                _dblArea = Clipper.Area(_actualContour);
                return (double)_dblArea;
            }
        }

        private IntRect RECTANGLE
        {
            get
            {
                List<List<IntPoint>> lst = new List<List<IntPoint>>();
                lst.Add(_actualContour);
                return Clipper.GetBounds(lst);
            }
        }
        
        public double CIRCUMFERENCE_FOR_SORTING
        {
            get
            {
                if (_circumference_for_sorting == null)
                {
                    Random r = new Random();
                    _circumference_for_sorting = CIRCUMFERENCE;
                    _circumference_for_sorting += r.NextDouble();
                }
                return (double)_circumference_for_sorting;
            }
        }

        public double CIRCUMFERENCE
        {
            get
            {
                if (_circumference == null)
                {
                    double output = 0;

                    IntPoint lastPoint = _actualContour[0];

                    foreach (IntPoint point in _actualContour)
                    {
                        double xDistance = point.X - lastPoint.X;
                        double yDistance = point.Y - lastPoint.Y;
                        double distance = Math.Sqrt(xDistance * xDistance + yDistance * yDistance);

                        output += distance;
                    }
                    
                    _circumference = output;

                    return output;
                }
                else
                {
                    return (double)_circumference;
                }

            }
        }

        /// <summary>
        /// The count of the contour's polygon
        /// </summary>
        public int COUNT
        {
            get
            {
                return _actualContour.Count;
            }
        }

        /// <summary>
        /// Is it a hole?
        /// </summary>
        public bool IS_HOLE
        {
            get
            {
                return _isHole;
            }
        }

        /// <summary>
        /// The rotation of the polygon, can be used to determine if it is a "hole" or not
        /// </summary>
        public bool CLOCKWISE
        {
            get
            {
                if (_isClockwise == null)
                {
                    _isClockwise = Clipper.Orientation(_actualContour);
                }

                return (bool)_isClockwise;
            }
        }

        /// <summary>
        /// The actual contour, as a list of IntPoint
        /// </summary>
        public List<IntPoint> CONTOUR
        {
            get
            {
                return _actualContour;
            }
        }

        /// <summary>
        /// The surrounding rectangle
        /// </summary>
        public IntRect GET_RECT
        {
            get
            {

                if (_rectangle == null)
                {
                    IntRect rect = RECTANGLE;
                    //_rectangle = new IntRect(RECTANGLE);
                    _rectangle = new IntRect(rect.left, rect.top, rect.right, rect.bottom);
                }
                
                return (IntRect)_rectangle;
            }
        }

        /////////////////////////////////////////
        //MISC
        /// <summary>
        /// Does this contour collide with the given contour?
        /// </summary>
        /// <param name="c">The other contour</param>
        /// <returns>"true" on collision</returns>
        public bool collidesWith(Contour c)
        {   
            try
            {
                return Sutherland.SutherlandHodgman.GetIntersectedPolygon(this._actualContour.ToArray(), c._actualContour.ToArray()).Length != 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Faster algorithm for collisions, just checks the ROIs
        /// </summary>
        /// <param name="rect"></param>
        /// <returns>If the rectangle of the contour collides with the given rectangle</returns>
        public bool isROIOverlapping(IntRect rect)
        {
            IntRect rect2 = this.RECTANGLE;
            double x1 = rect.right + rect.left;
            double y1 = rect.bottom + rect.top;

            double x2 = rect2.right + rect2.left;
            double y2 = rect2.bottom + rect2.top;

            x1 /= 2;
            y1 /= 2;

            x2 /= 2;
            y2 /= 2;

            double width = ((rect.right - rect.left) + (rect2.right - rect2.left)) / 2;
            double height = ((rect.bottom - rect.top) + (rect2.bottom - rect2.top)) / 2;

            if (Math.Abs(x2 - x1) > width)
            {
                return false;
            }
            if (Math.Abs(y2 - y1) > height)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Does this contour collide with the given contour?
        /// </summary>
        /// <param name="intPointList">A list of IntPoints</param>
        /// <returns>"true" on collision</returns>
        public bool collidesWith(List<IntPoint> intPointList)
        {
            try
            {
                return Sutherland.SutherlandHodgman.GetIntersectedPolygon(this._actualContour.ToArray(), intPointList.ToArray()).Length != 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
