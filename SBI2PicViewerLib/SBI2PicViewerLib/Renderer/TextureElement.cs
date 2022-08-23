using OpenTK.Graphics.OpenGL;
using SBI2PicViewerLib.Geom;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBI2PicViewerLib.Renderer
{
    public class TextureElement : IDisposable
    {
        protected RectangleF _drawRect;
        protected int _iTileSize;
        protected int _textureID = -1;

        protected int _iDepth;

        //public List<DrawPolyline2D> POLYLINES;

        //ToDo: Sichtbarkeit an und ausschalten
        protected static readonly bool _blnVisibleCorners = false;

        protected TextureElement() { }

        public TextureElement(Bitmap input, int xPos, int yPos, float fSize, int iDepth)
        {
            //_TextureBitmap = input;
            _iDepth = iDepth;
            if (_blnVisibleCorners == true)
            {
                fSize -= 10;
            }

            _drawRect = new RectangleF(xPos, yPos, fSize, fSize);

            _iTileSize = input.Width;

            activateTexture(input);
        }

        //////////////////////////

        public void render()
        {
            float xStart, yStart, xEnd, yEnd;

            xStart = _drawRect.Left;
            yStart = _drawRect.Top;

            xEnd = _drawRect.Right;
            yEnd = _drawRect.Bottom;

            GL.BindTexture(TextureTarget.Texture2D, _textureID);
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(Color.White);

            addPoint(xStart, yStart, 0.0f, 0.0f);
            addPoint(xStart, yEnd,   0.0f, 1.0f);
            addPoint(xEnd,   yEnd,   1.0f, 1.0f);
            addPoint(xEnd,   yStart, 1.0f, 0.0f);

            GL.End();
        }

        protected void addPoint(float x, float y, float u, float v)
        {
            GL.TexCoord2(u, v);
            GL.Vertex2(x, y);
        }

        //////////////////////////

        private void generateImageTexture_nobackground(Bitmap input)
        {
            if (input == null)
            {
                return;
            }
            try
            {
                System.Drawing.Imaging.BitmapData TextureData = input.LockBits(
                    new System.Drawing.Rectangle(0, 0, input.Width, input.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb
                );
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, input.Width, input.Height, 0,
                        OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, TextureData.Scan0);
                input.UnlockBits(TextureData);
                
            }
            catch
            {
                throw new Exception("Unable to generate image data");
            }
        }

        private void generateImageTexture(Bitmap input)
        {
            if (input == null)
            {
                return;
            }
            try
            {
                
                using (Bitmap secondaryBitmap = new Bitmap(input.Width, input.Height, input.PixelFormat))
                {
                    
                    using (Graphics g = Graphics.FromImage(secondaryBitmap))
                    {
                        g.Clear(Color.FromArgb(255, SBIRenderContainer.BACKGROUND_RED, SBIRenderContainer.BACKGROUND_GREEN, SBIRenderContainer.BACKGROUND_BLUE));
                        // Draw using the color map
                        Rectangle rect = new Rectangle(0, 0, input.Width, input.Height);
                        g.DrawImage(input, rect, 0, 0, rect.Width, rect.Height, GraphicsUnit.Pixel);
                    }
                    
                    System.Drawing.Imaging.BitmapData TextureData = secondaryBitmap.LockBits(
                            new System.Drawing.Rectangle(0, 0, secondaryBitmap.Width, secondaryBitmap.Height),
                            System.Drawing.Imaging.ImageLockMode.ReadOnly,
                            System.Drawing.Imaging.PixelFormat.Format32bppArgb
                        );
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, secondaryBitmap.Width, secondaryBitmap.Height, 0,
                            OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, TextureData.Scan0);
                    secondaryBitmap.UnlockBits(TextureData);
                }
            }
            catch
            {
                throw new Exception("Unable to generate image data");
            }
        }

        private void prepareTexture()
        {
            GL.GenTextures(1, out _textureID);
            GL.BindTexture(TextureTarget.Texture2D, _textureID);

            if (!_blnVisibleCorners)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }
        }

        private void finishTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        protected void activateTexture(Bitmap input)
        {
            if (_textureID < 0)
            {
                prepareTexture();
                generateImageTexture(input);
                finishTexture();
            }
            else
            {
                throw new Exception("This should not happen.");
            }
        }

        public void deactivateTexture()
        {
            if (_textureID > -1)
            {
                GL.DeleteTexture(_textureID);
            }
            _textureID = -1;
        }

        //////////////////////////

        public bool isAt(int x, int y, int z)
        {
            bool blnIsAtX = _drawRect.X == x;
            bool blnIsAtY = _drawRect.Y == y;
            bool blnIsAtZ = _iDepth == z;
            return blnIsAtX && blnIsAtY && blnIsAtZ;
        }

        public void addToSize(ref int xMin, ref int xMax, ref int yMin, ref int yMax)
        {
            if (xMin == -1)
            {
                xMin = (int)_drawRect.Left;
                xMax = (int)_drawRect.Right;
                yMin = (int)_drawRect.Top;
                yMax = (int)_drawRect.Bottom;

                return;
            }

            xMin = _drawRect.Left < xMin ? (int)_drawRect.Left : xMin;
            xMax = _drawRect.Right > xMax ? (int)_drawRect.Right : xMax;
            yMin = _drawRect.Top < yMin ? (int)_drawRect.Top : yMin;
            yMax = _drawRect.Bottom > yMax ? (int)_drawRect.Bottom : yMax;
        }

        public bool GETS_RENDERED
        {
            get
            {
                return _textureID != -1;
            }
        }

        public RectangleF ROI
        {
            get
            {
                return _drawRect;
            }
        }

        public int DEPTH
        {
            get
            {
                return _iDepth;
            }
        }

        //////////////////////////

        public void Dispose()
        {
            //deactivateTexture();
        }

        ~TextureElement()
        {
            Dispose();
        }
    }
}
