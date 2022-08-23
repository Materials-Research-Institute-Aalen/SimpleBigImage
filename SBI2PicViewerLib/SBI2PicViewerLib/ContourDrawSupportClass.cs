using Cornerstones.Poly2DMath;
using SBI2PicViewerLib.Geom;
using SBI2PicViewerLib.Renderer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBI2PicViewerLib
{
    /// <summary>
    /// A support for drawing polygons on an SBIRenderContainer
    /// </summary>
    class ContourDrawSupportClass
    {
        List<IntPoint> _oldContour;
        
        /// <summary>
        /// The Constructor
        /// </summary>
        public ContourDrawSupportClass()
        {
            _oldContour = null;
        }

        /// <summary>
        /// Creates an unsorted PointCloud from the mouse pointer
        /// </summary>
        /// <param name="_container">The SBI Render Container</param>
        /// <param name="x">x offset of the pointcloud</param>
        /// <param name="y">y offset of the pointcloud</param>
        /// <returns>A List of IntPoints</returns>
        private List<IntPoint> makeMousePolygonAPointCloud( SBIRenderContainer _container, float x, float y)
        {
            List<IntPoint> contour = new List<IntPoint>();
            foreach (DrawPolyline2D line in _container.MOUSEPOINTER)
            {
                List<IntPoint> mouseContour = line.CONTOUR.CONTOUR;

                for (int i = 0; i < mouseContour.Count; i++)
                {
                    contour.Add(new IntPoint(mouseContour[i].X + (int)x, mouseContour[i].Y + (int)y));
                }

            }
            return contour;
        }

        /// <summary>
        /// Creates a convex hull of two polygons (mostly "Mousepointer" polygons)
        /// </summary>
        /// <param name="_container">The SBI Render Container</param>
        /// <param name="x">The X Mouse Offset on Image</param>
        /// <param name="y">The Y Mouse Offset on Image</param>
        /// <returns></returns>
        public Contours getContoursToAddOnMouseDown(SBIRenderContainer _container, float x, float y)
        {
            List<Contour> contourList = new List<Contour>();
            List<IntPoint> currentContour = makeMousePolygonAPointCloud(_container, x, y);

            List<IntPoint> both = getConvexHull(currentContour, null);

            _oldContour = currentContour;

            contourList.Add(new Contour(both));
            
            Contours contours = new Contours(contourList);
            return contours;
        }

        /// <summary>
        /// Resets the Contour
        /// </summary>
        /// <param name="result">unused</param>
        public void onMouseUp(Contours result)
        {
            _oldContour = null;
        }

        /// <summary>
        /// Smears a polygon in a simulated movement from a to b
        /// </summary>
        /// <param name="_container">The render container (contains the "mouse polygon")</param>
        /// <param name="x">The mouse x position on image</param>
        /// <param name="y">The mouse y position on image</param>
        /// <returns>A polygon in the form of an IntPoint List</returns>
        public List<IntPoint> getIntPointListToAddOnMove(SBIRenderContainer _container, float x, float y)
        {
            if (_container != null)
            {
                List<Contour> contourList = new List<Contour>();
                List<IntPoint> currentContour = makeMousePolygonAPointCloud(_container, x, y);

                List<IntPoint> both = getConvexHull(currentContour, _oldContour);

                _oldContour = currentContour;
                return both;
            }
            return null;
        }

        /// <summary>
        /// Whenever the mouse moves, this function is called. It adds one smeared contour to the next to create a full shape.
        /// </summary>
        /// <param name="_container">The RenderContainer</param>
        /// <param name="x">the mouse x position</param>
        /// <param name="y">the mouse y position</param>
        /// <returns>Enlonged Contour</returns>
        public Contours getContoursToAddOnMove(SBIRenderContainer _container, float x, float y)
        {
            List<IntPoint> list = getIntPointListToAddOnMove(_container, x, y);
            List<Contour> cont = new List<Contour>();
            cont.Add(new Contour(list));

            return new Contours(cont);
        }

        /// <summary>
        /// Creates a convex hull from IntPoints
        /// </summary>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <returns>The Convex Hull from whatever was put in</returns>
        public List<IntPoint> getConvexHull(List<IntPoint> input1, List<IntPoint> input2)
        {
            List<IntPoint> values = new List<IntPoint>();
            if (input1 != null)
            {
                values.AddRange(input1);
            }
            if (input2 != null)
            {
                values.AddRange(input2);
            }

            values = GrahamScan.GrahamScanCompute(values).ToList();
            return values;
        }

    }
}
