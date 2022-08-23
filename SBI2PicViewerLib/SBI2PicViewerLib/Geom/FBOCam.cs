using System;
using System.Collections.Generic;
using System.Text;


using OpenTK;
//using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace SBI2PicViewerLib.Geom
{
    /*
    class FBOCam : Camera
    {
        //Rendert das Ganze auf eine Textur, wenn der Grafikram mindestens 3*Texturgröße ist.
        int iTextureWidth;
        int iTextureHeight;

        public uint ColorTexture;
        public uint DepthTexture;
        public uint FBOHandle;

        public FBOCam(Camera sourceCam, float xStart, float yStart, float xEnd, float yEnd, float fResultPicWidth, float fResultPicHeight)
            : base(xStart, yStart, 0, 0)
        {
            iTextureHeight = (int)fResultPicHeight;
            iTextureWidth = (int)fResultPicWidth;

            rotate(sourceCam.getRotation());
            //Set scaling
            //scaling = cam.scaling / cam.width * this.width
            //set Position
            //position = sourcecam.ownPositionToReference(xStart,yStart)
            //set width/height
            //endPosition = sourcecam.ownPositionToReference(xEnd,yEnd)
            //width = (xEnd - xStart) * cam.scaling / this.scaling
            //height = (yEnd - yStart) * cam.scaling / this.scaling

            // Create Color Tex
            GL.GenTextures(1, out ColorTexture);
            GL.BindTexture(TextureTarget.Texture2D, ColorTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, iTextureWidth, iTextureHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

            // Create a FBO and attach the textures
            GL.Ext.GenFramebuffers(1, out FBOHandle);
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, FBOHandle);
            GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, ColorTexture, 0);
            //GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachmentExt, TextureTarget.Texture2D, DepthTexture, 0);
        }

        public void preRender()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); //Einfach mal das Bild aufräumen, sauber machen

            GL.PushAttrib(AttribMask.ViewportBit);
            {
                GL.Viewport(0, 0, iTextureWidth, iTextureHeight);

                // clear the screen in red, to make it very obvious what the clear affected. only the FBO, not the real framebuffer
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.ClearColor(1f, 0f, 0f, 0f);
            }
        }

        new public void render(TexturePiece[, ,] tpList)
        {
            preRender();
            base.render(tpList);
            postRender();
        }

        public void postRender()
        {
            GL.PopAttrib();
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0); // disable rendering into the FBO

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); //Einfach mal das Bild aufräumen, sauber machen
        }

        public void dispose()
        {
            GL.DeleteTextures(1, ref ColorTexture);
            GL.DeleteTextures(1, ref DepthTexture);
            GL.Ext.DeleteFramebuffers(1, ref FBOHandle);
        }
    }
    */
}
