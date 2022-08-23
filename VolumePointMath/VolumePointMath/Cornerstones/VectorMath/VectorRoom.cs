using Cornerstones.Poly2DMath;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Cornerstones.VectorMath
{
    /// <summary>
    /// A class that creates a "VectorRoom"
    /// A Vectorroom is a concept, which creates a reference between two euklid rooms, i.e. the position of a point on an image ("virtual") as a reference to the position of a point on the stage
    /// "virtual" and "real" are just concepts
    /// </summary>
    [Serializable]
    public class VectorRoom
    {
        public int XPOS = 0, YPOS = 1, ZPOS = 2;
        public double[] virtualOffsetMatrix, realOffsetMatrix;
        public double[][] virtualCalibPositions, realCalibPositions;

        private Matrix calculate, inverse;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="calibPointCount">Count of calibration points. Default count is 3. Only 3 points possible at the moment.</param>
        public VectorRoom(int calibPointCount = 3)
        {
            virtualOffsetMatrix = new double[calibPointCount];
            realOffsetMatrix = new double[calibPointCount];
            virtualCalibPositions = new double[calibPointCount][];
            realCalibPositions = new double[calibPointCount][];

            for (int i = 0; i < calibPointCount; i++)
            {
                virtualCalibPositions[i] = new double[3];
                realCalibPositions[i] = new double[3];
            }
        }

        /// <summary>
        /// Constructor for serialization.
        /// </summary>
        /// <param name="calibPointCount">Count of calibration points. Default count is 3. Only 3 points possible at the moment.</param>
        public VectorRoom()
        {
            int calibPointCount = 3;
            virtualOffsetMatrix = new double[calibPointCount];
            realOffsetMatrix = new double[calibPointCount];
            virtualCalibPositions = new double[calibPointCount][];
            realCalibPositions = new double[calibPointCount][];

            for (int i = 0; i < calibPointCount; i++)
            {
                virtualCalibPositions[i] = new double[3];
                realCalibPositions[i] = new double[3];
            }
        }

        /// <summary>
        /// Calculates the vector matrices to be able to find a position on a picture in real life.
        /// </summary>
        public void calculateVectorMatrix()
        {
            Matrix RealMatrix = new Matrix(2, 2);
            Matrix VirtualMatrix = new Matrix(2, 2);
            Matrix RealMatrixINV = new Matrix(2, 2);

            try
            {
                RealMatrixINV[0, 0] = realCalibPositions[0][XPOS] - realCalibPositions[1][XPOS];
                RealMatrixINV[1, 0] = realCalibPositions[0][YPOS] - realCalibPositions[1][YPOS];

                RealMatrixINV[0, 1] = realCalibPositions[2][XPOS] - realCalibPositions[1][XPOS];
                RealMatrixINV[1, 1] = realCalibPositions[2][YPOS] - realCalibPositions[1][YPOS];

                RealMatrix = RealMatrixINV.Invert();

                VirtualMatrix[0, 0] = virtualCalibPositions[0][XPOS] - virtualCalibPositions[1][XPOS];
                VirtualMatrix[1, 0] = virtualCalibPositions[0][YPOS] - virtualCalibPositions[1][YPOS];

                VirtualMatrix[0, 1] = virtualCalibPositions[2][XPOS] - virtualCalibPositions[1][XPOS];
                VirtualMatrix[1, 1] = virtualCalibPositions[2][YPOS] - virtualCalibPositions[1][YPOS];

                calculate = VirtualMatrix * RealMatrix;
                inverse = calculate.Invert();
            }
            catch (Exception ex)
            {
                throw new Exception("Beim erstellen der Vektormatrix ist ein Problem aufgetreten. Appliaktion wird zurückgesetzt.\n\n" + ex.Message);
            }
        }

        /// <summary>
        /// Sets three points as calibration points in the virtual euclid vector room
        /// </summary>
        /// <param name="volumePoint0">The first point</param>
        /// <param name="volumePoint1">The second point</param>
        /// <param name="volumePoint2">The third point</param>
        public void setVirtualCalibPosition(DoublePoint volumePoint0, DoublePoint volumePoint1, DoublePoint volumePoint2)
        {
            setVirtualCalibPosition(0, volumePoint0);
            setVirtualCalibPosition(1, volumePoint1);
            setVirtualCalibPosition(2, volumePoint2);
        }

        /// <summary>
        /// Sets three points as calibration points in the virtual euclid vector room
        /// </summary>
        /// <param name="volumePoint0">The first point</param>
        /// <param name="volumePoint1">The second point</param>
        /// <param name="volumePoint2">The third point</param>
        public void setVirtualCalibPosition(IntPoint volumePoint0, IntPoint volumePoint1, IntPoint volumePoint2)
        {
            setVirtualCalibPosition(0, volumePoint0);
            setVirtualCalibPosition(1, volumePoint1);
            setVirtualCalibPosition(2, volumePoint2);
        }

        /// <summary>
        /// Sets one virtual calibration position
        /// </summary>
        /// <param name="number">The position to set (0 to 2)</param>
        /// <param name="volumePoint">The position</param>
        public void setVirtualCalibPosition(int number, DoublePoint volumePoint)
        {
            virtualCalibPositions[number % 3][XPOS] = volumePoint.X;
            virtualCalibPositions[number % 3][YPOS] = volumePoint.Y;
            virtualCalibPositions[number % 3][ZPOS] = 0;// volumePoint.Z;
            //Es wird der Mittelpunkt der drei Werte als Offset genommen, um Messfehler auszugleichen
            virtualOffsetMatrix[0] = (virtualCalibPositions[0][XPOS] + virtualCalibPositions[1][XPOS] + virtualCalibPositions[2][XPOS]) / 3;
            virtualOffsetMatrix[1] = (virtualCalibPositions[0][YPOS] + virtualCalibPositions[1][YPOS] + virtualCalibPositions[2][YPOS]) / 3;
            virtualOffsetMatrix[2] = (virtualCalibPositions[0][ZPOS] + virtualCalibPositions[1][ZPOS] + virtualCalibPositions[2][ZPOS]) / 3;
        }

        /// <summary>
        /// Returns one virtual calibration position
        /// </summary>
        /// <param name="number">The reference of the calibration position</param>
        /// <param name="volumePoint">Where to put the result in</param>
        /// <returns>The result</returns>
        public DoublePoint getVirtualCalibPosition(int number, DoublePoint volumePoint)
        {
            volumePoint.X = virtualCalibPositions[number % 3][XPOS];
            volumePoint.Y = virtualCalibPositions[number % 3][YPOS];
            //volumePoint.Z = (long)virtualCalibPositions[number % 3][ZPOS];

            return volumePoint;
        }

        /// <summary>
        /// Sets three points as calibration points in the real euclid vector room
        /// </summary>
        /// <param name="volumePoint0">The first point</param>
        /// <param name="volumePoint1">The second point</param>
        /// <param name="volumePoint2">The third point</param>
        public void setRealCalibPosition(DoublePoint volumePoint0, DoublePoint volumePoint1, DoublePoint volumePoint2)
        {
            setRealCalibPosition(0, volumePoint0);
            setRealCalibPosition(1, volumePoint1);
            setRealCalibPosition(2, volumePoint2);
        }

        /// <summary>
        /// Sets one real calibration position
        /// </summary>
        /// <param name="number">The position to set (0 to 2)</param>
        /// <param name="volumePoint">The position</param>
        public void setRealCalibPosition(int number, DoublePoint volumePoint)
        {
            realCalibPositions[number % 3][XPOS] = volumePoint.X;
            realCalibPositions[number % 3][YPOS] = volumePoint.Y;
            realCalibPositions[number % 3][ZPOS] = 0;// volumePoint.Z;
            //Es wird der Mittelpunkt der drei Werte als Offset genommen, um Messfehler auszugleichen
            realOffsetMatrix[0] = (realCalibPositions[0][XPOS] + realCalibPositions[1][XPOS] + realCalibPositions[2][XPOS]) / 3;
            realOffsetMatrix[1] = (realCalibPositions[0][YPOS] + realCalibPositions[1][YPOS] + realCalibPositions[2][YPOS]) / 3;
            realOffsetMatrix[2] = (realCalibPositions[0][ZPOS] + realCalibPositions[1][ZPOS] + realCalibPositions[2][ZPOS]) / 3;
        }

        /// <summary>
        /// Sets one virtual calibration position
        /// </summary>
        /// <param name="number">The position to set (0 to 2)</param>
        /// <param name="volumePoint">The position</param>
        public void setVirtualCalibPosition(int number, IntPoint volumePoint)
        {
            virtualCalibPositions[number % 3][XPOS] = volumePoint.X;
            virtualCalibPositions[number % 3][YPOS] = volumePoint.Y;
            virtualCalibPositions[number % 3][ZPOS] = volumePoint.Z;
            //Es wird der Mittelpunkt der drei Werte als Offset genommen, um Messfehler auszugleichen
            virtualOffsetMatrix[0] = (virtualCalibPositions[0][XPOS] + virtualCalibPositions[1][XPOS] + virtualCalibPositions[2][XPOS]) / 3;
            virtualOffsetMatrix[1] = (virtualCalibPositions[0][YPOS] + virtualCalibPositions[1][YPOS] + virtualCalibPositions[2][YPOS]) / 3;
            virtualOffsetMatrix[2] = (virtualCalibPositions[0][ZPOS] + virtualCalibPositions[1][ZPOS] + virtualCalibPositions[2][ZPOS]) / 3;
        }

        /// <summary>
        /// Returns one virtual calibration position
        /// </summary>
        /// <param name="number">The reference of the calibration position</param>
        /// <param name="volumePoint">Where to put the result in</param>
        /// <returns>The result</returns>
        public IntPoint getVirtualCalibPosition(int number, IntPoint volumePoint)
        {
            volumePoint.X = (long)virtualCalibPositions[number % 3][XPOS];
            volumePoint.Y = (long)virtualCalibPositions[number % 3][YPOS];
            volumePoint.Z = (long)virtualCalibPositions[number % 3][ZPOS];

            return volumePoint;
        }

        /// <summary>
        /// Sets three points as calibration points in the real euclid vector room
        /// </summary>
        /// <param name="volumePoint0">The first point</param>
        /// <param name="volumePoint1">The second point</param>
        /// <param name="volumePoint2">The third point</param>
        public void setRealCalibPosition(IntPoint volumePoint0, IntPoint volumePoint1, IntPoint volumePoint2)
        {
            setRealCalibPosition(0, volumePoint0);
            setRealCalibPosition(1, volumePoint1);
            setRealCalibPosition(2, volumePoint2);
        }

        /// <summary>
        /// Returns one virtual calibration position
        /// </summary>
        /// <param name="number">The reference of the calibration position</param>
        /// <param name="volumePoint">Where to put the result in</param>
        /// <returns>The result</returns>
        public void setRealCalibPosition(int number, IntPoint volumePoint)
        {
            realCalibPositions[number % 3][XPOS] = volumePoint.X;
            realCalibPositions[number % 3][YPOS] = volumePoint.Y;
            realCalibPositions[number % 3][ZPOS] = volumePoint.Z;
            //Es wird der Mittelpunkt der drei Werte als Offset genommen, um Messfehler auszugleichen
            realOffsetMatrix[0] = (realCalibPositions[0][XPOS] + realCalibPositions[1][XPOS] + realCalibPositions[2][XPOS]) / 3;
            realOffsetMatrix[1] = (realCalibPositions[0][YPOS] + realCalibPositions[1][YPOS] + realCalibPositions[2][YPOS]) / 3;
            realOffsetMatrix[2] = (realCalibPositions[0][ZPOS] + realCalibPositions[1][ZPOS] + realCalibPositions[2][ZPOS]) / 3;
        }

        /// <summary>
        /// Returns one real calibration position
        /// </summary>
        /// <param name="number">The reference of the calibration position</param>
        /// <param name="volumePoint">Where to put the result in</param>
        /// <returns>The result</returns>
        public IntPoint getRealCalibPosition(int number, IntPoint volumePoint)
        {
            volumePoint.X = (long)realCalibPositions[number % 3][XPOS];
            volumePoint.Y = (long)realCalibPositions[number % 3][YPOS];
            volumePoint.Z = (long)realCalibPositions[number % 3][ZPOS];

            return volumePoint;
        }

        /// <summary>
        /// Returns one real calibration position
        /// </summary>
        /// <param name="number">The reference of the calibration position</param>
        /// <param name="volumePoint">Where to put the result in</param>
        /// <returns>The result</returns>
        public DoublePoint getRealCalibPosition(int number, DoublePoint volumePoint)
        {
            volumePoint.X = realCalibPositions[number % 3][XPOS];
            volumePoint.Y = realCalibPositions[number % 3][YPOS];
            //volumePoint.Z = (long)realCalibPositions[number % 3][ZPOS];

            return volumePoint;
        }

        /// <summary>
        /// Sets the system as unity, that means a 1:1 transformation from "real" to "virtual" and back, meaning both are the same
        /// </summary>
        public void setAsUnity()
        {
            setRealCalibPosition(0, new DoublePoint(0, 0));
            setRealCalibPosition(1, new DoublePoint(1, 0));
            setRealCalibPosition(2, new DoublePoint(1, 1));

            setVirtualCalibPosition(0, new DoublePoint(0, 0));
            setVirtualCalibPosition(1, new DoublePoint(1, 0));
            setVirtualCalibPosition(2, new DoublePoint(1, 1));

            calculateVectorMatrix();
        }

        /// <summary>
        /// Returns the real position of the virtual room input vector
        /// </summary>
        /// <param name="volumePoint">the position in the virtual room</param>
        /// <returns>the position in the real room</returns>
        public System.Drawing.PointF getRealPosition(System.Drawing.PointF volumePoint)
        {
            double x1;
            double x2;

            volumePoint.X -= (long)virtualOffsetMatrix[0];
            volumePoint.Y -= (long)virtualOffsetMatrix[1];

            x1 = volumePoint.X * inverse[0, 0] + volumePoint.Y * inverse[0, 1];
            x2 = volumePoint.X * inverse[1, 0] + volumePoint.Y * inverse[1, 1];

            volumePoint.X = (long)x1;
            volumePoint.Y = (long)x2;

            volumePoint.X += (long)realOffsetMatrix[0];
            volumePoint.Y += (long)realOffsetMatrix[1];

            return volumePoint;
        }

        /// <summary>
        /// Returns the real position of the virtual room input vector
        /// </summary>
        /// <param name="volumePoint">the position in the virtual room</param>
        /// <returns>the position in the real room</returns>
        public IntPoint getRealPosition(IntPoint volumePoint)
        {
            double x1;
            double x2;

            volumePoint.X -= (long)virtualOffsetMatrix[0];
            volumePoint.Y -= (long)virtualOffsetMatrix[1];
            volumePoint.Z -= (long)virtualOffsetMatrix[2];

            x1 = volumePoint.X * inverse[0, 0] + volumePoint.Y * inverse[0, 1];
            x2 = volumePoint.X * inverse[1, 0] + volumePoint.Y * inverse[1, 1];

            volumePoint.X = (long)x1;
            volumePoint.Y = (long)x2;

            volumePoint.X += (long)realOffsetMatrix[0];
            volumePoint.Y += (long)realOffsetMatrix[1];
            volumePoint.Z += (long)realOffsetMatrix[2];

            return volumePoint;
        }

        /// <summary>
        /// Returns the real position of the virtual room input vector
        /// </summary>
        /// <param name="volumePoint">the position in the virtual room</param>
        /// <returns>the position in the real room</returns>
        public DoublePoint getRealPosition(DoublePoint volumePoint)
        {
            double x1;
            double x2;

            volumePoint.X -= virtualOffsetMatrix[0];
            volumePoint.Y -= virtualOffsetMatrix[1];
            //volumePoint.Z -= (long)virtualOffsetMatrix[2];

            x1 = volumePoint.X * inverse[0, 0] + volumePoint.Y * inverse[0, 1];
            x2 = volumePoint.X * inverse[1, 0] + volumePoint.Y * inverse[1, 1];

            volumePoint.X = x1;
            volumePoint.Y = x2;

            volumePoint.X += realOffsetMatrix[0];
            volumePoint.Y += realOffsetMatrix[1];
            //volumePoint.Z += (long)realOffsetMatrix[2];

            return volumePoint;
        }

        /// <summary>
        /// Returns the real position of the virtual room input vector
        /// </summary>
        /// <param name="volumePoint">the position in the virtual room</param>
        /// <returns>the position in the real room</returns>
        public System.Drawing.Point getRealPosition(System.Drawing.Point Point)
        {
            System.Drawing.Point output;

            IntPoint p = getRealPosition(new IntPoint(Point.X, Point.Y));

            output = new System.Drawing.Point((int)(p.X + 0.5), (int)(p.Y + 0.5));

            return output;
        }

        /// <summary>
        /// Returns the virtual position of the real room input vector
        /// </summary>
        /// <param name="volumePoint">the position in the virtual room</param>
        /// <returns>the position in the real room</returns>
        public System.Drawing.PointF getVirtualPosition(System.Drawing.PointF volumePoint)
        {
            double x1;
            double x2;

            volumePoint.X -= (long)realOffsetMatrix[0];
            volumePoint.Y -= (long)realOffsetMatrix[1];

            //fix for deserialization
            if (calculate == null)
                calculateVectorMatrix();

            x1 = volumePoint.X * calculate[0, 0] + volumePoint.Y * calculate[0, 1];
            x2 = volumePoint.X * calculate[1, 0] + volumePoint.Y * calculate[1, 1];

            volumePoint.X = (long)x1;
            volumePoint.Y = (long)x2;

            volumePoint.X += (long)virtualOffsetMatrix[0];
            volumePoint.Y += (long)virtualOffsetMatrix[1];

            return volumePoint;
        }

        /// <summary>
        /// Returns the virtual position of the real room input vector
        /// </summary>
        /// <param name="volumePoint">the position in the virtual room</param>
        /// <returns>the position in the real room</returns>
        public IntPoint getVirtualPosition(IntPoint volumePoint)
        {
            double x1;
            double x2;

            volumePoint.X -= (long)realOffsetMatrix[0];
            volumePoint.Y -= (long)realOffsetMatrix[1];
            volumePoint.Z -= (long)realOffsetMatrix[2];

            //fix for deserialization
            if (calculate == null)
                calculateVectorMatrix();

            x1 = volumePoint.X * calculate[0, 0] + volumePoint.Y * calculate[0, 1];
            x2 = volumePoint.X * calculate[1, 0] + volumePoint.Y * calculate[1, 1];

            volumePoint.X = (long)x1;
            volumePoint.Y = (long)x2;

            volumePoint.X += (long)virtualOffsetMatrix[0];
            volumePoint.Y += (long)virtualOffsetMatrix[1];
            volumePoint.Z += (long)virtualOffsetMatrix[2];

            return volumePoint;
        }

        /// <summary>
        /// Returns the virtual position of the real room input vector
        /// </summary>
        /// <param name="volumePoint">the position in the virtual room</param>
        /// <returns>the position in the real room</returns>
        public DoublePoint getVirtualPosition(DoublePoint volumePoint)
        {
            double x1;
            double x2;

            volumePoint.X -= realOffsetMatrix[0];
            volumePoint.Y -= realOffsetMatrix[1];
            //volumePoint.Z -= (long)realOffsetMatrix[2];

            //fix for deserialization
            if (calculate == null)
                calculateVectorMatrix();

            x1 = volumePoint.X * calculate[0, 0] + volumePoint.Y * calculate[0, 1];
            x2 = volumePoint.X * calculate[1, 0] + volumePoint.Y * calculate[1, 1];

            volumePoint.X = x1;
            volumePoint.Y = x2;

            volumePoint.X += virtualOffsetMatrix[0];
            volumePoint.Y += virtualOffsetMatrix[1];
            //volumePoint.Z += (long)virtualOffsetMatrix[2];

            return volumePoint;
        }

        /// <summary>
        /// Returns the virtual position of the real room input vector
        /// </summary>
        /// <param name="volumePoint">the position in the virtual room</param>
        /// <returns>the position in the real room</returns>
        public System.Drawing.Point getVirtualPosition(System.Drawing.Point Point)
        {
            System.Drawing.Point output;

            IntPoint p = getVirtualPosition(new IntPoint(Point.X, Point.Y));

            output = new System.Drawing.Point((int)Math.Round((double)p.X), (int)Math.Round((double)p.Y));

            return output;
        }
    }
}
