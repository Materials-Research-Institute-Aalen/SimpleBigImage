using Cornerstones.Poly2DMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;
using SBI2PicViewerLib.Geom;


namespace SBI2PicViewerLib.Renderer
{
    public enum GUIRenderType
    {
        SCALEBAR,
        HUD
    }

    public class GUIElementInterface
    {
        GUIElement _element;
        GUIRenderType _type;

        public GUIElementInterface(GUIRenderType type, Bitmap input, int xPos, int yPos, double fSize, int width = 0, int height = 0)
        {
            _element = new GUIElement(input, xPos,yPos,fSize,width,height);
            _type = type;
        }

        //////////////////////////

        public void render(Object cam)
        {
            if (cam.GetType() == typeof(Camera))
            {
                Camera camera = (Camera)cam;
                _element.render(camera, _type);
            }
            else
            {
                throw new Exception("render() needs Camera object, not some random stuff");
            }
        }

    }


    class GUIElement : TextureElement
    {
    
        public GUIElement(Bitmap input, int xPos, int yPos, double fSize, int width = 0, int height = 0)
        {
            _iDepth = 0;
            _drawRect = new RectangleF(xPos, yPos, (float)(fSize * width), (float)(fSize * height));
            activateTexture(input);
        }

        //////////////////////////

        new public void render()
        {
            throw new NotImplementedException();
        }

        public void render(Camera cam, GUIRenderType type)
        {
            float xStart, yStart, xEnd, yEnd;

            xStart = _drawRect.Left;
            yStart = _drawRect.Top;

            xEnd = _drawRect.Right;
            yEnd = _drawRect.Bottom;

            float width = cam.getWidth();
            float height = cam.getHeight();

            bool blnInverseX = xStart < 0 && _drawRect.Width < 0;
            bool blnInverseY = yStart < 0  && _drawRect.Height < 0;

            if (blnInverseX)
            {
                float newXStart = width + xEnd;

                float newXEnd = width + xStart;

                xStart = newXStart;

                xEnd = newXEnd;
            }
            if (blnInverseY)
            {
                float newYStart = height + yEnd;

                float newYEnd = height + yStart;

                yStart = newYStart;

                yEnd = newYEnd;
            }

            if (type == GUIRenderType.SCALEBAR)
            {
                float newWidth = (xEnd - xStart) * cam.getZoom();
                float newHeight = (yEnd - yStart) * cam.getZoom();

                if (blnInverseX)
                {
                    xStart = xEnd - newWidth;
                }
                else
                {
                    xEnd = xStart + newWidth;
                }
                if (blnInverseY)
                {
                    yStart = yEnd - newHeight;
                }
                else
                {
                    yEnd = yStart + newHeight;
                }
            }

            GL.BindTexture(TextureTarget.Texture2D, _textureID);
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(Color.White);

            addPoint(xStart, yStart, 0.0f, 0.0f);
            addPoint(xStart, yEnd,   0.0f, 1.0f);
            addPoint(xEnd,   yEnd,   1.0f, 1.0f);
            addPoint(xEnd,   yStart, 1.0f, 0.0f);

            GL.End();
        }

    }
}
