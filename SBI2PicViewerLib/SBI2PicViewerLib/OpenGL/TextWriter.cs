﻿using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;


namespace SBI2PicViewerLib
{
    class TextWriter
    {
        private readonly Font TextFont = new Font(FontFamily.GenericSansSerif, 8);
        private readonly Bitmap TextBitmap;
        private List<PointF> _positions;
        private List<string> _lines;
        private List<Brush> _colours;
        private int _textureId;
        Matrix4 ortho_projection;
        //private Size _clientSize;

        public void resize(float x, float y)
        {
            ortho_projection = Matrix4.CreateOrthographicOffCenter(0, x, y, 0, -1, 1);
        }

        public void Update(int ind, string newText)
        {
            if (ind < _lines.Count)
            {
                _lines[ind] = newText;
                UpdateText();
            }
        }


        public TextWriter(Size areaSize)
        {
            _positions = new List<PointF>();
            _lines = new List<string>();
            _colours = new List<Brush>();

            TextBitmap = new Bitmap(areaSize.Width, areaSize.Height);
            //this._clientSize = ClientSize;
            _textureId = CreateTexture();

            resize(800, 600);
        }

        private int CreateTexture()
        {
            int textureId;
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.Replace);//Important, or wrong color on some computers
            Bitmap bitmap = TextBitmap;
            GL.GenTextures(1, out textureId);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.Finish();
            bitmap.UnlockBits(data);
            return textureId;
        }

        public void Dispose()
        {
            if (_textureId > 0)
                GL.DeleteTexture(_textureId);
        }

        public void Clear()
        {
            _lines.Clear();
            _positions.Clear();
            _colours.Clear();
        }

        public void AddLine(string s, PointF pos, Brush col)
        {
            _lines.Add(s);
            _positions.Add(pos);
            _colours.Add(col);
            UpdateText();
        }

        public void UpdateText()
        {

            /*
            if (_lines.Count > 0)
            {
                using (Graphics gfx = Graphics.FromImage(TextBitmap))
                {
                    gfx.Clear(Color.White);
                    gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    for (int i = 0; i < _lines.Count; i++)
                        gfx.DrawString(_lines[i], TextFont, _colours[i], _positions[i]);
                }

                System.Drawing.Imaging.BitmapData data = TextBitmap.LockBits(new Rectangle(0, 0, TextBitmap.Width, TextBitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, TextBitmap.Width, TextBitmap.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                TextBitmap.UnlockBits(data);
            }
            */
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

        public void Draw()
        {
            prepareMatrix();
            GL.BindTexture(TextureTarget.Texture2D, _textureId);


            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0, 0); GL.Vertex2(0, 0);
            GL.TexCoord2(1, 0); GL.Vertex2(TextBitmap.Width, 0);
            GL.TexCoord2(1, 1); GL.Vertex2(TextBitmap.Width, TextBitmap.Height);
            GL.TexCoord2(0, 1); GL.Vertex2(0, TextBitmap.Height);
            GL.End();
            unprepareMatrix();
        }
    }
}
