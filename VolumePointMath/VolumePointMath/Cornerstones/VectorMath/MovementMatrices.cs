using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cornerstones.VectorMath
{
    /// <summary>
    /// The Movement Matrices
    /// </summary>
    public class MovementMatrices
    {
        /// <summary>
        /// Creates a rotation matrix for angle in X direction
        /// </summary>
        /// <param name="angle">The rotation</param>
        /// <returns>A Matrix for 3D Rotations</returns>
        public static Matrix GetRotationMatrixX(double angle)
        {
            if (angle == 0.0)
            {
                return Matrix.IdentityMatrix(4,4);
            }
            float sin = (float)Math.Sin(angle);
            float cos = (float)Math.Cos(angle);

            Matrix m = new Matrix(4, 4);

            m.mat = new double[4, 4] {
            { 1.0f, 0.0f, 0.0f, 0.0f }, 
            { 0.0f, cos, -sin, 0.0f }, 
            { 0.0f, sin, cos, 0.0f }, 
            { 0.0f, 0.0f, 0.0f, 1.0f } };
            return m;
        }

        /// <summary>
        /// Creates a rotation matrix for angle in Y direction
        /// </summary>
        /// <param name="angle">The rotation</param>
        /// <returns>A Matrix for 3D Rotations</returns>
        public static Matrix GetRotationMatrixY(double angle)
        {
            if (angle == 0.0)
            {
                return Matrix.IdentityMatrix(4, 4); ;
            }
            float sin = (float)Math.Sin(angle);
            float cos = (float)Math.Cos(angle);

            Matrix m = new Matrix(4, 4);

            m.mat = new double[4, 4] {
            { cos, 0.0f, sin, 0.0f }, 
            { 0.0f, 1.0f, 0.0f, 0.0f }, 
            { -sin, 0.0f, cos, 0.0f }, 
            { 0.0f, 0.0f, 0.0f, 1.0f } };
            return m;
        }

        /// <summary>
        /// Creates a rotation matrix for angle in Z direction
        /// </summary>
        /// <param name="angle">The rotation</param>
        /// <returns>A Matrix for 3D Rotations</returns>
        public static Matrix GetRotationMatrixZ(double angle)
        {
            if (angle == 0.0)
            {
                return Matrix.IdentityMatrix(4,4);
            }
            float sin = (float)Math.Sin(angle);
            float cos = (float)Math.Cos(angle);
            Matrix m = new Matrix(4, 4);

            m.mat = new double[4, 4] {
            { cos, -sin, 0.0f, 0.0f }, 
            { sin, cos, 0.0f, 0.0f }, 
            { 0.0f, 0.0f, 1.0f, 0.0f }, 
            { 0.0f, 0.0f, 0.0f, 1.0f } };
            return m;
        }

        /// <summary>
        /// Creates a rotation matrix for angle in all three directions. X Rotation first, Y Rotation second, Z Rotation third
        /// </summary>
        /// <param name="ax">x rotation</param>
        /// <param name="ay">y rotation</param>
        /// <param name="az">z rotation</param>
        /// <returns></returns>
        public static Matrix GetRotationMatrix(double ax, double ay, double az)
        {
            Matrix my = null;
            Matrix mz = null;
            Matrix result = null;
            if (ax != 0.0)
            {
                result = GetRotationMatrixX(ax);
            }
            if (ay != 0.0)
            {
                my = GetRotationMatrixY(ay);
            }
            if (az != 0.0)
            {
                mz = GetRotationMatrixZ(az);
            }
            if (my != null)
            {
                if (result != null)
                {
                    result *= my;
                }
                else
                {
                    result = my;
                }
            }
            if (mz != null)
            {
                if (result != null)
                {
                    result *= mz;
                }
                else
                {
                    result = mz;
                }
            }
            if (result != null)
            {
                return result;
            }
            else
            {
                return Matrix.IdentityMatrix(4,4);
            }
        }

        /// <summary>
        /// Creates a matrix which translates points
        /// </summary>
        /// <param name="x">The movement in x direction</param>
        /// <param name="y">The movement in y direction</param>
        /// <param name="z">The movement in z direction</param>
        /// <returns>A Movement Matrix</returns>
        public static Matrix GetTranslationMatrix(double x, double y, double z)
        {
            Matrix m = new Matrix(4, 4);

            m.mat = new double[4, 4] {
            { 1, 0, 0, x }, 
            { 0, 1, 0, y }, 
            { 0, 0, 1, z }, 
            { 0, 0, 0, 1 } };
            return m;
        }

        /// <summary>
        /// Creates a matrix that can scale a list of points
        /// </summary>
        /// <param name="x">scaling in x direction</param>
        /// <param name="y">scaling in y direction</param>
        /// <param name="z">scaling in z direction</param>
        /// <returns>a matrix</returns>
        public static Matrix GetScalingMatrix(double x = 1, double y = 1, double z = 1)
        {
            Matrix m = new Matrix(4, 4);

            m.mat = new double[4, 4] {
            { x, 0, 0, 0 }, 
            { 0, y, 0, 0 }, 
            { 0, 0, z, 0 }, 
            { 0, 0, 0, 1 } };
            return m;
        }

    }
}
