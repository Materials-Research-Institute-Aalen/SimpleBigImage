using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cornerstones.Poly2DMath
{
    /// <summary>
    /// The Reumann contour simplifier
    /// </summary>
    class Reumann : ContourSimplifier
    {
        public Reumann(double tolerance)
            : base(tolerance)
        {

        }

        /// <summary>
        /// The simplification process
        /// </summary>
        /// <param name="input">A list of IntPoints which forms a polygon</param>
        /// <returns>A less complex list of IntPoints which forms a similar polygon</returns>
        protected override List<IntPoint> simplify(List<IntPoint> input)
        {
            if (_tolerance > 0)
            {
                List<IntPoint> list = input;
                int key = 0;
                while (key < list.Count - 3)
                {
                    int firstOut = key + 2;
                    while (firstOut < list.Count && PerpendicularDistance(list[key], list[key + 1], list[firstOut]) < _tolerance)
                    {
                        firstOut++;
                    }
                    //for (var i = key + 1; i < firstOut - 1; i++)
                    for (var i = firstOut - 2; i > key; i--)
                    {
                        list.RemoveAt(i);
                    }
                    key++;
                }
                return list;
            }
            return input;
        }

    }
}
