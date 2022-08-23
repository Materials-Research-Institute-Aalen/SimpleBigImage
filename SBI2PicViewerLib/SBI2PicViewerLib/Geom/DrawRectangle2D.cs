using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using Cornerstones.VectorMath;

namespace SBI2PicViewerLib.Geom
{
    class DrawRectangle2D : Rectangle2D
    {
        private System.Drawing.Color _color;
        //private VectorRoom _calculator;

        public DrawRectangle2D(double left, double top, double right, double bottom, System.Drawing.Color c)
            : base(left, top, right, bottom)
        {
            //_calculator = calculator;
            _color = c;
        }

        public DrawRectangle2D(float left, float top, float right, float bottom, System.Drawing.Color c)
            : base(left, top, right, bottom)
        {
            //_calculator = calculator;
            _color = c;
        }

        public void render()
        {
            if (_color == null)
            {
                return;
            }
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(PrimitiveType.Lines);

            GL.Color3(_color);

            double drawWidth = width;
            double drawHeight = height;

            double xPos = this.p.X - drawWidth / 2;
            double xPosWithWidth = this.p.X + drawWidth / 2;

            double yPos = this.p.Y - drawHeight / 2;
            double yPosWithHeight = this.p.Y + drawHeight / 2;

            GL.Vertex2(xPos, yPos);
            GL.Vertex2(xPos, yPosWithHeight);

            GL.Vertex2(xPos, yPosWithHeight);
            GL.Vertex2(xPosWithWidth, yPosWithHeight);

            GL.Vertex2(xPosWithWidth, yPosWithHeight);
            GL.Vertex2(xPosWithWidth, yPos);

            GL.Vertex2(xPosWithWidth, yPos);
            GL.Vertex2(xPos, yPos);

            GL.End();
        }
        
        public void resize(double left, double top, double right, double bottom)
        {
            this.position(left, top);

            width = (float)(right - left);
            height = (float)(bottom - top);
        }

    }
}
