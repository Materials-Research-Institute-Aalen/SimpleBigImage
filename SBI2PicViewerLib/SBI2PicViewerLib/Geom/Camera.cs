using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using SBI2PicViewerLib.Renderer;
using Cornerstones.Poly2DMath;

namespace SBI2PicViewerLib.Geom
{
    class Camera : Rectangle2D
    {
        Matrix4 ortho_projection;
        private static readonly float _dblZoomMinFactor = 0.02f;
        //private static int lastK = -1;

        public Camera(double left, double top, double right, double bottom) : base(left,top,right,bottom)
        {

        }

        public void addRotation(float f, float refPointX = -1, float refPointY = -1)
        {
            if (refPointX < 0 || refPointX > getWidth())
            {
                refPointX = getWidth() / 2;
            }
            if (refPointY < 0 || refPointX > getHeight())
            {
                refPointY = getHeight() / 2;
            }

            float[] Pos = fromOwnPointToReference(new float[] { refPointX, refPointY });
            rotate(getRotation() + f);
            putOwnPointOverReferencePoint(refPointX, refPointY, Pos[0], Pos[1]);
        }

        public void addPosition(double x, double y)
        {
            addPosition((float)x, (float)y);
        }

        public void addPosition(float x, float y)
        {
            translate(x, y);
        }

        public void resize(float x, float y)
        {
            ortho_projection = Matrix4.CreateOrthographicOffCenter(0, x, y, 0, -1, 1);
            this.height = y;
            this.width = x;
        }

        public void zoom(int iStep)
        {
            scaling /= (iStep != 0 ? iStep > 0 ? 1.3f : 1.0f / 1.3f : 1);
            if (scaling < _dblZoomMinFactor)
            {
                scaling = _dblZoomMinFactor;
            }
            recalculateMatrices();
        }

        public float getZoom()
        {
            return 1.0f/scaling;
        }

        public void render(GUIManagement GUI, TextureManagement textures, ContourManagement contours, List<DrawPolyline2D> mousePointer)
        {
            //26,35,41

            GL.ClearColor(SBIRenderContainer.BACKGROUND_RED / 255.0f, SBIRenderContainer.BACKGROUND_GREEN / 255.0f, SBIRenderContainer.BACKGROUND_BLUE / 255.0f, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            prepareMatrix();
            GL.Scale(getZoom(), getZoom(), 1);
            GL.Rotate(-getRotation(), 0, 0, 1);
            GL.Translate(-getXPos(), -getYPos(), 0);

            textures.render(this);
            if (contours != null)
            {
                contours.render(this, textures.CURRENTLY_RENDERING);
            }
            if (mousePointer != null)
            {
                foreach (DrawPolyline2D drawLine in mousePointer)
                {
                    drawLine.render(this.getZoom());
                }
            }

            GL.Translate(getXPos(), getYPos(), 0);
            GL.Rotate(-getRotation(), 0, 0, 1);
            GL.Scale(1 / getZoom(), 1 / getZoom(), 1);
            if (GUI != null)
            {
                //prepareMatrix();
                GUI.render(this);
            }
            unprepareMatrix();
        }
        
        private int setValueInside(int value, int iMax, int iMin)
        {
            if (value > iMax)
            {
                value = iMax;
            }
            if (value < iMin)
            {
                value = iMin;
            }
            return value;
        }
        
        public void prepareMatrix()
        {
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.MatrixMode(MatrixMode.Projection);

            GL.PushMatrix();//
            GL.LoadMatrix(ref ortho_projection);
        }

        public void unprepareMatrix()
        {
            GL.PopMatrix();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();
        }

    }
}
