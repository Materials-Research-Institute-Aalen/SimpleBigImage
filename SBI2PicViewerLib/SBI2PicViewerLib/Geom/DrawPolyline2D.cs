using Cornerstones.Poly2DMath;
using SBI2PicViewerLib.Geom;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using SBI2PicViewerLib.Renderer;

namespace SBI2PicViewerLib.Geom
{
    /// <summary>
    /// A Polyline, based on a concept called "2D Room". Used to draw Elements onto an image projected onto the OpenGL GUI
    /// </summary>
    class DrawPolyline2D : Room2D
    {
        float _x;
        float _y;
        float _width;
        float _height;
        Contour _contour;
        private System.Drawing.Color _color;

        /// <summary>
        /// The constructor, needing a contour and a color
        /// </summary>
        /// <param name="contour"></param>
        /// <param name="c"></param>
        public DrawPolyline2D(Contour contour, Color c)
            : base()
        {
            _contour = contour;
            _x = contour.GET_RECT.left;
            _y = contour.GET_RECT.top;
            _width = contour.GET_RECT.right - contour.GET_RECT.left;
            _height = contour.GET_RECT.bottom - contour.GET_RECT.top;
            _color = c;
        }

        /// <summary>
        /// Does it overlap with a rectangle? Useful i.e. for checking if it is visible for the camera, or if it touches a tile.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool doesOverlap(RectangleF rect)
        {
            if (_width == 0 || _height == 0)
            {
                return false;
            }

            // normal formula: (x11+x12)/2 - (x21+x22)/2, if multiplied with 2:
            float fDoubleDistanceX = (_x + _x + _width) - (rect.Left + rect.Right);
            fDoubleDistanceX = Math.Abs(fDoubleDistanceX);
            // normally you would only need half the width of each element
            float fCombinedWidthX = _width + rect.Width;

            float fDoubleDistanceY = (_y + _y + _height) - (rect.Top + rect.Bottom);
            fDoubleDistanceY = Math.Abs(fDoubleDistanceY);
            // normally you would only need half the width of each element
            float fCombinedWidthY = _height + rect.Height;

            return fCombinedWidthX >= fDoubleDistanceX && fCombinedWidthY >= fDoubleDistanceY;
            //return doesOverlap(rect.X, rect.Y, rect.Width, rect.Height);
        }

        /// <summary>
        /// Does it overlap with a rectangle? Useful i.e. for checking if it is visible for the camera, or if it touches a tile.
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public bool doesOverlap(float xPos, float yPos, float w, float h)
        {

            System.Drawing.PointF[] corners = getCorners();
            IntPoint[] firstPoly;
            IntPoint[] secondPoly;

            firstPoly = new IntPoint[]{
                new IntPoint(corners[0].X, corners[0].Y),
                new IntPoint(corners[1].X, corners[1].Y),
                new IntPoint(corners[2].X, corners[2].Y),
                new IntPoint(corners[3].X, corners[3].Y)
            };

            secondPoly = new IntPoint[]{
                new IntPoint(xPos, yPos),
                new IntPoint(xPos, yPos + h),
                new IntPoint(xPos + w, yPos + h),
                new IntPoint(xPos + w, yPos)
            };
            IntPoint[] collision;
            try
            {
                collision = Sutherland.SutherlandHodgman.GetIntersectedPolygon(firstPoly, secondPoly);
            }
            catch
            {
                return false;
            }

            bool result1 = collision.Length > 0;

            return result1;
        }

        public bool render(float zoom)
        {
            float size = _width > _height ? _width : _height;

            size *= zoom;

            if (_color == null)
            {
                return true;
            }

            float stepSize = 1 / (zoom * 5);
            int iStepSize = (int)Math.Floor(stepSize);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(PrimitiveType.Lines);

            GL.Color3(_color);
            IntPoint[] polygon;
            if (size < ContourManagement.MINIMUM_DRAWING_SIZE)
            {
                PointF[] corners = getCorners();
                List<IntPoint> interim = new List<IntPoint>();
                foreach (PointF corner in corners)
                {
                    interim.Add(new IntPoint(corner.X, corner.Y));
                }
                polygon = interim.ToArray();
            }
            else
            {
                polygon = _contour.CONTOUR.ToArray();
                if ((int)this.p.X != 0 && (int)this.p.Y != 0)
                {
                    for (int iElm = 0; iElm < polygon.Length; iElm++)
                    {
                        polygon[iElm].X += (int)this.p.X;
                        polygon[iElm].Y += (int)this.p.Y;
                    }
                }
            }

            int iMax = polygon.Length - 1;
            long x1;
            long y1;
            long x2;
            long y2;

            iStepSize = iStepSize > (iMax / 4) ? (iMax / 4) : iStepSize;
            iStepSize = iStepSize < 1 ? 1 : iStepSize;
            
            int i = 0;
            for (i = 0; i <= iMax - iStepSize; i += iStepSize)
            {
                x1 = polygon[i].X;
                y1 = polygon[i].Y;
                x2 = polygon[i + iStepSize].X;
                y2 = polygon[i + iStepSize].Y;

                GL.Vertex2(x1, y1);
                GL.Vertex2(x2, y2);
            }

            if (i > iMax)
            {
                i -= iStepSize;
            }

            x1 = polygon[i].X;
            y1 = polygon[i].Y;

            x2 = polygon[0].X;
            y2 = polygon[0].Y;

            GL.Vertex2(x1, y1);
            GL.Vertex2(x2, y2);

            GL.End();

            return true;
        }

        public float getWidth()
        {
            return _width;
        }

        public float getHeight()
        {
            return _height;
        }

        public System.Drawing.PointF[] getCorners()
        {
            System.Drawing.PointF[] corners = new System.Drawing.PointF[4];
            //ToDo: General Function: Width and Hight to Polygon!
            for (int i = 0; i < 4; i++)
            {
                corners[i] = new System.Drawing.PointF(((i & 2) == 2 ? _width : 0) + _x, (((i - 1) & 2) == 2 ? 0 : _height) + _y);
            }
            transformationMatrix.TransformPoints(corners);

            return corners;
        }

        public void getBounds(out float left, out float right, out float top, out float bottom)
        {
            System.Drawing.PointF[] corners = getCorners();

            left = corners[0].X;
            right = left;
            top = corners[0].Y;
            bottom = top;

            foreach (System.Drawing.PointF point in corners)
            {
                left = point.X < left ? point.X : left;
                right = point.X > right ? point.X : right;
                top = point.Y < top ? point.Y : top;
                bottom = point.Y > bottom ? point.Y : bottom;
            }
        }

        public bool isPointInside(long x, long y)
        {
            return isPointInside(new IntPoint(x, y));
        }

        public bool isPointInside(IntPoint p)
        {
            if (p.X > _x && p.Y > _y)
            {
                float x2 = _x + _width;
                float y2 = _y + _height;

                if (p.X < x2 && p.Y < y2)
                {
                    return Cornerstones.Poly2DMath.Clipper.PointInPolygon(p, _contour.CONTOUR) == 1;
                }
            }
            return false;
        }

        public Color COLOR
        {
            set
            {
                _color = value;
            }
            get
            {
                return _color;
            }
        }

        public Contour CONTOUR
        {
            get
            {
                return _contour;
            }
        }
    }
}
