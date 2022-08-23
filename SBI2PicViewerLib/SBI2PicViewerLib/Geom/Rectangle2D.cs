using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing.Drawing2D;
using Cornerstones.Poly2DMath;
using System.Drawing;

namespace SBI2PicViewerLib.Geom
{
    class Rectangle2D : Room2D
    {
        protected float width, height;

        public Rectangle2D(double left, double top, double right, double bottom)
        {
            this.position(left, top);

            width = (float)(right - left);
            height = (float)(bottom - top);
        }

        public Rectangle2D(float left, float top, float right, float bottom)
        {
            this.position(left, top);

            width = right - left;
            height = bottom - top;
        }

        public bool doesOverlap(RectangleF rect)
        {
            return doesOverlap(rect.X, rect.Y, rect.Width, rect.Height);
        }

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
        
        public float getWidth()
        {
            return width;
        }

        public float getHeight()
        {
            return height;
        }

        public System.Drawing.PointF[] getCorners()
        {
            System.Drawing.PointF[] corners = new System.Drawing.PointF[4];
            //ToDo: General Function: Width and Hight to Polygon!
            for (int i = 0; i < 4; i++)
            {
                corners[i] = new System.Drawing.PointF(((i & 2) == 2 ? width : 0), (((i - 1) & 2) == 2 ? 0 : height));
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
    }
}
