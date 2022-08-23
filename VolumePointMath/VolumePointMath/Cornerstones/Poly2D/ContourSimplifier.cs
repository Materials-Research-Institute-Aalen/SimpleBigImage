using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cornerstones.Poly2DMath
{
    /// <summary>
    /// An abstract class to simplify contours, must be used if implementing your own contour simplifier. Else, there is no need for you to know about it.
    /// </summary>
    public abstract class ContourSimplifier
    {
        protected List<Contour> _toSimplify;
        protected double _tolerance;

        public double TOLERANCE
        {
            get
            {
                return _tolerance;
            }
        }

        /// <summary>
        /// The constructor, with the given tolerance "per simplification"
        /// </summary>
        /// <param name="tolerance">The expected tolerance, i.e. the distance between the resulting polygon to any point of the previous polygon</param>
        public ContourSimplifier(double tolerance)
        {
            _toSimplify = new List<Contour>();
            _tolerance = tolerance;
        }

        /// <summary>
        /// The list of elements to calculate/simplify
        /// </summary>
        /// <param name="input"></param>
        public void contoursToCalculate(List<Contour> input)
        {
            _toSimplify = input;
        }

        /// <summary>
        /// Adds an element to calculate/simplify
        /// </summary>
        /// <param name="input"></param>
        public void contoursToCalculate(Contour input)
        {
            _toSimplify.Add(input);
        }

        /// <summary>
        /// A private function to return the contour as IntPoint
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private List<IntPoint> getContour(Contour input)
        {
            return input.CONTOUR;
        }

        /// <summary>
        /// The actual simplification call
        /// </summary>
        public void simplify()
        {
            foreach (Contour c in _toSimplify)
            {
                c.simplifyWith(this.simplify);
            }
        }

        /// <summary>
        /// The distance of a point from a line made from point1 and point2.
        /// </summary>
        /// <param name="pt1">The PT1.</param>
        /// <param name="pt2">The PT2.</param>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        public Double PerpendicularDistance
            (IntPoint Point1, IntPoint Point2, IntPoint Point)
        {
            //Area = |(1/2)(x1y2 + x2y3 + x3y1 - x2y1 - x3y2 - x1y3)|   *Area of triangle
            //Base = v((x1-x2)²+(x1-x2)²)                               *Base of Triangle*
            //Area = .5*Base*H                                          *Solve for height
            //Height = Area/.5/Base

            Double area = Math.Abs(.5 * (Point1.X * Point2.Y + Point2.X *
            Point.Y + Point.X * Point1.Y - Point2.X * Point1.Y - Point.X *
            Point2.Y - Point1.X * Point.Y));
            Double bottom = Math.Sqrt(Math.Pow(Point1.X - Point2.X, 2) +
            Math.Pow(Point1.Y - Point2.Y, 2));
            Double height = area / bottom * 2;

            return height;

        }

        /// <summary>
        /// This function must be implemented
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        abstract protected List<IntPoint> simplify(List<IntPoint> input);
    }
}
