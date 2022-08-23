using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing.Drawing2D;
using System.Drawing;

namespace SBI2PicViewerLib.Geom
{
    class Room2D
    {
        protected Matrix transformationMatrix = new Matrix();
        protected Matrix invertedTransformationMatrix = new Matrix();

        protected float rotation;
        protected PointF p;
        protected float scaling;

        public Room2D()
        {
            rotation = 0;
            p = new PointF(0, 0);
            scaling = 1;
        }

        protected void recalculateMatrices()
        {
            if (scaling == float.NaN || scaling == 0 || scaling == float.PositiveInfinity || scaling == float.NegativeInfinity)
            {
                return;
            }
            transformationMatrix = new Matrix();

            transformationMatrix.Rotate(rotation, MatrixOrder.Append);
            transformationMatrix.Scale(scaling, scaling, MatrixOrder.Append);
            transformationMatrix.Translate(p.X, p.Y, MatrixOrder.Append);
            
            invertedTransformationMatrix = transformationMatrix.Clone();
            invertedTransformationMatrix.Invert();
        }

        public void setPos(double fAngle, double fScaling, double xPos, double yPos)
        {
            setPos((float)fAngle, (float)fScaling, (float)xPos, (float)yPos);
        }

        public void setPos(float fAngle, float fScaling, float xPos, float yPos)
        {
            rotation = fAngle;
            p.X = xPos;
            p.Y = yPos;
            if (fScaling > 0)
            {
                scaling = fScaling;
            }
            recalculateMatrices();
        }

        public void rotate(double fAngle)
        {
            rotate((float)fAngle);
        }

        public void rotate(float fAngle)
        {
            rotation = fAngle;
            recalculateMatrices();
        }

        public void scale(double fScaling)
        {
            scale((float)fScaling);
        }

        public void scale(float fScaling)
        {
            if (scaling <= 0)
            {
                return;
            }
            scaling = fScaling;
            recalculateMatrices();
        }

        public void translate(double xPos, double yPos)
        {
            translate((float)xPos, (float)yPos);
        }

        public void translate(float xPos, float yPos)
        {
            position(p.X + xPos, p.Y + yPos);
        }

        public void position(double xPos, double yPos)
        {
            position((float)xPos, (float)yPos);
        }

        public void position(float xPos, float yPos)
        {
            p.X = xPos;
            p.Y = yPos;
            recalculateMatrices();
        }

        public float getXPos()
        {
            return p.X;
        }

        public float getYPos()
        {
            return p.Y;
        }

        public float getRotation()
        {
            return rotation;
        }

        public float getScale()
        {
            return scaling;
        }

        public float[] putOwnPointOverReferencePoint(float xSelf, float ySelf, float xRef, float yRef)
        {
            float[] pos = fromOwnPointToReference(new float[] { xSelf, ySelf });
            float xDif, yDif;

            xDif = xRef - pos[0];
            yDif = yRef - pos[1];

            position(p.X + xDif, p.Y + yDif);

            return new float[] { xDif, yDif };
        }

        public float[] fromReferencePointToOwnPoint(float[] points2D)
        {
            PointF[] calc = new PointF[1];
            calc[0] = new PointF(points2D[0], points2D[1]);
            invertedTransformationMatrix.TransformPoints(calc);

            return new float[] { calc[0].X, calc[0].Y };
        }

        public float[] fromOwnPointToReference(float[] points2D)
        {
            PointF[] calc = new PointF[1];
            calc[0] = new PointF(points2D[0], points2D[1]);
            transformationMatrix.TransformPoints(calc);

            return new float[] { calc[0].X, calc[0].Y };
        }
    }
}
