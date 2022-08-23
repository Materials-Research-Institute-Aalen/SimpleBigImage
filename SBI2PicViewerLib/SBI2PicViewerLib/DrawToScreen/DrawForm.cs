using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing;
using System.Windows.Forms;

namespace SBI2PicViewerLib.DrawToScreen
{
    class DrawForm
    {
        Form drawForm;
        Form parentForm;
        
        System.Drawing.Image img;
        bool visible;

        int xOff;
        int yOff;

        int iW;
        int iH;

        public DrawForm(System.Drawing.Image toDraw, Form bindTo, int xOffset, int yOffset, int iWidth, int iHeight)
        {
            visible = true;

            bindTo.Move += bindTo_Move;
            bindTo.FormClosing += bindTo_FormClosing;
            bindTo.Resize += bindTo_Resize;
            bindTo.Load += bindTo_Load;
            bindTo.LostFocus += bindTo_LostFocus;
            bindTo.GotFocus += bindTo_GotFocus;

            parentForm = bindTo;
            drawForm = new Form();

            xOff = xOffset;
            yOff = yOffset;
            iW = iWidth;
            iH = iHeight;
            img = toDraw;

            drawForm.BackColor = Color.Pink;
            drawForm.TransparencyKey = Color.White;
            drawForm.BackgroundImage = toDraw;
            drawForm.BackgroundImageLayout = ImageLayout.Zoom;
            drawForm.FormBorderStyle = FormBorderStyle.None;
            drawForm.Top = bindTo.Top + xOff;
            drawForm.Left = bindTo.Left + yOff;
            drawForm.Width = iWidth;
            drawForm.Height = iHeight;
            drawForm.TopMost = true;


            Application.EnableVisualStyles();
            drawForm.Show();
            update();
        }

        void bindTo_GotFocus(object sender, EventArgs e)
        {
            update();
        }

        void bindTo_LostFocus(object sender, EventArgs e)
        {
            if (drawForm.Focused)
            {
                return;
            }
            update();
        }

        public void centerOn(PictureBox p)
        {
            int iWidth = p.Width;
            int iHeight = p.Height;
            int iXOffset = iWidth / 2;
            int iYOffset = iHeight / 2;
            iXOffset -= iW / 2;
            iYOffset -= iH / 2;

            iXOffset += p.Left;
            iYOffset += p.Top;
        }

        public bool visibility
        {
            get
            {
                return visible;
            }
            set
            {
                visible = value;
                update();
            }
        }

        public void setSize(int iWidth, int iHeight)
        {
            iW = iWidth;
            iH = iHeight;
            update();
        }

        public void setOffset(int iXOffset, int iYOffset)
        {
            xOff = iXOffset;
            yOff = iYOffset;
            update();
        }

        public void setImage(System.Drawing.Image toDraw)
        {
            /*
            if (toDraw != null)
            {
                toDraw.Dispose();
                toDraw = null;
            }
            drawForm.BackgroundImage = toDraw;
            img = toDraw;
            update();
            */

            if (toDraw != null)
            {
                drawForm.BackgroundImage.Dispose();
                drawForm.BackgroundImage = null;
                img.Dispose();
                img = null;
            }
            drawForm.BackgroundImage = toDraw;
            img = toDraw;
            update();
        }

        public void update()
        {
            drawForm.Visible = ((parentForm.Width > (xOff + iW)) && (parentForm.Height > (yOff + iH)) && visible);

            drawForm.Left = parentForm.Left + xOff;
            drawForm.Top = parentForm.Top + yOff;
            drawForm.TopMost = true;

            drawForm.Width = iW;
            drawForm.Height = iH;
            drawForm.Update();
        }

        void bindTo_Load(object sender, EventArgs e)
        {
            try
            {
                update();
            }
            catch { };
        }

        void bindTo_Resize(object sender, EventArgs e)
        {
            try
            {
                update();
            }
            catch { };
        }

        public void bindTo_Move(object sender, EventArgs e)
        {
            try
            {
                update();
            }
            catch { };
        }

        void bindTo_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                drawForm.Close();
                drawForm.Dispose();
                drawForm = null;
            }
            catch { };
        }
    }
}
