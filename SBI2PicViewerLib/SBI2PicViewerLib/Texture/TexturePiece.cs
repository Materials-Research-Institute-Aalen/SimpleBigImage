using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SBI2PicViewerLib
{
    class TexturePiece
    {
        static System.Drawing.Bitmap emptyImage = null;
        public bool blnActive;
        Bitmap TextureBitmap;

        public int x;
        public int y;
        public int z;
        public int pixelX;
        public int pixelY;

        private string strImageIdentifier;

        private bool blnEmpty = false;
        private bool blnLoaded;

        private int texture;

        //ToDo: Hier "Sichtbarkeit" an und ausschalten
        public static readonly bool makeVisible = false;

        public static bool checkPieceExists(int xPos, int yPos, int depth)
        {
            int iStepSize = (int)Math.Pow(2, depth);
            if (iStepSize < 2 || (xPos % iStepSize == 0) && (yPos % iStepSize == 0))
            {
                return true;
            }
            return false;
        }

        private System.Drawing.Bitmap loadImage(string strName)
        {
            try
            {
                return new Bitmap(strName); ;
            }
            catch
            {
                return emptyImage;
            }
        }

        public TexturePiece(int xPos, int yPos, int depth)
        {
            if (emptyImage == null)
            {
                emptyImage = new System.Drawing.Bitmap(OpenCVVariables.Schrittweite, OpenCVVariables.Schrittweite);
            }

            x = xPos;
            y = yPos;
            z = depth;
            pixelX = x * OpenCVVariables.Schrittweite;
            pixelY = y * OpenCVVariables.Schrittweite;

            strImageIdentifier = OpenCVVariables.strFolder + @"\" + x.ToString() + @"\" + y.ToString() + @"\" + z.ToString() + @"\" + "texture.png";

            TextureBitmap = null;
            blnLoaded = false;
            blnActive = false;
            texture = 0;

            blnEmpty = false;
        }
        
        private void load()
        {
            if (blnEmpty)
            {
                TextureBitmap = emptyImage;
                return;
            }
            if (!blnLoaded)
            {
                if (strImageIdentifier != null)
                {
                    TextureBitmap = loadImage(strImageIdentifier);
                }
                else
                {
                    TextureBitmap = emptyImage;
                }
                blnLoaded = true;
            }
        }

        public void unload()
        {
            if (blnLoaded && !blnEmpty)
            {
                deactivateTexture();
                blnLoaded = false;
                if (TextureBitmap != emptyImage)
                {
                    TextureBitmap.Dispose();
                }
                TextureBitmap = null;
            }
        }

        public void checkLoaded()
        {
            if (blnEmpty)
            {
                return;
            }
            if (blnLoaded != blnActive)
            {
                if (blnActive)
                {
                    load();
                }
                else
                {
                    unload();
                }
            }
        }

        private void addPoint(float x, float y, float u, float v)
        {
            GL.TexCoord2(u, v);
            GL.Vertex2(x, y);
        }

        public bool render(Geom.Camera cam, float fSize)
        {
            fSize = fSize * OpenCVVariables.Schrittweite;

            if (makeVisible == true)
            {
                fSize -= 10;
            }

            float xPos = pixelX;
            float yPos = pixelY;

            if (cam.doesOverlap(pixelX, pixelY, fSize, fSize))
            {
                activateTexture();
                GL.BindTexture(TextureTarget.Texture2D, texture);
                GL.Begin(PrimitiveType.Quads);
                GL.Color3(Color.White);

                addPoint(xPos,
                         yPos,
                         0f, 0.0f);
                addPoint(xPos,
                         fSize + yPos,
                         0.0f, 1.0f);
                addPoint(fSize + xPos,
                         fSize + yPos,
                         1.0f, 1.0f);
                addPoint(fSize + xPos,
                         yPos,
                         1.0f, 0.0f);

                GL.End();
                return true;
            }
            else
            {
                unload();
                return false;
            }
        }

        private void activateTexture()
        {
            if (texture < 1)
            {
                load();
                prepareGenerateTexture();
                generateImageTexture();
                endGenerateTexture();
            }
        }

        private void deactivateTexture()
        {
            if (texture > 0)
            {
                GL.DeleteTexture(texture);
            }
            texture = 0;
        }

        private void prepareGenerateTexture()
        {
            GL.GenTextures(1, out texture);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            if (!makeVisible)
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

        private void generateImageTexture()
        {
            if (TextureBitmap == null)
            {
                return;
            }
            try
            {
                System.Drawing.Imaging.BitmapData TextureData = TextureBitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, OpenCVVariables.Schrittweite, OpenCVVariables.Schrittweite),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb
                );
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, TextureBitmap.Width, TextureBitmap.Height, 0,
                        OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, TextureData.Scan0);
                //free the bitmap data (we dont need it anymore because it has been passed to the OpenGL driver
                TextureBitmap.UnlockBits(TextureData);
            }
            catch
            {
                System.Drawing.Imaging.BitmapData TextureData = emptyImage.LockBits(
                    new System.Drawing.Rectangle(0, 0, OpenCVVariables.Schrittweite, OpenCVVariables.Schrittweite),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb
                );
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, TextureBitmap.Width, TextureBitmap.Height, 0,
                        OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, TextureData.Scan0);
                //free the bitmap data (we dont need it anymore because it has been passed to the OpenGL driver
                TextureBitmap.UnlockBits(TextureData);
            }
        }

        private void endGenerateTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Dispose()
        {
            deactivateTexture();
        }

        ~TexturePiece()
        {
            if (blnEmpty)
            {
                return;
            }

            if (TextureBitmap != null)
            {
                TextureBitmap.Dispose();
                TextureBitmap = null;
            }
        }
    }
}